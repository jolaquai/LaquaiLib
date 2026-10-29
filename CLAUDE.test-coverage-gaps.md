# CLAUDE.test-coverage-gaps.md

Plan: rank every LaquaiLib feature that currently has **zero** tests by how much it would benefit from having them, then close the gaps top-down.

Anchor: `5fd992d` on branch `net10`. Survey performed 2026-08-20. Scope is `multitarget/LaquaiLib` only (per standing review-scope preference); `LaquaiLib.Generators` and `LaquaiLib.Analyzers` have their own test projects and are out of scope.

**Update 2026-08-20**: the 19 placeholder test files identified in fact 2 (41 bytes, namespace-only, zero tests) have been deleted via `git rm` - `StringExtensionsTests.cs`, `StringExtensionsTests.Replace.cs`, `StreamExtensionsTests.cs`, `StreamExtensionsTests.MemoryStream.cs`, `UriExtensionsTests.cs`, `UriExtensionsTests.Query.cs`, `MethodInfoExtensionsTests.cs`, `PropertyInfoExtensionsTests.cs`, `ProcessExtensionsTests.cs`, `PartitionerExtensionsTests.cs`, `QueueExtensionsTests.cs`, `RandomExtensionsTests.cs`, `RangeExtensionsTests.cs`, `TimingExtensionsTests.cs`, `SemaphoreExtensionsTests.cs`, `StackTraceExtensionsTests.cs`, `ThreadExtensionsTests.cs`, `FileSystemModelExtensionsTests.cs`, `FileSystemModelExtensionsTests.FileProxy.cs`. Deletion is staged, not yet committed. They carried no tests, so nothing was lost; every ledger row that referenced one of these names now creates the file fresh rather than filling a stub. The three orphan names (`SemaphoreExtensionsTests`, `StackTraceExtensionsTests`, `ThreadExtensionsTests`) are gone for good - `ThreadingExtensions.SwitchTo` coverage (rank 9) should go in a newly-created `ThreadingExtensionsTests.cs`, not a revived `ThreadExtensionsTests.cs`. Section 8's "Also:" note is now historical.

---

## 0. PROTOCOL

1. Read the ledger in section 1. The first row not marked `DONE` is the current step.
2. Read the detail entry for that rank in sections 4-7 before writing any test.
3. Code + ledger flip go in the **same commit**. Never one without the other.
4. Facts in section 3 were measured, not guessed. Do not re-derive them; re-verify only if the anchor moved.

Test project conventions (already in force, do not deviate):
- xUnit.v3 + Microsoft.Testing.Platform. `TestContext`, not `ITestOutputHelper`. NSubstitute for mocks.
- Rebuild before running, then run the produced exe directly:

```bash
dotnet build TestProjects/LaquaiLib.UnitTests/LaquaiLib.UnitTests.csproj -c Debug
```

```bash
./TestProjects/LaquaiLib.UnitTests/bin/Debug/net11.0-windows/LaquaiLib.UnitTests.exe -class "LaquaiLib.UnitTests.Extensions.LinqMemoryExtensionsTests"
```

- Test file naming follows source file naming: `Foo.Bar.cs` -> `FooTests.Bar.cs`, mirrored directory structure.

---

## 1. LEDGER

| # | Target | Tier | Cost | Status |
|---|--------|------|------|--------|
| 1 | `Extensions/Memory/Linq/**` (`LinqMemoryExtensions`) | S | L | TODO |
| 2 | `UnsafeUtils/Accessors/**` | S | XS | DONE |
| 3 | `Collections/SequenceEqualityComparer` | S | M | TODO |
| 4 | `Numerics/Matrix<T>` | S | M | TODO |
| 5 | `Collections/Observable/ObservableCollectionFast<T>` | S | M | TODO |
| 6 | `Text/CharComparer` | S | S | TODO |
| 7 | `Extensions/StringExtensions` (+ `.Replace`) | A | L | TODO |
| 8 | `IO/Streams/` MultiStream, ObservableStream, NullStream, ExceptStream | A | M | TODO |
| 9 | `Threading/AsyncCountdownEvent` + `SwitchAwaitInfra` | A | S | TODO |
| 10 | `Extensions/Path.Static` | A | S | TODO |
| 11 | `IO/FileSystemHelper` (+ `.OSAlternatives`) | A | L | TODO |
| 12 | `Extensions/MethodInfoExtensions.RebuildMethod` | A | M | TODO |
| 13 | `Extensions/StreamExtensions` + `.MemoryStream` | A | S | TODO |
| 14 | `Text/Json/*Converter` | A | S | TODO |
| 15 | `IO/ResumableFileIO` + `IO/ResumableDirectoryIO` | A | L | TODO |
| 16 | `Extensions/IEnumerableExtensions.ToCollection` | B | S | TODO |
| 17 | `Extensions/UriExtensions.Query` + `QueryBuilder` | B | S | TODO |
| 18 | `Extensions/RangeExtensions` | B | S | TODO |
| 19 | `Collections/Enumeration/` MemoryEnumerable, SpanChunkEnumerable | B | XS | TODO |
| 20 | `Wrappers/` UsingWrapper, TempDirectory, TimeStamp, FlexEnumerableContainer, Async*Wrapper | B | S | TODO |
| 21 | `Util/MemoryDiff`, `Util/SpanFiller`, `Util/Sequence` | B | S | TODO |
| 22 | `Numerics/RandomMath`, `Numerics/Fibonacci` | B | S | TODO |
| 23 | `UnsafeUtils/MemoryManager` | B | M | TODO |
| 24 | `Extensions/FileSystemModelExtensions` (+ `.FileProxy`) | B | M | TODO |
| 25 | `Extensions/TextWriterExtensions.IndentedTextWriter` | B | XS | TODO |
| 26 | `Util/DelayingHttpMessageHandler`, `Util/SlidingWindowHttpMessageHandler` | B | M | TODO |
| 27 | `IO/FileSizePartitioner` + `Extensions/PartitionerExtensions` | B | S | TODO |
| 28 | Small extension leftovers (see 6.13) | B | S | TODO |
| 29 | `Threading/ExtendedDebugTask` | B | M | TODO |
| 30 | `Util/WpfForms/VirtualKey/VirtualKeyUtils` | B | S | TODO |
| - | Everything in section 7 | C | - | WON'T DO |

Cost key: XS < 30 min, S ~1h, M ~3h, L ~1 day.

---

## 2. HOW THE RANKING WAS DERIVED

Rank = **silent-wrongness risk x blast radius / cost**, with two multipliers:

- **Oracle bonus.** If a BCL type defines the expected behaviour (`Enumerable`, `Stream`, `IEqualityComparer` contract, `string.Equals`), the test is differential and costs a fraction of a hand-written fixture. Everything in tier S except #5 has an oracle.
- **Loud-failure penalty.** Code that throws on first misuse (P/Invoke, process spawning, WPF interop) is partially self-testing in practice. Code that returns a wrong value quietly (numerics, comparers, hashes, index math) is not. The latter ranks higher at equal size.

Deliberately **not** used as a ranking input: raw line count. `Util/WpfForms/WindowMessage.cs` is 994 lines of constants and ranks near the bottom; `Threading/AsyncCountdownEvent.cs` is 55 lines and ranks 9th.

---

## 3. MEASURED FACTS

1. `LaquaiLib.UnitTests` currently holds **1758** `[Fact]`/`[Theory]` methods.
2. **19 test files are placeholders**: 41 bytes each, containing only a namespace declaration. The feature they name has zero coverage despite the file existing, so any file-existence-based coverage estimate is wrong:
   `StringExtensionsTests.cs`, `StringExtensionsTests.Replace.cs`, `StreamExtensionsTests.cs`, `StreamExtensionsTests.MemoryStream.cs`, `UriExtensionsTests.cs`, `UriExtensionsTests.Query.cs`, `MethodInfoExtensionsTests.cs`, `PropertyInfoExtensionsTests.cs`, `ProcessExtensionsTests.cs`, `PartitionerExtensionsTests.cs`, `QueueExtensionsTests.cs`, `RandomExtensionsTests.cs`, `RangeExtensionsTests.cs`, `TimingExtensionsTests.cs`, `SemaphoreExtensionsTests.cs`, `StackTraceExtensionsTests.cs`, `ThreadExtensionsTests.cs`, `FileSystemModelExtensionsTests.cs`, `FileSystemModelExtensionsTests.FileProxy.cs`.
   Note: `SemaphoreExtensionsTests`, `StackTraceExtensionsTests` and `ThreadExtensionsTests` name types that do not exist in the library at all. Delete those three rather than filling them.
3. `Extensions/Memory/Linq/` is **69 files, 3744 lines, 312 public member declarations, 32 distinct operators, 0 tests**. It is the single largest untested unit in the repo. `MemoryExtensionsTests.cs` covers `Extensions/Memory/MemoryExtensions.cs` (`ToArray`, `Split`, `Write`, `ReadString`) only, not the Linq family.
4. `IO/FileSystemHelper.cs` is 846 lines with ~20 public methods; the only test file touching it (`IO/FileSystemHelperTests/PathStringTests.cs`, 13 tests) exercises `ChangeName` alone.
5. **Confirmed divergence, `Extensions/Memory/Linq/Sum.cs`**: the `int`/`long` overloads accumulate with `+=` in an unchecked context. `Enumerable.Sum` throws `OverflowException` on the same input (verified empirically). The XML docs claim parity via `<inheritdoc cref="Enumerable.Sum..."/>`. One of the two has to give; a test must pin whichever is intended.
6. **Confirmed divergence, `Text/CharComparer.OrdinalIgnoreCase`**: `Fold` is ASCII-only (`(uint)((c | 0x20) - 'a') <= 'z' - 'a'`). Measured against `string.Equals(..., StringComparison.OrdinalIgnoreCase)`:

   | pair | `CharComparer.OrdinalIgnoreCase` | `StringComparison.OrdinalIgnoreCase` |
   |---|---|---|
   | `A`/`a` | equal | equal |
   | `U+00C4`/`U+00E4` (Ae/ae) | **not equal** | equal |
   | `U+03A3`/`U+03C3` (Sigma) | **not equal** | equal |
   | `U+0410`/`U+0430` (Cyrillic A) | **not equal** | equal |
   | `U+0130`/`i` | not equal | not equal |

   Every non-ASCII cased letter disagrees with the comparer it is named after.
7. **Deterministic hang, `Threading/AsyncCountdownEvent`**: constructed with `count: 0`, `remaining` starts at 0, so the internal `TaskCompletionSource` is never completed and `WaitAsync()` returns a task that never finishes, while `Signal()` throws `InvalidOperationException`. `CountdownEvent(0)` is signalled immediately by contrast.
8. `Extensions/IBufferWriterExtensions`, `Extensions/TaskExtensions` (3 of 4 blocks) and several `extension` blocks are empty shells. No tests needed for empty blocks; they are noted so nobody counts them as gaps.

---

## 4. TIER S

### 4.1 (rank 1) `Extensions/Memory/Linq/**` - `LinqMemoryExtensions`

Untested: all 32 operators over `ReadOnlySpan<T>`/`Span<T>` plus the 25 `_Of*` primitive specialisation files (`_OfInt32`, `_OfNullableDouble`, `_OfKeyValuePair`, `_OfValueTuple2`, ...).

Why it ranks first: largest untested surface by an order of magnitude, it is `AggressiveInlining`-marked hot-path code intended to replace LINQ in allocation-sensitive paths, and its own XML docs bind it to `Enumerable` semantics via `<inheritdoc>`. That doc contract is a free oracle.

Test shape: one `Theory`-driven differential harness per operator. Build `T[]`, run `arr.AsSpan().Op(...)` against `arr.Op(...)` from `System.Linq`, assert equality of result or of the thrown exception type.

Must-cover boundaries:
- Empty span for every operator (`Sum` -> 0, `Average` -> throws, `Average` over `int?` -> null, `First`/`Single`/`Max` -> `InvalidOperationException`, `FirstOrDefault` -> `default`).
- Single-element and all-null nullable sequences (`Sum(int?)`, `Average(int?)`, `Min`/`Max` over `T?` skip nulls).
- Overflow: see fact 5. Pin the intended behaviour explicitly, in both directions (`int`, `long`).
- Destination-span overloads (`Where`, `Select`, `Chunk`, `ToArray`, `Distinct`, `Zip`, `SelectMany`): exact-fit destination, destination one element too short (must throw `ArgumentException`, must not have written past the end), oversized destination (return value is the count, tail untouched), and overlapping source/destination if that is meant to be legal.
- `Single`/`SingleOrDefault`/`OnlyOrDefault` with 2+ matches. `OnlyOrDefault` has no BCL twin; hand-specify it.
- Predicate index overloads receive 0-based source indices, not destination indices.
- `Chunk` with `chunkSize <= 0`, and last-chunk remainder.
- `ToLookup`/`ToDictionary`/`GroupBy` with duplicate and null keys, and with a custom `IEqualityComparer`.

`Extensions/Memory/MemoryHelpers.cs` (the internal `SpanLookup`/`Grouping` backing `ToLookup`/`GroupBy`) is covered incidentally by (d) below; it needs no separate suite.

Suggested split so the step is committable in pieces: (a) harness + aggregates (`Sum`/`Average`/`Min`/`Max`/`Count`/`LongCount`/`Aggregate`), (b) filtering and projection with destination spans, (c) element access and quantifiers, (d) grouping and materialisation, (e) `_Of*` specialisations.

### 4.2 (rank 2) `UnsafeUtils/Accessors/**`

Untested: `ListAccessors<T>` (`_items`, `_size`, `_version`), `QueueAccessors<T>` (`_array`, `_head`, `_tail`, `_size`), `StackAccessors<T>` (`_items`, `_size`), `MemoryStreamAccessors` (9 fields), `CompositeFormatAccessors._segments`, `RegexAccessors` (`Match._regex`, `Capture.get_Text`).

Why it ranks this high despite being ~180 lines total: these are `[UnsafeAccessor]` bindings to **private BCL fields**. They break on any runtime that renames a field, and this repo targets `net11.0` preview. The failure is a `MissingFieldException` thrown at the first call site, at runtime, in whatever consumer code happened to touch it. `Extensions/StreamExtensions.MemoryStream` and parts of `IO/BufferWriter` sit directly on top of these.

Test shape: one assert per accessor. Construct the BCL object in a known state, read the field through the accessor, assert it matches the observable state (`_size` == `Count`, `_items.Length` == `Capacity`, `_position` after a seek, `Capture.get_Text()` == the input string). About 25 asserts total. This is the cheapest high-value insurance in the repository and should be treated as a runtime-upgrade canary.

### 4.3 (rank 3) `Collections/SequenceEqualityComparer` / `SequenceEqualityComparer<T>`

Untested: 441 lines, both the non-generic type (implements `IEqualityComparer` for `ICollection`, `IList`, `Array`, `IEnumerable`, `object`) and the generic one (`T[]`, `List<T>`, `IList<T>`, `ICollection<T>`, `IEnumerable<T>`), plus bitwise fast paths keyed on `SequenceHelpers.GetBitwiseSize(typeof(T))` and the `Create(inner)` wrapping form.

Why: a comparer that violates the `Equals`/`GetHashCode` contract corrupts every dictionary and set built on it, silently, at a call site far from the bug. There are five overload-dispatch paths per type here and a hand-rolled fast path per element size, which is exactly the shape where one path drifts from the others.

Must-cover:
- Equal sequences must produce equal hash codes, across **every** static type they can be viewed as. Same array passed as `T[]`, `IList<T>`, `IEnumerable<T>` must hash identically.
- Reference equality, both-null, one-null, different lengths, same elements different order.
- `T[]` vs `List<T>` with identical contents (the `CollectionsMarshal.AsSpan` path vs the array path).
- Multidimensional and non-SZ arrays (`IsIndexable` rejects them; `Equals(Array, Array)` has its own branch).
- Nested sequences (element itself a collection) - does the default comparer recurse or compare by reference? Pin it.
- `Create(inner)` custom comparer is honoured by both `Equals` and `GetHashCode`.
- Non-generic vs generic agreement on the same data.

### 4.4 (rank 4) `Numerics/Matrix<T>`

Untested: 944 lines, 45 public members. `Determinant`, `Inverse`, `GetRowEchelonForm`, `GetReducedRowEchelonForm`, `Rank`, `IsRowEchelonForm`, `IsReducedRowEchelonForm`, `IsUpper/LowerTriangularForm`, `GetLeadingCoefficients`, operators `+ - * /` (matrix and scalar), `SwapRows`/`SwapColumns`, `Transpose`, `MultiplyElementWise`, four constructors, `Identity`, `Zero`.

Why: linear algebra is the canonical silent-wrongness domain. `DetRowReduce` and `Inverse` are hand-written elimination routines with pivoting; a wrong pivot or a missed sign flip produces plausible-looking garbage. It also has a strong property-test oracle, which makes it cheap relative to its size.

Test shape, mostly properties over generated small matrices (`double` and `decimal`):
- `A * Identity == A`, `A * A.Inverse() ~= Identity` (tolerance for `double`), `A.Transpose().Transpose() == A`.
- `det(A * B) ~= det(A) * det(B)`, `det(Identity) == 1`, `det` of a matrix with two equal rows == 0, `det` flips sign on `SwapRows`.
- `Rank` of a rank-deficient matrix; `Inverse()` on a singular matrix must throw, not return garbage.
- RREF is idempotent; `IsReducedRowEchelonForm(GetReducedRowEchelonForm(A))` is true; RREF of an invertible matrix is the identity.
- Hand-computed 2x2 and 3x3 fixtures for `Determinant` and `Inverse` so a property-only suite cannot pass on a consistently-wrong implementation.
- Non-square guards on `Determinant`/`Inverse`/`Identity`, dimension mismatch on `+`/`-`/`*`.
- `default(Matrix<T>)` (the struct with a null `_data`) must not null-ref on `Rows`/`Columns`/`ToString`.

### 4.5 (rank 5) `Collections/Observable/ObservableCollectionFast<T>`

Untested: 536 lines, 47 public members. Every mutator has a `...Silent` twin (`Add`/`AddSilent`, `Insert`/`InsertSilent`, `Remove`/`RemoveSilent`, `Clear`/`ClearSilent`, `Move`/`MoveSilent`, `RemoveAt`/`RemoveAtSilent`, `Sort`/`SortSilent`), plus `Silenced(Action)`/`Unsilenced(Action)`, `KeepOrdered`, `Comparer`, `Filter`, `Range`/`Index` indexers, `Reset`, `Find*`, `IndexOf`/`LastIndexOf` with custom comparer and start index, and `ReadOnlySpan` over the backing `List<T>`.

Why: the whole value proposition is event fidelity, and there is no oracle - `ObservableCollection<T>` deliberately behaves differently here. The `Silent` twins double every code path, and `Filter` affects `GetEnumerator` while `Count` and the indexer read the unfiltered backing store, which is an invariant that will surprise someone. Nothing here needs I/O, so cost is low relative to member count.

Must-cover:
- For each mutator pair: contents identical afterwards, event raised exactly once for the loud variant with the correct `NotifyCollectionChangedAction`/index/items, zero events for the silent variant.
- `Silenced(action)` suppresses events raised inside, including manual `RaiseCollectionChanged`, and restores the prior state afterwards (including when `action` throws).
- `KeepOrdered = true` sorts immediately and re-sorts after every mutation; assigning `Comparer` re-sorts and raises `Reset`.
- `Filter` affects enumeration only; `Count`, `this[int]`, `Contains`, `CopyTo` and `ReadOnlySpan` are documented against whichever behaviour is intended - pin it either way.
- `ReadOnlySpan` reflects `Count`, not `Capacity`, and is invalidated correctly after a growth-triggering `Add`.
- Range indexer with `Index`/`Range` including from-end and out-of-range.
- `Move(i, j)` for i < j, i > j, i == j, and the ends.

### 4.6 (rank 6) `Text/CharComparer`

Untested: 6 singletons (`Ordinal`, `OrdinalIgnoreCase`, `CurrentCulture(IgnoreCase)`, `InvariantCulture(IgnoreCase)`), `FromComparison`, and the `IComparer<char>`/`IEqualityComparer<char>` implementations behind each strategy.

Why: fact 6 is a confirmed, reproducible divergence from the comparison the member is named after, and this is a comparer, so the wrongness is silent. Small enough to be a one-hour job.

Must-cover:
- `FromComparison` maps each `StringComparison` to the right singleton and throws on an undefined value.
- For each strategy and a fixed corpus (ASCII pairs, `U+00C4`/`U+00E4`, `U+03A3`/`U+03C3`, `U+0410`/`U+0430`, `U+0130`/`i`, digits, surrogate halves): `Equals(x, y)` agrees with `string.Equals(x.ToString(), y.ToString(), comparison)`, and `Compare` agrees in sign with `string.Compare`.
- `Equals(x, y) == true` implies `GetHashCode(x) == GetHashCode(y)` for every strategy. This is the assert that fails today for `OrdinalIgnoreCase` on non-ASCII if the fold is fixed but the hash is not.
- Culture strategies cache `CompareInfo` in a `static readonly` field, so `CurrentCulture` is captured at type init, not per call. Pin that behaviour or fix it.

---

## 5. TIER A

### 5.1 (rank 7) `Extensions/StringExtensions` + `.Replace`

700 lines, placeholder test file. Untested families: `IndicesOf` (char/string/span, with and without `startIndex`), `IndexOfAny`/`IndicesOfAny` (char and string sets, with `StringComparison`), `IndicesOfExcept`/`IndexOfAnyExcept`/`IndicesOfAnyExcept`, `TransparentSplit` (regex and string forms, returning value+separator pairs), `Remove` (4 overloads), `SelectLines`/`ForEachLine` (4 overloads each), `Repeat`, `ToSentence`, `ToTitle`, `GetSimilarity`, `EnumerateSplits`, and `Replace(search, factory, recurse)` / `TryReplace`.

Highest-traffic untested extension in the library, pure and deterministic, so tests are cheap per assert. Priority within it: the index-search family (overlapping matches, empty needle, `startIndex` at 0/length/out of range, needle longer than haystack, deferred-execution semantics of the `IEnumerable<int>` returns), then `Replace` with `recurse: true` (a replacement containing the search term is an infinite loop - pin the guard or add one), then `TransparentSplit` (round-trip: concatenating values and separators reproduces the input).

### 5.2 (rank 8) `IO/Streams/` MultiStream, ObservableStream, NullStream, ExceptStream

~740 lines, zero tests, while the sibling `ArrayPoolMemoryStream` carries 190. `Stream` is the oracle: every override must match documented `Stream` semantics.

- `MultiStream`: write fan-out to N inner streams, `CanRead`/`CanSeek`/`CanWrite` aggregation when inners disagree, `Length`/`Position` when inners diverge, `leaveOpen` on both `Dispose` and `DisposeAsync`, one inner throwing mid-write, sync/async parity, `SetLength`/`Seek` throwing `NotSupportedException`.
- `ObservableStream<T>`: each of `DataRead`/`DataWritten`/`Seeked`/`Flushed`/`Resized` fires once per operation with correct counts and offsets, including through `CopyTo`/`CopyToAsync`, `ReadByte`/`WriteByte`, and the span/memory overloads. Partial reads must report actual bytes, not requested.
- `NullStream`: `Read` returns 0, `Position` stays consistent, `Instance` is shared and safe to reuse. `ExceptStream`: every member throws the documented exception type.

### 5.3 (rank 9) `Threading/AsyncCountdownEvent` + `SwitchAwaitInfra` + `ThreadingExtensions.SwitchTo`

`AsyncCountdownEvent`: fact 7 is a deterministic hang for `count: 0`. Also needs: `Signal` past zero throws, concurrent `Signal` from N threads completes exactly once, `Reset` cancels pending waiters with `OperationCanceledException` and restores the initial count, `WaitAsync` after completion returns an already-completed task, `SignalAndWaitAsync` from the last signaller.

`SynchronizationContextAwaitable`/`Awaiter` and `TaskSchedulerAwaitable`/`Awaiter`: `IsCompleted` is true only when already on the target context (test with a custom single-threaded `SynchronizationContext`), the continuation actually runs on the target, null constructor arguments throw. Awaiter bugs are extremely expensive to diagnose in the field and near-free to test here.

### 5.4 (rank 10) `Extensions/Path.Static`

`EnsureQuoted`, `TrimQuotes`, `IsQuoted`, `IsUncPath`, `EnsureEndingDirectorySeparator`, `ResolveMappedDrive`, `GetParts`. `GetParts` and `TrimQuotes`/`IsQuoted` each exist in a `string` and a `ReadOnlySpan<char>` form, so the string form is a free differential oracle for the span form - assert they agree on every input.

Cases: no extension, trailing dot, multiple dots, trailing separator, root-only, relative, UNC (`\\server\share\file`), extended-length prefix, empty and whitespace, already-quoted and half-quoted strings. `ResolveMappedDrive` touches the machine, so gate it or test only the parse half.

### 5.5 (rank 11) `IO/FileSystemHelper` + `.OSAlternatives`

1102 lines, 13 existing tests all on `ChangeName`. Untested: `MigrateDirectoryAsync`, `MigrateDirectoryAsArchiveAsync`, `CutFile`/`CutFileAsync` (4 overloads), `EnumerateDirectoryStructureMatches`, `UnpackDirectory`, `IsBaseOf`, `IsEmpty`/`DeleteIfEmpty` (string and `DirectoryInfo`), `TryCopyLockedFile`, `RemoveZoneIdentifier`, `OpenAlternateContentStream`, `ReplacePermissions`, the `SearchValues`/invalid-char tables and `FillInvalid*Chars`.

Split by cost: the pure ones (`IsBaseOf`, the char tables, `FillInvalid*Chars`, `ChangeName` edge cases) are trivial and should land first. The temp-directory ones (`IsEmpty`, `DeleteIfEmpty`, `UnpackDirectory`, `MigrateDirectory*`, `CutFile`) need `TempDirectory` fixtures and cancellation coverage. `ReplacePermissions`, `RemoveZoneIdentifier`, `OpenAlternateContentStream` and `TryCopyLockedFile` are NTFS/Windows-specific - test on a real temp file, skip elsewhere.

### 5.6 (rank 12) `Extensions/MethodInfoExtensions.RebuildMethod`

263 lines that emit C# source text from a `MethodInfo`, plus the `IsGetter`/`IsSetter`/`IsAdder`/`IsRemover`/`IsAccessor`/`IsExtern`/`IsPartial` predicates and a `BodyGenerator` delegate hook. Deterministic given a fixture type, so golden-string tests are exact and cheap.

Cover: generic methods with constraints, `ref`/`out`/`in`/`params` parameters, default values (including `null`, `default(T)`, enums, strings needing escapes), nullable and tuple return types, explicit interface implementations, accessors, static/abstract/virtual/sealed modifier combinations, and `inheritdoc: false`. Same fixture types the generator test project already uses where possible.

### 5.7 (rank 13) `Extensions/StreamExtensions` + `.MemoryStream`

`ReadToEnd`/`ToArray`/`ReadToEndAsync` on non-seekable and partially-consumed streams; `AsSpan`/`AsMemory` (10 overloads) and `CopyBlock` over `MemoryStream` internals. The latter reach into `MemoryStreamAccessors`, so these tests double as coverage for rank 2. Boundaries: stream at non-zero `Position`, `MemoryStream` created over a user array with a non-zero origin, non-exposable buffer (`new MemoryStream(byte[], writable: false)`), empty stream, `Range` with from-end indices, and length past `Length` vs past `Capacity`.

### 5.8 (rank 14) `Text/Json/` DateTimeUnixConverter, TimeSpanFromSecondsConverter, FlexibleUnmanagedTypeConverter

Serialization output that is wrong is persisted wrong, and round-trip tests are near-free. Cover: round-trip for each converter, epoch and negative values, fractional seconds, `DateTimeKind` handling, reading from both a JSON number and a JSON string, malformed token types throwing `JsonException` and not something else, and the reflection-vs-source-gen path in `FlexibleUnmanagedTypeConverter` (`JsonHelper.IsReflectionAllowed`).

### 5.9 (rank 15) `IO/ResumableFileIO` + `IO/ResumableDirectoryIO`

580 lines whose entire reason to exist is surviving interruption. Untested: `CopyFileAsync`/`MoveFileAsync`/`CopyDirectoryAsync`/`MoveDirectoryAsync`, `Cancel`/`CancelAsync`, the JSON state files, and `FileCopyState.FileHash` verification.

Cover: cancel mid-copy then resume and verify the result is byte-identical to the source; a corrupt or truncated state file; a state file pointing at a source that changed since (hash mismatch); resume when the destination already holds a complete file; the `CompletedFiles`/`PendingFiles` bookkeeping after a partial directory copy; move semantics deleting the source only after a verified copy. Higher cost than its rank because of the async plus filesystem setup, which is the only reason it sits at the bottom of tier A rather than in tier S.

---

## 6. TIER B

Same reasoning, lower stakes or smaller surface. Batch several per commit.

6.1 (rank 16) `IEnumerableExtensions.ToCollection` - 20 methods, all one-liners over LINQ. The one real trap is `ToStack(preserveOrder)`, whose two branches produce reversed results; assert both. Selector-with-index overloads must pass source indices.

6.2 (rank 17) `UriExtensions.Query` + `QueryBuilder` - escaping is security-adjacent. Cover: value containing `&`, `=`, `#`, `%`, spaces and non-ASCII; overwriting an existing parameter; removing via null value; empty query; fragment preservation; the `ReadOnlySpan<string>` overload with an odd argument count.

6.3 (rank 18) `RangeExtensions` - `GetRange()` on a `Range` with from-end indices (needs a length; must throw or be documented), `GetRange(length)`, reverse ranges, empty ranges, and the LINQ-shaped wrappers (`Select`, `Where`, `Join`, `GroupJoin`, `OrderBy`).

6.4 (rank 19) `MemoryEnumerable<T>` (a struct that is both `IEnumerable<T>` and `IEnumerator<T>`, so `GetEnumerator` returns itself - double enumeration and boxing semantics need pinning) and `SpanChunkEnumerable<T>` (last partial chunk, `chunkSize <= 0`, empty source, chunk size exceeding length).

6.5 (rank 20) `Wrappers/` - `UsingWrapper` (all 3 factory forms, double dispose, `Instance` after dispose throws `ObjectDisposedException`, implicit conversion, async dispose path), `TempDirectory` (created on construct, deleted on dispose including when non-empty, explicit-path ctor, double dispose), `TimeStamp` (`Elapsed` monotonic, comparison and equality operators), `FlexEnumerableContainer` (single vs enumerable mode, `default` instance, both `TryGetValue` overloads, non-boxing enumerator), `AsyncEnumerableWrapper`/`AsyncEnumeratorWrapper` (`Empty`, cancellation observed by `MoveNextAsync`).

6.6 (rank 21) `Util/MemoryDiff` (`FindDifference`/`FindDifferences`: identical spans, differing lengths, `startIndex` past the end, custom comparer, empty), `Util/SpanFiller.FillSequential` (wrap behaviour at `T.MaxValue`, non-zero start, empty destination), `Util/Sequence.Create` (negative step, step of zero, start > stop, non-integer `T`).

6.7 (rank 22) `Numerics/RandomMath` (`GCD`/`LCM` with zero, negatives, single element, empty span; `RuleOfThree` with y == 0; `RoundToMultiple` with multiple <= 0; `Interpolate` boundary behaviour at `xStart`/`xEnd`) and `Numerics/Fibonacci` (`GetNth` against known values including 0, 1, 2 and a large index; `GenerateSequence` with start > end).

6.8 (rank 23) `UnsafeUtils/MemoryManager` - real value (round-trip write/read through `Allocate`, `ReCAlloc` preserving existing contents and handling shrink, `Free` with and without GC pressure accounting, `SetMemoryLimit`/`GetMemoryLimit`), but a failing test here can take the test host down. Keep the allocations small and run it in its own class.

6.9 (rank 24) `FileSystemModelExtensions` + `.FileProxy` - `Directory`/`MakeDirectory`/`File` path composition (including `params` forms and traversal segments), `IsBaseOf` for sibling and case-differing paths. `FileProxy` is a ~40-method forwarding layer over `File.*`; a table-driven test that each proxy hits the same path and encoding as the direct call is mechanical and catches copy-paste slips.

6.10 (rank 25) `TextWriterExtensions.IndentedTextWriter` - new, currently untracked. `ItwIndent`/`ItwScope` restore the previous indent on dispose, including when the body throws, and nest correctly. Ten minutes of work.

6.11 (rank 26) `DelayingHttpMessageHandler`, `SlidingWindowHttpMessageHandler` - real rate-limiting logic (`MakeReadOnly`, `Window`/`MaximumRequestCount` mutation after read-only, `NextCallAllowed`, cancellation during the wait). Both read wall-clock time directly, so tests are slow or flaky until a time abstraction is injected. Do that refactor first or accept short real delays with generous tolerances.

6.12 (rank 27) `IO/FileSizePartitioner` (balance across partitions given skewed sizes, partition count > file count, empty input, `SupportsDynamicPartitions` false path) and `PartitionerExtensions.ToArray(partitions)`.

6.13 (rank 28) Small leftovers, one commit: `QueueExtensions.EnqueueRange` (both `Queue<T>` and `ConcurrentQueue<T>`, span and enumerable), `PropertyInfoExtensions.GetValue<T>`/`GetValueOrDefault<T>` (wrong type, null target, static property), `ICloneableExtensions.Copy`, `IEnumeratorExtensions` (`GetEnumerator` self-return enabling `foreach` over an enumerator, `AsAsynchronous`), `ConcurrentDictionaryExtensions.Create` (duplicate keys - last wins or first wins?), `RuntimeHelpersExtensions` (`IsReferenceOrContainsReferences` for pointer, function-pointer, generic struct and cached repeat calls; `GetUninitializedObject<T>` for value types and reference types), `HttpListenerRequestExtensions.Route` (no query, empty query, query-only), `TaskExtensions.WaitSafeAsync` (faulted task is swallowed, cancellation returns without throwing), `LinqXml/` (`Siblings`, `SiblingNodes`, `ReplaceWith` document order), `RandomExtensions.NextBytes(Stream, int)` sync and async (count of 0, count larger than the internal chunk size), `TimingExtensions.NextWeekday` (Friday and Saturday inputs, `includeSaturdays`) and its `GetAwaiter` forms.

6.14 (rank 28b) `IO/Streams/RandomStreams/` `RandomStream` and `CryptographicRandomStream` - `Read` always fills the requested buffer and returns the full count, `CanWrite`/`CanSeek`/`Length`/`Position` report the intended values, seeded `RandomStream` is reproducible, `CryptographicRandomStream` is not, both survive a zero-length read. Cheap.

6.15 (rank 29) `Threading/ExtendedDebugTask` - 694 lines, but mostly constructor and `Run` forwarding. Worth a smoke test per factory plus whatever debug state the type actually tracks; low risk of silent wrongness.

6.16 (rank 30) `Util/WpfForms/VirtualKey/VirtualKeyUtils` - table-driven key name to virtual-key mapping, currently dirty in the working tree. Round-trip every enum member and assert unmapped inputs fail cleanly. The `VirtualKey` enum itself (707 lines of constants) needs nothing.

---

## 7. NOT WORTH TESTING (deliberate)

Listed so nobody re-derives the decision later.

| Target | Why not |
|---|---|
| `Util/WpfForms/WindowMessage.cs` (994L), `VirtualKey.cs` (707L), `MessageBox*` (~550L), `ToolTip.cs` | Constant tables and native UI. A test would restate the constants; the UI parts need a message pump. Exception: `VirtualKeyUtils`, see 6.15. |
| `Interop.cs`, `IO/FileSystemHelper.OSAlternatives` P/Invoke half | Thin `[LibraryImport]` declarations. Failures are loud and immediate, and the generator already validates marshalling. Covered indirectly by rank 11. |
| `Util/ExceptionManagement/HResult.cs` | 120 `const int` values. Nothing to assert that is not a copy of the declaration. |
| `Util/Meta/*` (`MetaHelpers`, `MetaTool`, `VSVersion`, `VSEdition`, `Architecture`) | Probes the local Visual Studio install. Results differ per machine; assertions would be either tautological or environment-locked. |
| `Bcl/System/Environment.SpecialFolders.cs` (459L) | Enum-to-known-folder mapping resolved by the OS. At most a smoke test that every member resolves without throwing; not worth a slot in the ledger. |
| `Oxml/Extensions/*` | Requires OpenXml document fixtures and a package dependency the unit test project does not carry. Revisit if the Oxml surface grows. |
| `EF/Extensions/DbContextExtensions`, `DbSetExtensions` (92L) | Needs an EF provider in the test project. Reconsider only if these grow past trivial forwarding. |
| `Util/ShellInterfaces/*` (440L) | Spawns real shells. Environment-dependent, slow, and the failure mode is loud. |
| `Extensions/ProcessExtensions` (affinity) | Depends on CPU count and process privileges; assertions would be machine-locked. |
| `Config.cs`, `AppState.cs`, `Internal.cs`, `Interfaces/*`, `Collections/Observable/*EventArgs.cs`, `Collections/DetachedDequeNodeException` | Plumbing, marker interfaces, property-bag DTOs and exception shells. |
| `Extensions/IBufferWriterExtensions`, empty `extension` blocks in `TaskExtensions` | Empty. Nothing to test until something is added. |

---

## 8. APPENDIX: HAS TESTS, STILL THIN

Not part of this plan's ranking (the ask was zero-coverage features), but worth queueing after tier A.

| Target | Source | Tests | Note |
|---|---|---|---|
| `Extensions/TypeExtensions` | 1048L | 32 | Large reflection surface; `ReflectionOptions` (60L) has no tests at all. |
| `Text/StringHelpers` | small | 3 | Only `AllocString` and `GetSpan`. Mutating an interned literal through `GetSpan` deserves an explicit warning test. |
| `IO/Streams/MemoryOrFileStream` | 60L | 7 | The spill-to-file threshold transition is the interesting case; confirm it is covered. |
| `Dynamic/FullAccessDynamicFactory` | 73L | via `FullAccessDynamicTests` | Factory paths only exercised incidentally. |
| `Numerics/BitArray` | 1261L | 63 | Good ratio, but check the SIMD paths are hit for large sizes. |

Also: `SemaphoreExtensionsTests.cs`, `StackTraceExtensionsTests.cs` and `ThreadExtensionsTests.cs` name types that do not exist in the library (verified: no `SemaphoreExtensions`/`StackTraceExtensions`/`ThreadExtensions` class, and no `extension(Semaphore*)`/`extension(Thread)`/`extension(StackTrace)` block anywhere). Delete them, or rename `ThreadExtensionsTests.cs` to `ThreadingExtensionsTests.cs` and use it for rank 9.

**Out-of-order completion, 2026-08-20**: rank 2 (`UnsafeUtils/Accessors/**`) was closed before rank 1 because `Check-AccessorTestCoverage.ps1` (`LaquaiLib.UnitTests.csproj`'s `CheckAccessorTestCoverage` target) hard-errors CI on any `[UnsafeAccessor]` member not referenced by name in test source, independent of ledger ranking. Added `ListAccessorsTests.cs`, `QueueAccessorsTests.cs`, `StackAccessorsTests.cs`, `MemoryStreamAccessorsTests.cs`, `CompositeFormatAccessorsTests.cs`, `RegexAccessorsTests.cs` alongside the existing reflection-driven `AccessorCanaryTests.cs` (which proves binding correctness but, by design, never mentions member names as literal text, so it cannot satisfy the textual gate on its own).
