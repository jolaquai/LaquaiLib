# LaquaiLib
LaquaiLib is my personal little C# library. It contains mainly extension methods for tons of types, documented with XML comments. 

**Note that the intent of this library is NOT to compile it and reference it in your project. It is set to target .NET 8, Windows only (due to the Forms/WPF references). You are supposed to browse through it and copy whatever classes/functionality you need and just stick it into your own projects. The amount of dependencies (especially transitive dependencies) will just pollute your projects if you reference it.**

There is no need to give credit for anything you decide to use in this library. Enjoy!

^(If you have questions or feature/improvement suggestions, message me on Discord @ *`eyeoftheenemy`*)^

## Features
Here's a rough list of what's included:
- Limited collections (`LimitedQueue<T>`)
  - Concurrent limited collections (same as above, but inheriting from the concurrent version of the type)
- Stream wrappers (`MemoryOrFileStream`, `MultiStream`)
- Temp wrappers (`TempAlloc` for arbitrary memory allocation, `TempArray`, `TempDirectory`, `TempFile`)
- XML
  - `XRepetition` (which must be serialized using a custom `XmlWriter` extension)
- Utility classes
  - `ConsoleQueue` (static)
  - `ScreenCapture`
  - `TaskbarProgress` (singleton)
  - `TwoWayLookup`
- Extension methods
  - Third Party
    - `HtmlAgilityPack`
      - `HtmlDocument`
      - `HtmlNode`
  - WPF
  - .NET
    - Any (`object` or generic)
    - `Array` (any `Array` type, including multi-dimensional arrays)
    - `Assembly`
    - `byte[]`
    - `char`
    - `Color`
    - `ConcurrentQueue<T>`
    - `Dictionary<TKey, TValue>`
    - `Enum` (any `Enum` type)
    - `HashAlgorithm`
    - `Icon`
    - `IEnumerable<bool>`
    - `IEnumerable<T>`
    - `IEnumerable<IDisposable>`
    - `IEnumerable<Task>`
    - `List<T>`
    - `MethodInfo`
    - `Queue<T>`
    - `Span<T>`
    - `Stream`
    - `string`
    - `Type` (the actual `Type` type)
    - `XElement`
    - `XmlWriter`
    - `XNode`
- WPF
  - ValueConverters
    - `BoolInverterConverter`
    - `NotNullToBoolConverter`
- `Constants` (which aren't really constants, they're `static readonly`)
- `Miscellaneous`
- `RandomMath`