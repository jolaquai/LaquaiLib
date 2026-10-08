# FullAccessProxyGenerator: supporting inaccessible types

## The caveat being removed

`FullAccessProxyGenerator.cs` emitted this into every generated proxy's doc comment:

> Members with a result type (method return type or declared type for fields, events or properties) that is not public cannot be proxied.

That sentence conflated two unrelated problems, and was wrong about the first one.

### Problem A: the accessibility check was stricter than the language

`IsPubliclyAccessible` rejected anything whose declared accessibility was not `Public`. But the generated proxy is compiled *into the consuming assembly*, so an `internal` type declared in that same assembly is perfectly nameable, as is any `internal` type reached through `InternalsVisibleTo`. Every such member was being silently dropped for no reason.

### Problem B: genuinely un-nameable types

Private/protected nested types, and cross-assembly `internal` types without IVT, cannot be spelled in the consuming assembly at all. Since `[UnsafeAccessor]` signature matching is **exact and includes the return type**, erasing them to `object` does not bind - the runtime refuses to find the target. Before .NET 10 this was a hard wall.

## The enabler

.NET 10 added `System.Runtime.CompilerServices.UnsafeAccessorTypeAttribute(string typeName)`, valid on parameters and return values. It decouples the *signature placeholder* from the *type identity*:

```csharp
[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Compute")]
[return: UnsafeAccessorType("Ns.Owner+Hidden, TheAsm")]
extern static object Compute([UnsafeAccessorType("Ns.Owner, TheAsm")] object target);
```

Placeholders: `object` for reference types, `ref`/`in`/`out object` for byrefs, `void*` for pointers. The name string uses `Type.GetType` syntax.

### What the runtime refuses

| Case | Result |
| --- | --- |
| Inaccessible **value type** | `NotSupportedException` at first call |
| Erased **byref return** | `NotSupportedException` at first call |
| `UnsafeAccessorKind.Field` with an erased type | Blocked, because field accessors return `ref T` |
| Function pointer types | Not representable in the name syntax |

Every failure is a **runtime** failure at first call, never a compile error. That asymmetry drives the whole design: erasure is opt-in, and everything the runtime would reject is skipped at generation time with a diagnostic instead.

## Decisions

**One generator, not two.** A second `[Generator]` on the same attribute was considered and rejected. The accessible/inaccessible decision is per-*member*, not per-type, so there is no clean ownership split; both generators would emit into the same partial class but only one can own `_instance`, `Instance`, the constructor, `Create` and `Accessors`; Roslyn provides no ordering and no cross-generator output visibility; and since the transform *is* the entire codegen, a second `ForAttributeWithMetadataName` subscription re-runs full emission per proxy per keystroke.

**No sub-proxy wrapper types.** The erased surface is plain `object` in both the accessor and the forwarder. Callers pass that `object` straight back into any other erased member expecting the same type. Generating a `HiddenProxy` wrapper per inaccessible type buys nothing that `object` does not already provide, and costs a recursive, cycle-prone type-graph walk.

**Erasure is opt-in; the accessibility fix is not.** Problem A is a bug fix whose failures all remain compile-time, so it always applies. Problem B changes the generated public shape (members start appearing typed `object`) and converts compile-time errors into first-call `TypeLoadException`s, so it is gated behind `[FullAccessProxy(..., IncludeInaccessible = true)]`.

**Decide once, emit from the decision.** The pre-existing defect was that `IsPubliclyAccessible` was consulted three times per member at three layers that all had to agree: dispatch, accessor emission, and again inside each individual writer. Any divergence emits an extern with no forwarder or vice versa. Erasure would have added a fourth axis to all ten call sites.

## Design

```csharp
enum TypeRefKind { Nameable, Erased, Unsupported }

readonly struct TypeRef
{
    TypeRefKind Kind;
    string Text;          // "global::Ns.Foo" | "object" | "void*"
    string MetadataName;  // non-null iff Erased
}
```

Two consumers, not two strategies:

- the **extern** emitter writes `Text`, plus an inline `[UnsafeAccessorType(MetadataName)]` when `MetadataName` is set
- the **forwarder** emitter writes `Text` only

`Unsupported` centralises the "can this member exist at all" verdict.

A `MemberPlan` is computed once per member (result `TypeRef`, target `TypeRef`, parameter `TypeRef`s, accessor name, emitted accessibility, skip reason). Writers consume plans and never re-decide.

Deliberately **not** built: an `ITypeReferenceRenderer` interface with two implementations, or separate collector/analysis/emitter classes. Those are private statics with one caller each. The behavioural difference is a `bool allowErasure` field, not a strategy.

## Phases

### Phase 0 - collapse the triple check

Pure refactor. Replace the three scattered `IsPubliclyAccessible` consultations with a single per-member `MemberPlan`. Zero behaviour change; output asserted byte-identical against the existing emission tests. Load-bearing: without it, phases 1 and 3 each have to touch ten sites and will drift.

### Phase 1 - real accessibility

Swap the leaf predicate to `compilation.IsSymbolAccessibleWithin(type, compilation.Assembly)`, which handles `InternalsVisibleTo` for free. Keep the existing recursion over type arguments, array element types and pointed-at types, because `IsSymbolAccessibleWithin` does not inspect the type arguments of a constructed generic.

Because a member may now mention a type that is accessible but not public, emitted members are **clamped** to `internal` when any type in their signature is not effectively public (CS0050). The proxy class's own accessibility cannot be clamped - partial declarations must agree on modifiers (CS0262) - so a public proxy class over an internal proxied type ends up with no public surface, and that gets a **FAP002** warning.

### Phase 2 - metadata name builder

`MetadataTypeName.TryBuild(ITypeSymbol, IAssemblySymbol) -> string`, returning `null` for anything unrepresentable. Pure function, its own file, its own direct unit tests, no generator wiring. Every bug in it is a runtime `TypeLoadException` that a compile-only test can never see, which is exactly why it is tested in isolation.

Rules: `Ns.Outer+Inner` with Roslyn `MetadataName` per segment (backtick arity); `[[arg],[arg]]` outermost-first for constructed generics with each argument independently assembly-qualified; `!N` / `!!N` for class / method type parameters; `[]`, `[,]`, `[][]` for arrays; `*` for pointers; `System.Object` for `dynamic`; `null` for function pointers and error types. Assembly qualification uses `ContainingAssembly.Identity.Name` (simple name only - baking `Version=`/`PublicKeyToken=` in would be a future runtime break) and is omitted for same-assembly types and for special types.

The array recursion is deliberately *not* reversed: C# `int[,][]` has `Rank == 2` and `ElementType == int[]`, and the naive recursion yields `System.Int32[][,]`, which is precisely reflection's spelling for that type.

### Phase 3 - wire erasure

Behind `IncludeInaccessible`. In scope: methods, property accessors, constructors, and the accessor target parameter, where the inaccessible type is a **reference type**.

Out of scope by construction, each skipped with a **FAP003** info diagnostic naming the reason:

- fields (`UnsafeAccessorKind.Field` returns `ref T`, and erased byref returns are refused)
- anything involving an inaccessible **value type** (private nested structs and enums are common in exactly this scenario)
- ref-returning members
- interfaces - explicit implementations require spelling `IFace.Member`, and there is no `object` placeholder position, so interfaces benefit only from Phase 1

**Overload collapse** is the sharp edge. `Foo(PrivateA)` and `Foo(PrivateB)` both erase to `Foo(object)` - CS0111, in `Accessors` *and* in the public forwarders. `MakeMemberKey` keys on the pre-erasure signature so it cannot catch this.

The original design had the externs escape via `[UnsafeAccessor(Kind, Name = "Foo")]`, which decouples the C# name from the target name, emitting `Foo`, `Foo__2`, ... That was dropped as unreachable: the accessor key (name, arity, *target type*, erased parameter signature) is a strict refinement of the forwarder key (name, arity, erased parameter signature), so an accessor collision implies a forwarder collision. A surviving `Foo__2` extern would have no forwarder able to call it and would be dead code inside a `private static class`. `ResolveCollisions` therefore drops both sides on an accessor collision, and drops only the forwarder on a forwarder-only collision. Either way it is first-wins plus FAP003 for the rest.

Ordering is made deterministic with an explicit ordinal sort over `MakeMemberKey`, because the emitted model string *is* the incremental cache key and "first wins" must not mean "whichever one Roslyn enumerated first". Constructors get their own sort over the parameter signature, since they all share the emitted name `ProxyCtor` and are distinguished by nothing else.

Constructors are the worst case here, since every constructor already shares the single C# name `ProxyCtor` and is distinguished only by its parameter list.

Cross-assembly erasure needs a real second assembly to be tested at all, and the shape that assembly must have is heavily constrained. A `private` or IVT-less `internal` *member* of a referenced assembly is never imported into the symbol table under Roslyn's default `MetadataImportOptions.Public`, so the generator cannot see it to erase it. Granting `InternalsVisibleTo` makes the type directly nameable, so no erasure happens. Declaring the exposing member `public` while the exposed type is less accessible is illegal C# (CS0050/51/53). The one constructible shape is a **`public` nested type inside an `internal` top-level container with no IVT**: the declared accessibility satisfies CS0050 for public exposing members, while the effective accessibility is still bounded by the container, so erasure is still required. `LaquaiLib.Generators.RuntimeTests.External` is built entirely around that shape.

### Phase 4 - runtime fixtures

Mandatory, not optional: every phase-3 failure mode is a first-call runtime exception that no compile-time test can observe. Minimum coverage in `LaquaiLib.Generators.RuntimeTests`: private nested reference type as a return; as a parameter; a member declared on an inaccessible base; a constructor taking an inaccessible parameter; an internal proxied type reached from a public proxy class (pins the clamping); an erased-overload collision; and same-assembly vs cross-assembly metadata names.

## Diagnostics

| Id | Severity | Meaning |
| --- | --- | --- |
| FAP001 | Error | Type named in the attribute could not be resolved; no source emitted |
| FAP002 | Warning | Proxy members clamped below the proxy class's declared accessibility |
| FAP003 | Info | Member skipped, with the reason (value type, byref return, field, overload collision, unrepresentable type) |
| FAP004 | Error | The proxied type itself cannot be named and `IncludeInaccessible` was not set; no source emitted |

FAP003 is only reported when `IncludeInaccessible` is set. Proxies that did not opt in stay exactly as silent as they were before, so the change is invisible to existing consumers.

`ProxyModel` carries `EquatableArray<ProxyDiagnosticInfo>` rather than a single optional diagnostic, with the location stored once and each entry reduced to a descriptor id plus message arguments. `RegisterSourceOutput` rebuilds the `Diagnostic`. Keeping the model a pure value type is what preserves incremental caching.

## Fallout fixed along the way

Two defects surfaced only once real fixtures existed. Neither is caused by erasure; both were latent.

**`UnsafeAccessorValidators` had no idea `[UnsafeAccessorType]` exists.** It compared the declared (now erased) `object` parameter and return types against the real target signature by strict symbol equality and reported LAQ9001/LAQ9003 on every correctly erased accessor. Since an erased signature deliberately does not match its target, the analyzer cannot validate it at all; `AnalyzeMethodDeclaration` now bails out for any accessor carrying the attribute on its return value or any parameter.

**The same analyzer could never match a generic target method.** Both the Roslyn and the reflection matching paths compared type-parameter-typed parameters by identity, and an `ITypeParameterSymbol` (or a `Type` with `IsGenericParameter`) from the target's declaration is never equal to the accessor's own. Overload selection silently fell through to the wrong overload, and the following arity check then reported a bogus LAQ9008. The Roslyn path now re-constructs same-arity candidates over the accessor's type parameters before comparing; the reflection path matches generic parameters by ordinal plus declaring kind and filters candidates by arity first; and `TypeParametersEqual` for two symbol arrays became a structural comparison instead of a never-true `SymbolEqualityComparer` sequence check.

**Forwarders hid `object` members without `new`.** `ToString`, `Equals(object)`, `GetHashCode` and `GetType` are collected from the proxied type (the walk stops before `System.Object`, but an override declared on the proxied type is still its own member) and were emitted as plain `public`, producing CS0114 on the proxy. They now carry `new`.

**Nested type declarations were being treated as proxyable members.** `INamedTypeSymbol.GetMembers()` returns nested types alongside real members. They fell through `PlanMember`'s `default` arm to an unsupported `TypeRef`, so every private nested type used by an erasure fixture also produced a nonsensical FAP003 claiming its own result type could not be referenced. `GetProxyableMembers` now filters `INamedTypeSymbol` outright.

## Known limitation, accepted

`ForAttributeWithMetadataName` caches per **syntax tree**. If the proxy declaration's file is unchanged but the *proxied type's* file changes, the transform does not re-run and the IDE shows a stale proxy. This generator is unusually exposed because reading another type's members is its entire job, and erasure deepens the dependency further - it now also depends on the shape of types it never names.

The only fix is `CompilationProvider`, which would rebuild every proxy on every keystroke. Not worth it. Documented, not fixed.
