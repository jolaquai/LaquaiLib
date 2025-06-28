# FullAccessDynamic<>

`FullAccessDynamic<>` is a wrapper utility that allows access to any member of the wrapped object when cast to `dynamic`.
The type obeys type safety rules, meaning invalid casts are disallowed and will still cause exceptions at run-time.

Additionally, while _not_ being treated as `dynamic`, most operations on `FullAccessDynamic<>` have no special meaning; dynamic binding does not occur.

While the implementation itself is straightforward and sound as-is, there are several issues that need to be addressed by developers using the type.

## Performance

As with any other topic that involves `dynamic`, performance tanks very quickly since dynamic binding is indescribably slow. `FullAccessDynamic<>` has no other choice than to use reflection to gain access to the members of the wrapped object. While some caching is done to improve performance, it is still orders of magnitude slower than using the wrapped object directly.

## Boxing

- `struct`s used to instantiate `FullAccessDynamic<T>` are copied as any `struct` assignment would.
- Anything assigned to `dynamic` is boxed since `dynamic` is implemented on top of `object`. This is also the main reason why `FullAccessDynamic<T>` is a `class`.

## `null` propagation

`null` propagates only indirectly. Because member accesses (when they succeed) obviously have a type (either a property's or method's return type or a field's declared type), any access that succeeds (i.e. binding succeeds because the member exists) will result in a new `FullAccessDynamic<>` of that underlying type.

An access that returns a new `FullAccessDynamic<>` instance of `T`, even if that `T` instance is `null`, looks no different than a return where it is not since creating a new instance of `FullAccessDynamic<T>` with a `null` instance is valid (even if not very useful). Because of this, the conditional-access operator `?.` *cannot* correctly propagate `null` or avoid member accesses when the underlying value is `null`. Either `Unwrap()` before the member access or cast the returned value to the type of the member to avoid this issue:

```csharp
class MyClass
{
    public int SomeProperty { get; set; }
}

var dyn = new FullAccessDynamic<MyClass>(null);
var result = dyn?.SomeProperty; // BAD! Access will always happen

// Better:
if (dyn.Unwrap() is not null)
{
    var result = dyn.SomeProperty; // OK, no access if `myClass` is `null`
}
```

## Type checks and casts

The language will allow cast expressions involving `dynamic` to any type (except pointers). Dynamic binding will then validate this cast at run-time. `FullAccessDynamic<>` obeys type safety rules, meaning if the underlying type is not assignable to the type being cast to, an `InvalidCastException` is thrown.

However, this has the following side effects. Let `dyn` be an instance of `FullAccessDynamic<T>` typed as `dynamic`, where `T` is some arbitrary other type. Then:
- `dyn is A` will evaluate to `true` if and only if `A` is exactly `FullAccessDynamic<T>`.
- `dyn as A` will always return `null` except if and only if `A` is exactly `FullAccessDynamic<T>`.
- `(A)dyn` will succeed if and only if one of the following is true:
  - `A` is exactly `FullAccessDynamic<T>`, or
  - `A` is either exactly `T` or a type `T` is assignable to.

The above applies to implicit casts as well, such as when assigning `dynamic` to `lvalues` with a specified type that is not `dynamic`.

## Equality handling

A value returned by some member access through a `FullAccessDynamic<>` instance (that is, in the absense of a cast, as specified above) will never compare equal to the underlying value, even if they were reference-equal, since the value returned is a `FullAccessDynamic<>` instance of the value's type.

To perform equality checks correctly, perform them using the value returned by `Unwrap` or cast it to the underlying type. To help you do this, you can also keep a reference to the instance typed as `FullAccessDynamic<>` and perform equality checks using that reference. `FullAccessDynamic<T>` implements `IEquatable<FullAccessDynamic<T>>` and `IEquatable<T>` with all comparisons targeting the underlying value (the equality and inequality operators are also overloaded to use `Equals`).

## `GetType()`

`object.GetType()` never binds dynamically, the compiler will always call the method directly on the method without any dynamic binding. This means that calling `GetType()` on a `FullAccessDynamic<T>` instance typed as `dynamic` will always return `FullAccessDynamic<T>`.

## Inheritance

While `FullAccessDynamic<T>` will bind to members defined in base classes that `T` is derived from (or interfaces it implements, since interface member declarations are considered declared members of the implementing type), it will only do so if one of the following conditions is met:
- The member is defined in `T` itself.
- The member is defined in a base class of `T`, either directly or indirectly (through any further base classes), and it is visible to derived classes (i.e. it must not be `private`).

Essentially, this means that `private` members of base classes will not be accessible through `FullAccessDynamic<T>` since `T` knows nothing about those members. To facilitate this, use the `Cast()` method and specify the base type the member is defined in.

Finally, binding will always prioritize members declared in `T` itself over members declared in base classes or interfaces. That is, if `T` redeclares or overrides a member (that is, hiding the member it receives from its `base`), the access will always bind to the member declared in `T` itself.