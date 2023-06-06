namespace LaquaiLib.Classes.Collections;

/// <summary>
/// Represents a strongly typed list of objects that can be accessed by index. Provides methods to search, sort, and manipulate lists and generally everything a <see cref="List{T}"/> can do plus everything you ever wished it could.
/// </summary>
/// <typeparam name="T">The Type of the elements in the list.</typeparam>
public class WishList<T> : List<T>
{
    #region Constructors
    /// <summary>
    /// Instantiates a new <see cref="WishList{T}"/> that is empty and has the default capacity.
    /// </summary>
    public WishList() : base()
    {
    }

    /// <summary>
    /// Initializes a new <see cref="WishList{T}"/> that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection to copy items from.</param>
    public WishList(IEnumerable<T> collection) : base(collection)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="WishList{T}"/> that is empty and has the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The initial capacity of the list.</param>
    public WishList(int capacity) : base(capacity)
    {
    }

    /// <summary>
    /// Instantiates a new <see cref="WishList{T}"/> that contains the specified items and has a capacity equal to the number of items.
    /// </summary>
    /// <param name="items">The items for the list to contain.</param>
    public WishList(params T[] items) : base(items.Length)
    {
        AddRange(items);
    }
    #endregion

    #region Indexers
    /// <summary>
    /// Gets or sets the element at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">An <see cref="Index"/> instance that identifies the location of the element to get or set.</param>
    /// <returns>The element at the specified <paramref name="index"/>.</returns>
    public T this[Index index] {
        get => base[index];
        set => base[index] = value;
    }

    /// <summary>
    /// Gets or sets elements within the specified <paramref name="range"/>.
    /// </summary>
    /// <param name="range">The <see cref="Range"/> in which to get or set elements.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the items that were get or set.</returns>
    public IEnumerable<T> this[Range range] {
        get {
            var (offset, length) = range.GetOffsetAndLength(Count);
            for (var i = offset; i < offset + length; i++)
            {
                yield return this[i];
            }
        }
        set {
            var (offset, length) = range.GetOffsetAndLength(Count);
            for (var i = offset; i < offset + length; i++)
            {
                this[i] = value.ElementAt(i - offset);
            }
        }
    }
    #endregion
}
