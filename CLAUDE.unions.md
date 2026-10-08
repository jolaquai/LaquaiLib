# C# Unions - Feature Spec Reference

Source: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/unions  
Updated: 2026-06-03. Status: Preview proposal, not finalized.

---

## Core Concepts

**Union type** - any class or struct with `[System.Runtime.CompilerServices.UnionAttribute]`.  
**Case types** - the set of types a union value can contain (derived from constructor/factory parameter types).  
**Union declaration** - shorthand `union` keyword syntax; lowered to a plain struct.

---

## Union Members (Pattern Requirements)

### Basic union pattern (mandatory)
- **Union creation members**: public constructors with a single `by-value` or `in` param (one per case type), OR static `Create(T) -> Union` factory methods if using a union member provider.
- **`Value` property**: `public object? Value { get; }` (may also have `init`/`set` of any accessibility, ignored by compiler).

### Non-boxing access pattern (optional, enables efficient matching of value-type cases)
- `public bool HasValue { get; }` - true iff `Value != null`.
- `public bool TryGetValue(out T value)` - one per case type; returns true iff `Value` is a non-null `T`. For nullable value-type case types, `out` param is the *underlying* type (not `T?`).

### Union member provider
If the union type *directly contains* a nested `public interface IUnionMembers`, members are looked up there (not on the union type itself). The union type must implement this interface. Factory methods replace constructors in this case.

---

## Union Behaviors

### Union conversions
- Implicit conversion from any case type `C` to the union type `U` (calls the matching creation member).
- Also implicit from `C` to `U?` when `U` is a struct.
- **Not** a standard implicit conversion - cannot chain through another user-defined or union conversion.
- **Priority**: user-defined implicit operator > union conversion > explicit user-defined operator (when explicit cast used).
- No explicit-only union conversions. No lifted (`T?` -> `U`) union conversions.

```csharp
Pet pet = dog;          // -> new Pet(dog)
Result<string> r = "x"; // -> Result<string>.IUnionMembers.Create("x")
```

### Union matching
Patterns are applied to `Value` (i.e., the contents), **except** `_`, `var`, and `not` which apply to the union value itself.

```csharp
GetPet() is var pet      // 'pet' is Pet (union), NOT the contents
GetPet() is Dog dog      // applied to GetPet().Value
GetPet() is null         // applied to GetPet().Value (struct) or (union == null || union.Value == null) for class unions
GetPet() is { } value    // applied to GetPet().Value
```

Logical patterns (`and`/`or`/`not`):
- Rule applied per-branch.
- Left branch of `and` can change the incoming value for the right branch (e.g., after a type pattern succeeds).

**`is`-type operator** behaves as a type pattern for union types (not a raw runtime check).

**Compiler preference**: uses `TryGetValue` when available and conversion from pattern type to out-param type is identity/reference/boxing only. Falls back to `HasValue` for null checks, then to `Value` property.

For `Nullable<UnionStruct>`: unwrapping works the same as for class union nullable matching.

**Note**: `GetPet() is Pet pet` likely never matches because `Pet` is applied to `Value` (an `object?`), not to the union itself.

### Union exhaustiveness
A `switch` expression is exhaustive when all case types are covered; no fallback required.

```csharp
var name = pet switch
{
    Dog d => ...,
    Cat c => ...,
    // no warning
};
```

If `Value`'s null state is "maybe null", a warning fires for unhandled `null` even in an otherwise-exhaustive switch.

### Nullability
- Default null state of `Value`: "maybe null" if any case type is "maybe null"; "not null" otherwise. Determined by annotations on `Value`'s declared type.
- After a union conversion, `Value` inherits null state of the incoming value.
- After `HasValue`/`TryGetValue` checks (explicit or via pattern matching), `Value` is "not null" on the `true` branch.

---

## Union Declaration Syntax

```antlr
union_declaration
    : attributes? struct_modifier* 'partial'? 'union' identifier type_parameter_list?
      '(' type (',' type)* ')' struct_interfaces? type_parameter_constraints_clause*
      ('{' struct_member_declaration* '}' | ';')
    ;
```

Lowered to:

```csharp
[Union] public struct Pet : IUnion
{
    public Pet(Cat value) => Value = value;
    public Pet(Dog value) => Value = value;
    public object? Value { get; }
    // ...body
}
```

**Restrictions on union declaration body:**
- No instance fields, auto-properties, or field-like events.
- No explicitly declared public single-parameter constructors (conflicts with generated ones).
- Explicit constructors must delegate via `this(...)` to a generated constructor.

**Not** a record struct. `record union` is not supported.

Case types may be interfaces, type parameters, nullable types, or other unions. Cases may overlap.

```csharp
public union Pet(Cat, Dog, Bird);

public union OneOrMore<T>(T, IEnumerable<T>)
{
    public IEnumerable<T> AsEnumerable() => Value switch { ... };
}

public record class None();
public record class Some<T>(T value);
public union Option<T>(None, Some<T>);
```

---

## IUnion Interface

```csharp
public interface IUnion
{
    object? Value { get; }
}
```

Compiler-generated unions implement this. Enables: `value is IUnion { Value: null }`.

`IUnion<TUnion>` was removed from the design.

---

## Well-formedness Assumptions

The compiler assumes (violations cause undefined behavior, not errors):
- **Soundness**: `Value` is always null or a value of a case type, including for `default`.
- **Stability**: `Value` after creation reflects what was passed in (or null for null inputs).
- **Creation equivalence**: if a value converts to multiple case types, all matching creation members behave identically.
- **Access pattern consistency**: `HasValue`/`TryGetValue` are observably equivalent to checking `Value` directly.

---

## Key Resolved Design Decisions

| Question | Resolution |
|---|---|
| Compiler error for missing required members? | Open - currently unspecified behavior, not a hard error |
| Is union declaration a record? | No - plain struct only |
| Nullable value types as case types | Underlying type is the case type; `TryGetValue` out-param is non-nullable |
| `Nullable<UnionStruct>` matching | Yes - applies union matching through `Nullable<T>` |
| Lifted conversions (`int?` -> `Union`) | Not supported |
| `is`-type operator on union types | Behaves as type pattern |
| Type parameter constrained to union type | NOT treated as a union type - no union matching |
| "Bad" `TryGetValue`/`HasValue` APIs | Silently ignored, no error or diagnostic |
| Missing `UnionAttribute`/`IUnion` types | Not synthesized; user must reference or declare them |
| Ref-ness of creation member parameters | by-value or `in` only |

---

## Open / Unresolved Questions

- Should matching against `.Value` directly apply union exhaustiveness rules?
- Precise rules for finding `HasValue`/`TryGetValue` (inheritance? read/write `HasValue`?).
- `TryGetValue` and nullable analysis: which `TryGetValue` methods affect `Value`'s null state?
- Classes as union types: null-check ambiguity (union-null vs value-null) and inheritance confusion are known issues; no resolution yet.
- Namespace for `IUnion`: proposal is `System.Runtime.CompilerServices`.
- `IUnion.Value` vs public `Value` property lookup: may change to prefer public `Value`.
- Concurrency safety of union declarations (single ref field, `this` copied vs not).
