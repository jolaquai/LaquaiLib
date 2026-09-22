namespace System.Runtime.CompilerServices;

// netstandard2.0 does not ship this, and without it no `init` accessor (and therefore no positional record) compiles
internal static class IsExternalInit;
