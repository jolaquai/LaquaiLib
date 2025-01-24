namespace LaquaiLib.Numerics;

/// <summary>
/// Represents a matrix of arbitrary dimensions.
/// </summary>
public struct Matrix<T>
{
    private T[,] _data;

    /// <summary>
    /// Initializes a new <see cref="Matrix{T}"/> using the specified <see cref="Vector{T}"/>s.
    /// The number of vectors dictates the width of the matrix and the length of the longest vector dictates the height. All missing values are filled with the default value of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="vectors">The vectors to use for the matrix.</param>
    public Matrix(params ReadOnlySpan<Vector<T>> vectors)
    {
        var max = 0;
        for (var i = 0; i < vectors.Length; i++)
        {
            if (vectors[i]. > max)
            {
                max = vectors[i].Length;
            }
        }
        _data = new T[vectors.Length, vectors.Max(v => v.Length)];
    }
}
