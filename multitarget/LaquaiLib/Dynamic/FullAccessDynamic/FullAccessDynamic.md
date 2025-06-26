# FullAccessDynamic<>

`FullAccessDynamic<>` is a wrapper utility that allows access to any member of the wrapped object when cast to `dynamic`.
The type obeys type safety rules, meaning invalid casts are disallowed and will still cause exceptions at run-time.

Additionally, while _not_ being treated as `dynamic`, most operations on `FullAccessDynamic<>` have no special meaning; dynamic binding does not occur.

While the implementation itself is straightforward and sound as-is, there are several issues that need to be addressed by developers using the type.

## Performance

As with any other topic that involves `dynamic`, performance tanks very quickly since dynamic binding is indescribably slow. `FullAccessDynamic<>` has no other chance than to use reflection to gain access to the members of the wrapped object. While some caching is done to improve performance, it is still orders of magnitude slower than using the wrapped object directly.

## Boxing

- `struct`s used to instantiate `FullAccessDynamic<T>` are copied as any `struct` assignment would.
- Anything assigned to `dynamic` is boxed since `dynamic` is implemented on top of `object`. This is also the main reason why `FullAccessDynamic<T>` is a `class`.

## `null` propagation

`null` propagates only indirectly. Because member accesses (when they succeed) obviously have a type (either a property's or method's return type or a field's declared type), any access that succeeds (i.e. the sought member exists at all) will result in a new `FullAccessDynamic<>` of that underlying type.
an access that returns a new `FullAccessDynamic<>` instance of `T`, even if that `T` instance is `null`, looks no different than a return where it is not since creating a new instance of `FullAccessDynamic<T>` with a `null` instance is valid (even if not very useful).

## Type checks and casts

The language will allow cast expressions involving `dynamic` to any type. Dynamic binding will then validate this cast at run-time. `FullAccessDynamic<>` obeys type safety rules, meaning if the underlying type is not assignable to the type being cast to, an `InvalidCastException` is thrown.

However, this has the following side effects. Let `dyn` be an instance of `FullAccessDynamic<T>` typed as `dynamic`, where `T` is some arbitrary other type. Then:
- `dyn is A` will evaluate to `true` if and only if `A` is exactly `FullAccessDynamic<T>`.
- `(A)dyn` will succeed if and only if one of the following is true:
  - `A` is exactly `FullAccessDynamic<T>`, or
  - `A` is either exactly `T` or a type `T` is assignable to.
- `dyn as A` will always return `null` except if and only if `A` is exactly `FullAccessDynamic<T>`.

The above applies to implicit casts as well, such as when assigning `dynamic` to lvalues with a specified type that is not `dynamic`.

## Equality handling

A value returned by some member access through a `FullAccessDynamic<>` instance (that is, in the absense of a cast, as specified above) will never compare equal to the underlying value, even if they were reference-equal. To perform equality checks correctly, perform them using the value returned by `Unwrap` or cast it to the underlying type.