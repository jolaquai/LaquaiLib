# CLAUDE.generator-incrementality.md

Plan: close the incrementality-testing gap in `LaquaiLib.Generators` and fix the pipeline defects it exposes.

Anchor commit: `07a3562` on branch `net10`. Findings in section 2 were measured at `1d9ca88` and re-verified unchanged at `07a3562`.

---

## 0. PROTOCOL - READ FIRST, NON-NEGOTIABLE

You are resuming a multi-step plan. This file is the single source of truth for progress.

### 0.1 The sync invariant

**The code change for a step and the ledger update marking that step done MUST be in the same commit.**

This is the entire mechanism that makes this file trustworthy. Never commit code without flipping the ledger row in the same commit. Never flip a ledger row in a commit that does not contain the corresponding code. If those two things always ship together, the ledger cannot drift, and no one ever has to ask "is the plan up to date".

### 0.2 Resuming

1. Read this file top to bottom.
2. Read the ledger in section 1. The first row not marked `DONE` is your current step.
3. Read the files named in that step.
4. Execute. Verify. Commit code + ledger together.
5. Repeat.

Do not re-derive the findings in section 2. They were measured, not guessed. They are facts about this repo as of the anchor commit.

### 0.3 Committing

- Commit locally after every step. **Never push.**
- One-line commit messages, terse, lowercase-ish, no body, no trailing period.
- Examples: `add tracked-driver harness to GeneratorTestHost`, `fix EnumExpanderGenerator crash on non-generic enums`, `project InlineArray pipeline to equatable model`
- **Stage only files belonging to this plan.** Never `git add -A`, never `git commit -a`. Stage explicit paths. The working tree was clean at the anchor commit, but a parallel workstream on `LaquaiLib.Analyzers*` lands here regularly, so check `git status` before staging rather than assuming.

### 0.4 Ledger status tokens

| Token | Meaning |
| --- | --- |
| `TODO` | Not started |
| `WIP` | Started, incomplete. Must carry a `Notes` entry saying exactly what is done and what remains |
| `DONE` | Complete, verified, committed |
| `RED` | Intentionally committed as a failing test. Expected. Names the step that turns it green |
| `SKIP` | Deliberately dropped. Must carry a reason |

### 0.5 Self-check (only when explicitly asked)

```
git log --oneline -20
dotnet build LaquaiLib.sln
dotnet test TestProjects/LaquaiLib.Generators.Tests/LaquaiLib.Generators.Tests.csproj
```
Compare commit subjects against the ledger. If they disagree, the ledger loses and must be corrected to match reality in a commit of its own: `resync incrementality plan ledger`.

### 0.6 Line numbers

Line numbers in this file are anchored to commit `1d9ca88` and will drift. Locate by symbol name, not by line. If a referenced symbol has vanished, that is a finding: record it in Notes before proceeding.

---

## 1. LEDGER

| # | Step | Status | Notes |
| --- | --- | --- | --- |
| 1.1 | `EquatableArray<T>` helper in generator project | DONE | `netstandard2.0/LaquaiLib.Generators/EquatableArray.cs`, `internal`, `where T : IEquatable<T>` |
| 1.2 | Tracked-driver harness in `GeneratorTestHost` | DONE | `CreateTrackedDriver`, `RunTracked`, `RunAgain`, `AssertStepsCached`, `AssertStepsRan`, `SourceOutputStepName` const. Existing members untouched |
| 1.3 | `AssertNoRoslynObjectsInModel` object-graph walker | DONE | `ModelPurityAssertions.cs`. Walks `SourceOutput` inputs only. Does not descend into a flagged offender (one `ISymbol` reaches the whole compilation graph) |
| 1.4 | `AssertNoCapturingLambdas` reflection guard | DONE | `CapturingLambdaAssertions.cs`. `Type` and generic overloads. Passes today, all lambdas are `static` |
| 2.1 | `GeneratorIncrementalityTests` - FullAccessProxyGenerator | DONE | All six green as of 4.3 |
| 2.2 | `GeneratorIncrementalityTests` - InlineArrayExtensionsGenerator | DONE | Landed RED with three failures (unrelated edit, freshly-parsed trees, model purity). All six green after 4.2 |
| 2.3 | `GeneratorIncrementalityTests` - EnumExpanderGenerator | DONE | Landed RED with all four step tests failing on `result.Exception` (the 2.2 crash). 3.1 fixed the crash and flipped `ModelHoldsNoRoslynObjects` from a vacuous pass to a genuine RED, confirming the walker works. All six green after 4.4 |
| 3.1 | Fix `EnumExpanderGenerator` hard crash | DONE | Dropped `ConstructUnboundGenericType()`, `symbol.ToDisplayString(FullyQualified)` directly. `SourceOutput` now executes and emits for the first time ever; `result.Exception` is null across all four scenarios |
| 3.2 | Emission tests for `EnumExpanderGenerator` | DONE | `EnumExpanderGeneratorEmissionTests.cs`, 10 facts. Needed an additive `RunGenerator(IIncrementalGenerator, params string[])` overload on `GeneratorTestHost`. Landed RED: only `EmptyEnumIsSkippedEntirely` was green, the other 9 failed `AssertNoCompilationErrors` on two emission defects the plan did not anticipate. All 10 green after 3.3 + 3.4 |
| 3.3 | Fix unqualified `EnumFieldData<,>` reference | DONE | `EnumDataGenerator.cs`: the shared record struct is emitted into `namespace {ContainingAssembly.Name}` but each `*Data` class is emitted into the enum's own namespace and references `EnumFieldData<,>` bare, with no `using` and no qualification. CS0246 for every enum whose namespace differs from the assembly name. Fixed by hoisting `assemblyRootNamespace` out of the write-once block and emitting `global::{assemblyRootNamespace}.EnumFieldData<...>`. Landed in the same commit as 3.4 because neither alone turns 3.2 green |
| 3.4 | Fix global-namespace enum emission | DONE | `EnumDataGenerator.cs`: `writer.WriteLine($"namespace {symbol.ContainingNamespace.ToDisplayString()}")` is unconditional, so a top-level enum yields the literal `namespace <global namespace>` and a syntax error. Fixed by guarding on `IsGlobalNamespace` and passing `null` instead of `writer.Scope` to the `using` so the brace block is omitted too |
| 3.5 | `EnumExpanderGenerator` effective accessibility + skip un-emittable enums | TODO | found at 5.1 |
| 3.6 | `WriteLines` drops stray CR that shatters every doc comment | DONE | Found at 5.1. `IndentedTextWriterExtensions.WriteLines` split on `
` only, but `SourceEmitHelper.Summary` builds with `StringBuilder.AppendLine` = `
`, so every emitted line kept a trailing ``. A lone CR is a line terminator in C#, so each `///` line became its own isolated, unterminated doc comment: 184 CS1570 in `multitarget/LaquaiLib`. Fixed by trimming one trailing `` per slice. Shared infra, so all three generators benefit |
| 3.7 | Auto-generated header + obsolete suppression on enum output | TODO | found at 5.1 |
| 4.1 | Model-projecting `ForAttributeWithMetadataNameOn` overload | DONE | `ForAttributeWithMetadataNameOn<TNode, TModel>(string, Func<GeneratorAttributeSyntaxContext, CancellationToken, TModel>)` added alongside the existing single-type-param overload. Delete the old one once 4.2 and 4.3 have both migrated |
| 4.2 | `InlineArrayExtensionsGenerator` -> equatable model | DONE | `InlineArrayModel` record (TypeName, SimpleName, Namespace, ElementTypeName, FieldName, Length, TypeParameterList), all strings/int so no `EquatableArray<T>` needed. `CreateModel` returns null and is filtered by `.Where` instead of the old `.First()` throws. Emission behaviour preserved verbatim. Required a `System.Runtime.CompilerServices.IsExternalInit` polyfill (netstandard2.0 has no `init`). Turned 2.2 green, 10 -> 7 failures |
| 4.3 | `FullAccessProxyGenerator` -> equatable model, drop `CompilationProvider` | DONE | DEVIATION from the spec, deliberate. The model is `ProxyModel(HintName, Source, Diagnostic)`: the transform runs the existing emitter and carries the emitted **source text** as the model, instead of a flattened `ProxyMemberModel` mirror. Rationale: the emitter consumes the nested ancestor chain, proxyable interfaces with all their members, the deduplicated base-chain walk, per-member `[UnsafeAccessor]` target types, ordered constraint clauses and unsafe-context detection - a faithful structural mirror is 12-15 record types and a rewrite of every `Write*` method, for zero measurable gain. Transform cost is identical either way (FAWMN re-runs the transform on any compilation change regardless); what matters is that its output compares by value. All 20 `Write*` helpers are byte-identical to the anchor commit. `CompilationProvider.Combine` deleted, `GetTypeByMetadataName` moved into the transform via `SemanticModel.Compilation`, FAP001 carried as `ProxyDiagnosticInfo(TypeName, FilePath, TextSpan, LinePositionSpan)` and rebuilt with `Location.Create`. `.Collect()` dropped entirely, so `SourceOutput` is now per-proxy. Leaky single-type-param `ForAttributeWithMetadataNameOn<T>` overload deleted as 4.1 required |
| 4.4 | `EnumExpanderGenerator` -> equatable model | DONE | `EnumModel` + `EnumMemberModel` records; members carried as `EquatableArray<EnumMemberModel>`. `CreateSyntaxProvider` transform now projects the model and `.Where`-filters nulls (empty enums, which the emitter used to `continue` past). `Namespace` is null for global-namespace enums. All 10 emission tests still green, so output is byte-identical. Turned 2.3 green, 7 -> 3 failures |
| 4.5 | `WithTrackingName` on every user-defined pipeline step | DONE | Names live as `public const string` on `GeneratorStepNames` in the generator project, referenced from both sides. `FullAccessProxy.{Models,Filtered}` (no `Collected`, 4.3 dropped `.Collect()`), `InlineArray.{Models,Filtered,Collected}`, `EnumExpander.{Models,Filtered,Collected}` |
| 4.6 | Tighten 2.x to assert named steps, not just `SourceOutput` | DONE | All four scenarios now assert every named step plus `SourceOutput`. `AssertNoRoslynObjectsInModel` gained a `params string[] namedSteps` widening that walks those steps outputs, asserts each name exists (a rename can no longer no-op the assertion), asserts `result.Exception is null`, and asserts something was actually walked (kills the vacuous-pass hole recorded at 2.3) |
| 5.1 | Full verify pass | TODO | |

---

## 2. MEASURED FINDINGS (do not re-derive)

Measured by driving the real generator assembly with `trackIncrementalGeneratorSteps: true` against `Microsoft.CodeAnalysis.CSharp` 5.6.0, three runs each: cold, unrelated-file-added, identical-compilation.

### 2.1 API surface, confirmed against 5.6.0

```csharp
GeneratorDriverOptions(IncrementalGeneratorOutputKind disabledOutputs = None,
                       bool trackIncrementalGeneratorSteps = false,
                       string baseDirectory = null)

enum IncrementalStepRunReason { New = 0, Modified = 1, Unchanged = 2, Cached = 3, Removed = 4 }

GeneratorRunResult.TrackedSteps        // ImmutableDictionary<string, ImmutableArray<IncrementalGeneratorRunStep>>
GeneratorRunResult.TrackedOutputSteps  // same shape, key "SourceOutput"
IncrementalGeneratorRunStep            // .Name .Inputs .Outputs .ElapsedTime
step.Outputs                           // ImmutableArray<(object Value, IncrementalStepRunReason Reason)>

IncrementalValueProviderExtensions.WithTrackingName(this IncrementalValueProvider<T>, string)
IncrementalValueProviderExtensions.WithTrackingName(this IncrementalValuesProvider<T>, string)

CSharpGeneratorDriver.Create(IEnumerable<ISourceGenerator> generators,
                             IEnumerable<AdditionalText> additionalTexts = null,
                             CSharpParseOptions parseOptions = null,
                             AnalyzerConfigOptionsProvider optionsProvider = null,
                             GeneratorDriverOptions driverOptions = null)
```

Key consequence: `ForAttributeWithMetadataName` self-names its internal steps (`result_ForAttributeWithMetadataName`, `compilationAndGroupedNodes_ForAttributeWithMetadataName`, and others), and `TrackedOutputSteps["SourceOutput"]` is always populated. **`WithTrackingName` is not a prerequisite for the highest-value assertions.** Phase 2 therefore lands before any generator edit. `CreateSyntaxProvider` names nothing, so `EnumExpanderGenerator` yields a completely empty `TrackedSteps` until 4.5.

### 2.2 `EnumExpanderGenerator` is 100% dead

```
System.InvalidOperationException: Operation is not valid due to the current state of the object.
   at Microsoft.CodeAnalysis.CSharp.Symbols.NamedTypeSymbol.ConstructUnboundGenericType()
   at LaquaiLib.Generators.EnumExpanderGenerator.GenerateEnumExpansions(...) in EnumDataGenerator.cs:line 38
```

`EnumDataGenerator.cs:38` calls `symbol.ConstructUnboundGenericType()`. Enums are never generic, so this throws for every enum in every compilation. The generator has never emitted anything. It has zero test coverage.

### 2.3 `FullAccessProxyGenerator` re-runs fully on every keystroke

Adding one unrelated file to the compilation:

```
Compilation                            [Modified]
result_ForAttributeWithMetadataName    [Modified]
SourceOutput                           [Modified]     <- full re-emit
```

Cause: `FullAccessProxyGenerator.cs:27`, `context.CompilationProvider.Combine(...)`. `Compilation` changes identity on every edit so the combined node is permanently `Modified`. Cold-run cost: 348ms in `result_ForAttributeWithMetadataName`, 56ms in `SourceOutput`. That is the per-keystroke cost in the IDE.

### 2.4 `InlineArrayExtensionsGenerator` is broken identically, without `CompilationProvider`

Same `SourceOutput [Modified]` on an unrelated edit. Cause: `Extensions/SyntaxProviderExtensions.cs:10` returns raw `GeneratorAttributeSyntaxContext` as the pipeline model. It carries `SemanticModel`, `ISymbol`, `SyntaxNode`, all per-compilation reference-equality types, so `.Collect()` never compares equal. This one helper is the root cause for two of three generators.

### 2.5 What passes today

Re-running against the *identical* `Compilation` instance yields `Cached` everywhere. That is the degenerate case and proves nothing. Any incrementality test that only covers it is worthless.

---

## 3. TARGET FILES

Generator project: `netstandard2.0/LaquaiLib.Generators/LaquaiLib.Generators.csproj` (netstandard2.0, LangVersion preview, `Microsoft.CodeAnalysis.CSharp` 5.6.0, `IsRoslynComponent`, `EnforceExtendedAnalyzerRules`).

| File | Symbol |
| --- | --- |
| `netstandard2.0/LaquaiLib.Generators/EnumDataGenerator.cs` | `EnumExpanderGenerator` (note: file name does not match class name) |
| `netstandard2.0/LaquaiLib.Generators/FullAccessProxyGenerator.cs` | `FullAccessProxyGenerator`, 947 lines, `FAP001` descriptor |
| `netstandard2.0/LaquaiLib.Generators/SourceGeneratedExtensions/InlineArrayExtensionsGenerator.cs` | `InlineArrayExtensionsGenerator`, namespace `LaquaiLib.Generators.SourceGeneratedExtensions` |
| `netstandard2.0/LaquaiLib.Generators/Extensions/SyntaxProviderExtensions.cs` | `ForAttributeWithMetadataNameOn<T>`, 12 lines |

Test project: `TestProjects/LaquaiLib.Generators.Tests/LaquaiLib.Generators.Tests.csproj`. net11.0, `xunit.v3` 3.2.2, Microsoft.Testing.Platform (`UseMicrosoftTestingPlatformRunner`, `OutputType=Exe`), `Microsoft.CodeAnalysis.CSharp` 5.6.0. Generators referenced as plain assemblies, deliberately **not** `OutputItemType="Analyzer"`.

| File | Contents |
| --- | --- |
| `GeneratorTestHost.cs` | `GeneratorTestResult` struct, `GeneratorTestHost` static class. `RunGenerator` hardcodes `new FullAccessProxyGenerator()` and passes no `GeneratorDriverOptions` |
| `FullAccessProxyGeneratorDiagnosticsTests.cs` | FAP001 behaviour, 64 lines |
| `FullAccessProxyGeneratorEdgeCaseTests.cs` | robustness, 142 lines |
| `FullAccessProxyGeneratorEmissionTests.cs` | output shape, 492 lines. **This is the safety net for step 4.3** |

xUnit.v3 only. `[Fact]`/`[Theory]` from `Xunit`, `TestContext` not `ITestOutputHelper`, no `ConfigureAwait`. NSubstitute if mocking is ever needed. No Verify/snapshot packages, do not introduce them.

---

## 4. STEPS

### Phase 1 - harness. Test project + one generator-side helper.

**1.1 `EquatableArray<T>`**
New file in the generator project. `readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>` wrapping `ImmutableArray<T>`, with `SequenceEqual`-based `Equals` and an order-sensitive `GetHashCode`. Implicit conversion from `ImmutableArray<T>`, `IEnumerable<T>` implementation for consumption.

Rationale, and this is load-bearing: `ImmutableArray<T>` inside a record does **not** get structural equality from the compiler-generated `Equals`. Every model in phase 4 holds collections. Without this type, phase 4 silently fails scenario 3 of phase 2 and the whole exercise is theatre.

Commit: `add EquatableArray<T> for incremental generator models`

**1.2 Tracked-driver harness**
Extend `GeneratorTestHost` **additively**. Do not modify the existing `RunGenerator`, `CreateCompilation`, `AssertNoCompilationErrors`, `GetGeneratedSource`, or `AssertAllSystemReferencesGlobalQualified`. Three existing test files depend on them.

Add:
- `CreateTrackedDriver(IIncrementalGenerator gen)` -> `GeneratorDriver`, built with `new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true)` and `new CSharpParseOptions(LanguageVersion.Preview)`. Pass `None` for `disabledOutputs` so `SourceOutput` still executes and stays measurable.
- `RunTracked(IIncrementalGenerator gen, params string[] sources)` -> `(GeneratorDriver Driver, CSharpCompilation Compilation, GeneratorRunResult Result)`.
- `RunAgain(GeneratorDriver driver, Compilation next)` -> `GeneratorRunResult`.
- `AssertStepsCached(GeneratorRunResult result, params string[] stepNames)`. Accept `Cached` or `Unchanged`. Fail on `New`, `Modified`, `Removed`. Failure message must dump every tracked step with its reason and `ElapsedTime`, otherwise diagnosing a red run means re-instrumenting from scratch.
- `AssertStepsRan(GeneratorRunResult result, params string[] stepNames)`. Inverse, requires `New` or `Modified`.
- Also assert `result.Exception is null` inside these helpers. Finding 2.2 proves a generator can throw and leave every existing assertion trivially satisfied.

Commit: `add tracked-driver harness to GeneratorTestHost`

**1.3 `AssertNoRoslynObjectsInModel(GeneratorRunResult)`**
Reflectively walk the object graph of every `step.Outputs[].Value` across `TrackedSteps`. Fail if any reachable instance is assignable to `ISymbol`, `Compilation`, `SyntaxNode`, `SemanticModel`, `SyntaxTree`, `SyntaxReference`, `Location`, `AttributeData`, `IOperation`, or `GeneratorAttributeSyntaxContext`.

Implementation notes: track visited by reference identity to survive cycles, cap depth, skip `string` and primitives, recurse through fields not properties (properties can throw or lazily materialise Roslyn objects), and handle `IEnumerable` elements. Report the full field path to the offender, for example `ProxyModel.Members[3].ReturnType`, otherwise the failure is unactionable.

Skip Roslyn's own internal steps when walking. Their values legitimately contain `Compilation`. Restrict the walk to steps whose name is not in the built-in set, or run it only over steps registered via `WithTrackingName` once 4.5 lands. Until then, restrict to `SourceOutput` inputs.

This is the direct detector for the "models pinning ISymbol/Compilation" bug class named in the original gap note, and it also catches the IDE memory-leak variant that pure caching assertions miss.

Commit: `add roslyn-object-graph assertion for generator models`

**1.4 `AssertNoCapturingLambdas(Type generatorType)`**
Fail if `generatorType.GetNestedTypes(BindingFlags.NonPublic)` contains a display class matching `<Initialize>b__*` / `<>c__DisplayClass*` originating from `Initialize`. A non-capturing lambda is cached into the singleton `<>c`; a capturing one allocates a display class per call and can smuggle a `Compilation` into the graph.

All lambdas are currently `static` in all three generators, so this passes today. It is a regression guard, and it is the second half of the bug class the original note called out.

Commit: `add capturing-lambda guard for generators`

### Phase 2 - the tests. Land RED. Test project only.

New file `TestProjects/LaquaiLib.Generators.Tests/GeneratorIncrementalityTests.cs`, one nested class or region per generator. Four scenarios each:

1. **Unrelated edit.** Run, then `compilation.AddSyntaxTrees(parse("class TotallyUnrelated { void M() { } }"))`, run again. Assert `SourceOutput` is `Cached`. This is the money test. Fails today for all three per findings 2.2, 2.3, 2.4.
2. **No-op re-run.** Same `Compilation` instance twice. Assert `Cached`. Passes today, guards regressions.
3. **Equivalent-but-new trees.** Rebuild the compilation from freshly parsed identical source text, run against it. Assert `Cached`. Catches reference-based `Equals` and the `ImmutableArray<T>` trap from 1.1.
4. **Relevant edit.** Modify the actual generator target. Assert `Modified`. Without this, "cache everything and never regenerate" passes 1-3 and ships a generator that stops emitting.

Plus per generator: `AssertNoRoslynObjectsInModel` and `AssertNoCapturingLambdas`.

Sources to drive the generators:
```csharp
public enum Color { Red, Green, Blue }

[FullAccessProxy(typeof(System.IO.MemoryStream))]
public partial class MsProxy { }

[InlineArray(4)]
public struct Buf4 { private int _e0; }
```

Commit these failing. They are executable bug reports. Mark the ledger rows `RED` with the step that turns each green. Do **not** weaken an assertion to get green, and do **not** use `Skip`. A `RED` row is honest; a skipped test is not.

Commits: `add incrementality tests for FullAccessProxyGenerator (red)`, and likewise for the other two.

### Phase 3 - fix the crash.

**3.1** `EnumDataGenerator.cs:38`: drop `ConstructUnboundGenericType()`, use `symbol.ToDisplayString(SymbolDisplayFormats.FullyQualified)` directly. Enums cannot be generic, so the unbound-generic path is unreachable by construction. Verify the generator produces output at all, which it has never done.

Commit: `fix EnumExpanderGenerator crash on non-generic enums`

**3.2** Emission tests for `EnumExpanderGenerator`. None exist. Follow the shape of `FullAccessProxyGeneratorEmissionTests.cs`: `AssertNoCompilationErrors` first, then assert on generated text. Cover flags enums, nested enums, non-int underlying types, empty enums, duplicate member values, global-namespace enums.

Commit: `add emission tests for EnumExpanderGenerator`

### Phase 4 - fix the pipelines.

Shared principle: **resolve everything to an equatable model inside the transform, never downstream of it.** `GeneratorAttributeSyntaxContext.SemanticModel.Compilation` is reachable from inside the transform, so `CompilationProvider` is unnecessary in all three generators.

**4.1** Add a model-projecting overload to `SyntaxProviderExtensions.ForAttributeWithMetadataNameOn<T>`:
```csharp
IncrementalValuesProvider<TModel> ForAttributeWithMetadataNameOn<TNode, TModel>(
    string fullyQualifiedMetadataName,
    Func<GeneratorAttributeSyntaxContext, CancellationToken, TModel> transform) where TNode : SyntaxNode
```
Keep the existing overload compiling until 4.2 and 4.3 have both migrated, then delete it so nothing can leak `GeneratorAttributeSyntaxContext` again.

Commit: `add model-projecting ForAttributeWithMetadataNameOn overload`

**4.2** `InlineArrayExtensionsGenerator`. Project to a small equatable record: fully-qualified type name, element type FQN, length, accessibility, containing namespace. Then `.Collect()`. Small and mechanical. **Do this first as the proof of pattern before touching 4.3.** Turns test 2.2 green.

Commit: `project InlineArray pipeline to equatable model`

**4.3** `FullAccessProxyGenerator`. Highest risk step in the plan.

Build a flattened `ProxyModel` record: proxy type name, namespace, accessibility, type parameters, and the full member list as an `EquatableArray<ProxyMemberModel>` where each member carries name, parameter signature, static/instance, `UnsafeAccessorKind`, return type, and ref-kind, all as strings. Everything the emitter needs must be in the model; nothing may reach back to a symbol.

Then delete the `CompilationProvider.Combine` at `FullAccessProxyGenerator.cs:27` and move both `GetTypeByMetadataName` (for the string-name attribute form) and the `FAP001` diagnostic decision into the transform. Diagnostics must be carried in the model as data, not reported from a symbol held past the transform.

Run `FullAccessProxyGeneratorEmissionTests.cs` continuously during this refactor. All 492 lines of it are the only thing standing between this change and silent output regressions. Turns test 2.1 green.

Commit: `rebuild FullAccessProxyGenerator pipeline on equatable model`

**4.4** `EnumExpanderGenerator`. Replace `CreateSyntaxProvider` returning `(EnumDeclarationSyntax, SemanticModel)` with a transform projecting to an equatable model: enum FQN, underlying type name, flags-ness, and members as `EquatableArray<(string Name, long Value)>`. Turns test 2.3 green.

Note the residual design weakness, out of scope here: `.Collect()` of every enum into one source file means adding one enum member regenerates the entire file. Correct model equality at least makes *unrelated* edits free, which is the actual goal. Record as a follow-up, do not fix in this pass.

Commit: `project EnumExpander pipeline to equatable model`

**4.5** Add `WithTrackingName` to every user-defined `Select`, `Where`, `Collect`, and `Combine` across all three generators. Use stable, greppable names, for example `FullAccessProxy.Models`, `FullAccessProxy.Collected`, `InlineArray.Models`, `EnumExpander.Models`. Define them as `const string` shared between generator and tests where practical, so a rename cannot silently turn a test into a no-op.

Commit: `add tracking names to generator pipeline steps`

**4.6** Tighten phase 2 to assert on the named steps rather than only `SourceOutput`, and widen `AssertNoRoslynObjectsInModel` to walk every named step now that Roslyn's internal steps can be excluded by name.

Commit: `assert named pipeline steps in incrementality tests`

### Phase 5 - verify.

**5.1**
```
dotnet build LaquaiLib.sln
dotnet test TestProjects/LaquaiLib.Generators.Tests/LaquaiLib.Generators.Tests.csproj
```
Every phase 2 test green with no assertion weakened. Every `RED` ledger row flipped to `DONE`. All pre-existing diagnostics, edge-case, and emission tests still green.

Then confirm the actual objective: for all three generators, an unrelated edit yields `SourceOutput [Cached]`, and cold-run cost for `FullAccessProxyGenerator` no longer repeats on every keystroke. A scratch driver equivalent to the one used for section 2 is the fastest way to eyeball this; a temporary console project referencing `LaquaiLib.Generators.csproj` and `LaquaiLib.Analyzers.Shared.csproj` plus `Microsoft.CodeAnalysis.CSharp` 5.6.0 is sufficient. Build it outside the repo and do not commit it.

Commit: `verify generator incrementality end to end`

---

## 5. RISKS

| Risk | Mitigation |
| --- | --- |
| 4.3 regresses proxy output | `FullAccessProxyGeneratorEmissionTests.cs` runs continuously. Do not batch 4.3 with other steps |
| `ImmutableArray<T>` in a record silently breaks equality | 1.1 exists precisely for this. Scenario 3 of phase 2 is the detector |
| Object-graph walker false-positives on Roslyn internal steps | Restrict the walk to `SourceOutput` inputs until 4.5, then to named steps |
| Staging a parallel workstream's `Analyzers*` changes | Check `git status` first, stage explicit paths only. Never `git add -A` |
| `EnforceExtendedAnalyzerRules` rejects new generator-side types | `EquatableArray<T>` must stay netstandard2.0-clean. Expect RS-prefixed analyzer errors and fix rather than suppress |
| Emission tests pass while generator throws | `AssertStepsCached` also asserts `result.Exception is null`. Finding 2.2 is exactly this failure mode |

---

## 6. OUT OF SCOPE

- Pushing anything. Local commits only.
- Verify/snapshot testing packages. Not present, do not introduce.
- The per-file `.Collect()` granularity weakness in `EnumExpanderGenerator` (noted in 4.4).
- Anything under `LaquaiLib.Analyzers`, `LaquaiLib.Analyzers.Fixes`, or `TestProjects/LaquaiLib.Analyzers.Tests`. Separate, actively-worked workstream.
