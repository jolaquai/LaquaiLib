#warning TODO: Documentation is currently largely inheritdoc'd from System.Linq.Enumerable - Rewrite that

namespace LaquaiLib.Extensions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

internal sealed class SpanLookup<TKey, TElement>(int capacity, IEqualityComparer<TKey> equalityComparer) : ILookup<TKey, TElement>
{
    internal readonly Dictionary<TKey, List<TElement>> _lookup = new Dictionary<TKey, List<TElement>>(capacity, equalityComparer);
    public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
    {
        foreach (var kvp in _lookup)
        {
            yield return new Grouping<TKey, TElement>(kvp.Key, kvp.Value);
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _lookup.Count;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(TKey key) => _lookup.ContainsKey(key);
    public IEnumerable<TElement> this[TKey key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _lookup.TryGetValue(key, out var list) ? list : [];
    }
}

// I'd have loved to make this useless thing a ref struct, but since we're only going to be handed around as IGrouping<TKey, TElement>, I can't
// And not even a normal struct makes sense since the interface cast will box us anyway
internal sealed class Grouping<TKey, TElement>(TKey key, List<TElement> elements) : IGrouping<TKey, TElement>
{
    public TKey Key
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => key;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<TElement> GetEnumerator() => elements.GetEnumerator();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}