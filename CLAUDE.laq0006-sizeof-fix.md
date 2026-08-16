# CLAUDE.laq0006-sizeof-fix

Plan for repairing `Helpers.SizeOf` and `UseAllocateUninitializedArrayAnalyzer` (LAQ0006).

## Files in scope

| Path | Role |
| --- | --- |
| `netstandard2.0/LaquaiLib.Analyzers/Helpers.cs` | `SizeOf` + array-size extraction |
| `netstandard2.0/LaquaiLib.Analyzers/Performance (0XXX)/UseAllocateUninitializedArrayAnalyzer.cs` | LAQ0006 analyzer |
| `netstandard2.0/LaquaiLib.Analyzers.Fixes/Performance/UseAllocateUninitializedArrayAnalyzerFix.cs` | code fix (read-only, must not break) |
| `TestProjects/LaquaiLib.Analyzers.Tests/LaquaiLib.Analyzers.Tests.csproj` | test project to extend |

## Original defect

`Helpers.SizeOf`'s fallback did `Type.GetType(sym.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))` and `Unsafe.SizeOf<T>` via `MakeGenericMethod`. That resolves against the analyzer HOST process, not the user's compilation. Two failure modes:

1. Any user struct sharing a name with a host-loaded type returns the wrong size.
2. Otherwise returns -1 and the analyzer bails.

**Verified: the fallback was 100 percent dead.** `FullyQualifiedFormat` emits a `global::` prefix. Ran a console app: `Type.GetType("global::System.TimeSpan")` returns NULL, `Type.GetType("System.TimeSpan")` returns OK. Both `Type.GetType` calls used `FullyQualifiedFormat`, so both always returned null, `MakeGenericMethod(null)` always threw, `SizeOf` always returned -1. LAQ0006 only ever fired for the hardcoded primitive / `Half` / `Vector2..4` list.

## Ground truth: the runtime fast path

Verbatim from `dotnet/runtime` `src/coreclr/System.Private.CoreLib/src/System/GC.CoreCLR.cs` (main):

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static unsafe T[] AllocateUninitializedArray<T>(int length, bool pinned = false)
{
    if (!pinned)
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            return new T[length];
        }
#if !DEBUG
        // small arrays are allocated using `new[]` as that is generally faster.
        if (length < 2048 / sizeof(T))
        {
            return new T[length];
        }
#endif
    }
    return AllocateNewArrayWorker(length, pinned);
}
```

Consequences the analyzer must honour:

- Report only when the element type is unmanaged. Ref-containing element types unconditionally fall back to `new T[length]`.
- The bail is `length < 2048 / sizeof(T)`, **integer division on element size**. It is not a `totalBytes >= 2048` comparison.
- When `sizeOf >= 2048` the threshold is 0, so the length need not be a compile-time constant.
- Underestimating `sizeOf` raises the threshold, so it only ever costs a suggestion. Overestimating produces a false positive. **`SizeOf` must return a lower bound or -1, never an overestimate.**

## Verified: reference assemblies poison the field walk

Dumped `Microsoft.NETCore.App.Ref` with `System.Reflection.Metadata`. GenAPI deletes concrete-typed private fields from structs and INJECTS a fabricated `private int _dummyPrimitive` (sig `06 08`, FIELD / ELEMENT_TYPE_I4).

Poisoned (fields reduce to `[_dummyPrimitive]`): `Guid`, `Half`, `DateTime`, `TimeSpan`, `DateOnly`, `TimeOnly`, `DateTimeOffset`, `Decimal`, `Index`, `Range`, `Complex`, `Vector64<T>`, `Vector128<T>`, `Vector256<T>`, `Vector512<T>`.

Clean (public fields survive, no dummy): `Vector2`, `Vector3`, `Vector4`, `Quaternion`, `Plane`, `Matrix3x2`, `Matrix4x4`, `ValueTuple\`2`.

Mixed: `Nullable\`1` `[value, _dummyPrimitive]`, `KeyValuePair\`2` `[key, value, _dummyPrimitive]`, `Span\`1` `[_dummy(object), _dummyPrimitive]`.

Real false positives this causes: `Half` walks to 4, really 2 (reports lengths 512-1023); `KeyValuePair<int,int>` walks to 12, really 8; `bool?` walks to 5, really 2. The injected field is FABRICATED, not truncated, so `max(fieldSize)` does not rescue it either.

## Measured `Unsafe.SizeOf<T>` table (net10.0)

```
Guid 16   DateTime 8   DateTimeOffset 16   TimeSpan 8   DateOnly 4   TimeOnly 8
Half 2    Index 4      Range 8             decimal 16
Vector2 8 Vector3 12   Vector4 16          Quaternion 16  Plane 16
Matrix3x2 24           Matrix4x4 64        Complex 16
Vector64<T> 8          Vector128<T> 16     Vector256<T> 32  Vector512<T> 64
ValueTuple<int,long> 16     ValueTuple<byte,byte> 2
KeyValuePair<int,int> 8     KeyValuePair<byte,byte> 2
bool? 2   int? 8   Guid? 20
```

Also verified: `struct Empty {}` is 1 byte; `struct S { Empty a; int c; }` is 8, so no field occupies 0 bytes.

## Audit findings (oracle, all empirically verified)

### P0

1. **Ref-assembly `_dummyPrimitive` poisoning.** Breaks the lower-bound invariant.
2. **`StructLayoutAttribute` and `FieldOffsetAttribute` are pseudo-custom attributes.** They live in `TypeAttributes.ExplicitLayout` / the `ClassLayout` and `FieldLayout` tables, NOT the `CustomAttribute` table. `GetAttributes()` returns nothing for metadata types. Verified on `System.Runtime.InteropServices.ComTypes.BINDPTR` (ExplicitLayout, 3 overlapping `IntPtr` fields, zero relevant CAs). So `isExplicit` is always false for metadata and overlapping fields get SUMMED. A `[StructLayout(Explicit)] struct Union3` with 3 overlapping fields has real sizeof 8 but computes as 20 when consumed from another assembly. Note `max(fieldSize)` IS a valid lower bound under any layout kind.
3. **Exponential walk, no memoization.** `[StructLayout(Explicit)] struct S1 { [FieldOffset(0)] S0 a, b, c, d; }` nested 16 levels stays tiny in bytes but is 4^16 nodes. Legal C# source, IDE hang, `CancellationToken` never observed.
4. **`TypeKind.Enum` recursion passes `depth` unchanged** instead of `depth + 1`.

### P1

5. `[InlineArray(N)]` structs underestimated by factor N. `[InlineArray(64)] struct Buf64 { private int _e0; }` computes 4, real sizeof 256. `InlineArrayAttribute` IS a genuine custom attribute visible in source and metadata, but lives in `System.Runtime.CompilerServices`, so `IsInteropAttribute` will not match it.
6. `[StructLayout(Size = N)]` invisible on metadata. Safe direction, underestimates only.
7. `Platform.AnyCpu -> 4` is correct, keep it.
8. `GetArraySize` only accepts a boxed `int`. `new T[10L]`, `new T[(uint)5000]` box as `long` / `uint` and are silently skipped. Missed suggestions only.
9. Should reject `IsRefLikeType`. `ref struct R { int x; }` has `IsValueType: true, IsUnmanagedType: true`; `new R[10]` is CS0611 and the rewrite is also illegal.
10. Skip constant lengths `<= 0`. `new T[0]` reporting is pointless noise.
11. Drop `ReportDiagnostics` from `ConfigureGeneratedCodeAnalysis`. An Info perf hint on uneditable generated code is noise.

### Confirmed fine, no action

- `sum(fieldSize) <= real size` for Sequential and Auto is sound. `Pack` only removes padding, `Auto` reorders but never overlaps, the CLR takes `max(computed, ClassLayout.Size)`.
- Substituting 1 for an unresolvable field is safe.
- `Math.Max(..., 1)` for empty structs is correct.
- `GetMembers()` on metadata DOES return private instance fields. The problem is upstream in GenAPI, not Roslyn.
- `IsFixedSizeBuffer` / `FixedSize` work in both source and metadata; Roslyn rewrites the field type to `E*`.
- `ReadStructLayout`'s `{int, short}` ctor-arg set is correct and complete. Roslyn boxes an enum-typed `TypedConstant.Value` as its underlying primitive.
- `IsUnmanagedType` exactly matches `!IsReferenceOrContainsReferences<T>()`. GenAPI emits `private object _dummy` alongside `_dummyPrimitive` precisely so unmanaged-ness survives.
- Excluding pointers / function pointers is strictly redundant (`PointerTypeSymbol.IsValueType` is false) but keep it as documentation.
- Rank / initializer gating is correct.
- Nothing in either file can throw AD0001.

### Hard constraints

- **Report location must stay `NewKeyword.GetLocation()`.** `LaquaiLibTokenFixer` looks up the token at the diagnostic location and checks `IsKind(SyntaxKind.NewKeyword)`.
- **Keep `Unsafe.As<ArrayCreationExpressionSyntax>`.** Deliberate house style; the repo ships a `ChangeToUnsafeAsRefactor`.
- **netstandard2.0 has no `System.Index`,** so list patterns (`is [{ Value: int x }, ..]`) do not compile. Use `.Length > 0` plus indexer checks.

## Status: COMPLETE

Solution builds clean (0 errors; the 220 warnings are pre-existing XML-doc noise in `multitarget/LaquaiLib`, none from the touched files). `dotnet test TestProjects/LaquaiLib.Analyzers.Tests` is 74/74 green, 31 of them the new LAQ0006 tests.

Sensitivity checked by mutation: forcing metadata structs back through the field walk fails exactly `HalfIsNotFourBytes`, `KeyValuePairIsNotFieldWalked`, `NullableIsUnderlyingPlusAFlag`, `GuidAtThreshold` and `IntrinsicVectorAtThreshold`, which is the ref-assembly regression class this change exists to kill.

## Work done in the first pass (builds clean, 0 warnings, 0 errors)

`Helpers.cs`: swapped `using System.Reflection;` for `using System.Runtime.InteropServices;`, deleted the dead `_underlyingTypeSymbolProperty` field, deleted the whole reflection fallback. Added `MaxLayoutDepth = 16`; `SizeOf(this ITypeSymbol, Compilation)`; `PointerSize(Compilation)` switching on `compilation.Options.Platform` (`X64 or Arm64 or Itanium => 8, _ => 4`); `SizeOfCore` with a `SpecialType` switch then a `TypeKind` switch; `StructSize` bailing on `System.Numerics.Vector<T>`, reading `ReadStructLayout`, walking `GetMembers()` for `IFieldSymbol { IsStatic: false, IsConst: false }`, substituting 1 for unresolvable fields, `max(FieldOffset + size)` when explicit else `+=`, clamped through a `long` accumulator; `FieldSize` handling `RefKind != None`, `IsFixedSizeBuffer`, reference types; `ReadStructLayout`; `FieldOffset`; `IsInteropAttribute`.

`UseAllocateUninitializedArrayAnalyzer.cs`: description rewritten to the `2048 / sizeof(T)` rule; `AnalyzeNode` made `static`; rank / initializer checks moved first; element type gated on `is not { IsValueType: true, IsUnmanagedType: true }` plus a pointer / function-pointer rejection; calls `elementType.SizeOf(context.Compilation)`; computes `var minimumLength = 2048 / sizeOf;` and only requires a constant length when `minimumLength > 0`.

## Work done in the second pass

### 1. Never field-walk metadata structs

`StructSize` now splits on `type.OriginalDefinition.DeclaringSyntaxReferences.IsEmpty`.

- Source structs go to `SourceStructSize`, which is the previous walk verbatim.
- Metadata structs go to `MetadataStructSize`: `WellKnownStructSize` table first, then `System.Nullable<T>` as `SizeOf(T) + 1`, then the poison detector (any instance field named `_dummy` or `_dummyPrimitive` returns -1), then `max(fieldSize)` for what is left. No instance fields at all also returns -1.
- `WellKnownStructSize` is three namespace-gated switches (`System`, `System.Numerics` arity 0; `System.Runtime.Intrinsics` arity 1) carrying the measured sizes above. `Vector<T>` keeps its own earlier bail.

### 2. Memoize

`Initialize` moved to `RegisterCompilationStartAction`, which allocates one `ConcurrentDictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default)` per compilation and closes over it in the syntax-node action. `SizeOf` takes it as an optional third parameter (`null` makes the walk allocate its own on first struct, so the O(distinct types) guarantee does not depend on the caller). `MaxLayoutDepth` is gone; `StructSize` now guards with a lazily created `HashSet<ITypeSymbol>` cycle set, so the enum `depth` bug is moot.

`IEqualityComparer<in T>` is contravariant, so `SymbolEqualityComparer.Default` binds to both the `ITypeSymbol`-keyed dictionary and set directly.

### 3. Remaining point fixes

- `ApplyInlineArray` scales the accumulator by the `[InlineArray(N)]` ctor argument, guarded on exactly one instance field, applied in both the source and metadata paths. `IsInteropAttribute` generalised to `IsSystemRuntimeAttribute(attributeClass, subNamespace, name)` so it can reach `System.Runtime.CompilerServices`.
- `GetArraySize` routes through `AsInt32`, which accepts `int`, `long`, `uint`, `ulong`, `short`, `ushort`, `byte`, `sbyte`, `nint`, `nuint`, saturating instead of overflowing.
- Analyzer rejects `IsRefLikeType`.
- Length gating restructured so `<= 0` constants are skipped even when the threshold has divided down to 0.
- `ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None)` was already correct, no change.

### 4. Tests

`TestProjects/LaquaiLib.Analyzers.Tests/Performance/UseAllocateUninitializedArrayAnalyzerTests.cs`, 31 tests, house style as specified. csproj gained the two 1.1.4 testing packages and the `ProjectReference` to `LaquaiLib.Analyzers`.

Everything on the case list is covered. Notes on the ones that needed a specific shape:

- `[InlineArray(4096)] struct Big { byte _e0; }` is the >= 2048 element type used for the non-constant-length, initializer, zero-length and negative-length cases, because only `sizeOf > 2048` drives the threshold to 0 and makes those gates observable. `[InlineArray(2048)]` would not: 2048/2048 is 1.
- The explicit-layout pair (`U` with three fields all at offset 0) reports at 512 and not at 511. Summing would give 12 and report at 511, so the pair is what pins `max(FieldOffset + size)`.
- `ref struct` and negative length need their compiler errors declared: `new {|CS0611:R|}[512]` and `new Big[{|CS0248:-1|}]`.
- The termination case is S0..S15, 4 same-typed fields each, all at offset 0. 4^15 nodes unmemoized; runs instantly memoized.

### 5. Verify

```
dotnet build LaquaiLib.slnx
dotnet test TestProjects/LaquaiLib.Analyzers.Tests
```

MTP is the runner. Do not pass `--logger` or any other VSTest-only flag. Note `dotnet test --nologo` makes the runner print its help and exit 5; drop the flag.
