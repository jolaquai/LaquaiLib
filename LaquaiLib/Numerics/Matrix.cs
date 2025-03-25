using System.Collections;
using System.Numerics;

using LaquaiLib.Extensions;
using LaquaiLib.Interfaces;

namespace LaquaiLib.Numerics;

/// <summary>
/// Represents a 2-dimensional matrix of arbitrary sizes.
/// </summary>
public readonly struct Matrix<T> : IEnumerable<T>,
    IEquatable<Matrix<T>>,
    IStructuralEquatable,
    ICloneable<Matrix<T>>,
    IAdditionOperators<Matrix<T>, Matrix<T>, Matrix<T>>,
    ISubtractionOperators<Matrix<T>, Matrix<T>, Matrix<T>>,
    IMultiplyOperators<Matrix<T>, Matrix<T>, Matrix<T>>,
    IMultiplyOperators<Matrix<T>, T, Matrix<T>>,
    IDivisionOperators<Matrix<T>, Matrix<T>, Matrix<T>>,
    IDivisionOperators<Matrix<T>, T, Matrix<T>>
    where T : ISignedNumber<T>, IComparisonOperators<T, T, bool>, IModulusOperators<T, T, T>
{
    private readonly T[,] _data;

    /// <summary>
    /// Initializes a new <see cref="Matrix{T}"/> using the specified <see cref="Vector{T}"/>s.
    /// The number of vectors dictates the width of the matrix and the length of the longest vector dictates the height. All missing values are filled with the default value of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="vectors">The vectors to use for the matrix.</param>
    public Matrix(params ReadOnlySpan<Vector<T>> vectors)
    {
        if (vectors.Length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(vectors), "At least one vector must be provided.");
        }

        var max = 0;
        for (var i = 0; i < vectors.Length; i++)
        {
            if (vectors[i].Dimension > max)
            {
                max = vectors[i].Dimension;
            }
        }
        _data = new T[vectors.Length, max];

        for (var i = 0; i < vectors.Length; i++)
        {
            for (var j = 0; j < vectors[i].Dimension; j++)
            {
                _data[i, j] = vectors[i][j];
            }
        }

        Rows = _data.GetLength(0);
        Columns = _data.GetLength(1);
        IsSquare = Rows == Columns;
        _toString = $"Matrix<{TypeExtensions.AsKeyword(typeof(T).FullName)}>[{Rows}x{Columns}]";
    }
    /// <summary>
    /// Initializes a new <see cref="Matrix{T}"/> using the specified arrays of <typeparamref name="T"/>.
    /// The number of arrays dictates the height of the matrix and the length of the longest array dictates the width. All missing values are filled with the default value of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="arrays">The arrays to use for the matrix.</param>
    public Matrix(params T[][] arrays)
    {
        if (arrays.Length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(arrays), "At least one array must be provided.");
        }
        if (arrays.All(a => a.Length == 0))
        {
            throw new ArgumentOutOfRangeException(nameof(arrays), "At least one array must not be empty.");
        }

        var max = 0;
        for (var i = 0; i < arrays.Length; i++)
        {
            if (arrays[i].Length > max)
            {
                max = arrays[i].Length;
            }
        }
        _data = new T[arrays.Length, max];
        for (var i = 0; i < arrays.Length; i++)
        {
            for (var j = 0; j < arrays[i].Length; j++)
            {
                _data[i, j] = arrays[i][j];
            }
        }

        Rows = _data.GetLength(0);
        Columns = _data.GetLength(1);
        IsSquare = Rows == Columns;
        _toString = $"Matrix<{TypeExtensions.AsKeyword(typeof(T).FullName)}>[{Rows}x{Columns}]";
    }
    /// <summary>
    /// Initializes a new <see cref="Matrix{T}"/> using the specified 2D array of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="data">The 2D array to use for the matrix.</param>
    public Matrix(T[,] data)
    {
        if (data.Length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(data), "The data array must not be zero-sized.");
        }

        _data = System.Runtime.CompilerServices.Unsafe.As<T[,]>(data.Clone());

        Rows = _data.GetLength(0);
        Columns = _data.GetLength(1);
        IsSquare = Rows == Columns;
        _toString = $"Matrix<{TypeExtensions.AsKeyword(typeof(T).FullName)}>[{Rows}x{Columns}]";
    }
    /// <summary>
    /// Initializes a new <see cref="Matrix{T}"/> of size <c><paramref name="rows"/> x <paramref name="columns"/></c> and <paramref name="values"/> as the elements, distributed row-wise.
    /// </summary>
    /// <param name="rows">The number of rows in the matrix.</param>
    /// <param name="columns">The number of columns in the matrix.</param>
    /// <param name="values">The values to use for the matrix.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the number of values does not match the size of the matrix.</exception>
    public Matrix(int rows, int columns, params T[] values)
    {
        if (values.Length != rows * columns)
        {
            throw new ArgumentOutOfRangeException(nameof(values), "The number of values must match the size of the matrix.");
        }

        _data = new T[rows, columns];
        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < columns; j++)
            {
                _data[i, j] = values[(i * columns) + j];
            }
        }

        Rows = rows;
        Columns = columns;
        IsSquare = Rows == Columns;
        _toString = $"Matrix<{TypeExtensions.AsKeyword(typeof(T).FullName)}>[{Rows}x{Columns}]";
    }
    /// <inheritdoc cref="Matrix{T}.Matrix(int, int, T[])"/>"
    public Matrix(int rows, int columns, params ReadOnlySpan<T> values)
    {
        if (values.Length != rows * columns)
        {
            throw new ArgumentOutOfRangeException(nameof(values), "The number of values must match the size of the matrix.");
        }

        _data = new T[rows, columns];
        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < columns; j++)
            {
                _data[i, j] = values[(i * columns) + j];
            }
        }

        Rows = rows;
        Columns = columns;
        IsSquare = Rows == Columns;
        _toString = $"Matrix<{TypeExtensions.AsKeyword(typeof(T).FullName)}>[{Rows}x{Columns}]";
    }

    /// <summary>
    /// Gets the value at the specified <paramref name="row"/> and <paramref name="column"/> in the matrix.
    /// </summary>
    /// <param name="row">The row to get the value from.</param>
    /// <param name="column">The column to get the value from.</param>
    /// <returns>The value at the specified <paramref name="row"/> and <paramref name="column"/>.</returns>
    public readonly T this[int row, int column] => _data[row, column];
    /// <summary>
    /// Extracts a row from the matrix.
    /// </summary>
    /// <param name="row">The row to extract.</param>
    /// <returns>The row as an array.</returns>
    public T[] GetRow(int row)
    {
        var result = new T[Columns];
        IReadOnlySpanProvider<T> spanProvider = null;
        try
        {
            _data.TryGetReadOnlySpan(out spanProvider, out var ros);
            ros.Slice(row * Columns, Columns).CopyTo(result);
        }
        finally
        {
            spanProvider?.Dispose();
        }
        return result;
    }
    /// <summary>
    /// Extracts a column from the matrix.
    /// </summary>
    /// <param name="column">The column to extract.</param>
    /// <returns>The column as an array.</returns>
    public T[] GetColumn(int column)
    {
        // Can't do span copying here since values in a column aren't contiguous in memory
        var result = new T[Rows];
        for (var i = 0; i < Rows; i++)
        {
            result[i] = _data[i, column];
        }
        return result;
    }
    /// <summary>
    /// Extracts a column from the matrix and wraps it in a <see cref="Vector{T}"/>.
    /// </summary>
    /// <param name="column">The column to extract.</param>
    /// <returns>The column as a <see cref="Vector{T}"/>.</returns>
    public Vector<T> GetColumnAsVector(int column)
    {
        var result = new T[Rows];
        for (var i = 0; i < Rows; i++)
        {
            result[i] = _data[i, column];
        }
        return new Vector<T>(result);
    }
    /// <summary>
    /// Gets all rows in the matrix as arrays.
    /// </summary>
    /// <returns>All rows in the matrix as arrays.</returns>
    public T[][] GetRows()
    {
        var result = new T[Rows][];
        // Do this here again to prevent multiple calls to GetRow, which will box the IReadOnlySpanProvider on every call
        IReadOnlySpanProvider<T> spanProvider = null;
        try
        {
            _data.TryGetReadOnlySpan(out spanProvider, out var ros);
            for (var i = 0; i < Rows; i++)
            {
                result[i] = new T[Columns];
                ros.Slice(i * Columns, Columns).CopyTo(result[i]);
            }
        }
        finally
        {
            spanProvider?.Dispose();
        }
        return result;
    }
    /// <summary>
    /// Gets all columns in the matrix as arrays.
    /// </summary>
    /// <returns></returns>
    public T[][] GetColumns()
    {
        var result = new T[Columns][];
        for (var i = 0; i < Columns; i++)
        {
            result[i] = GetColumn(i);
        }
        return result;
    }
    /// <summary>
    /// Gets all columns in the matrix as <see cref="Vector{T}"/>s.
    /// </summary>
    /// <returns></returns>
    public Vector<T>[] GetColumnsAsVectors()
    {
        var result = new Vector<T>[Columns];
        for (var i = 0; i < Columns; i++)
        {
            result[i] = GetColumnAsVector(i);
        }
        return result;
    }
    /// <summary>
    /// Gets a copy of entire backing store of the matrix as a 2D array.
    /// </summary>
    /// <returns>A copy of the backing store of the matrix as a 2D array.</returns>
    public T[,] ToArray() => System.Runtime.CompilerServices.Unsafe.As<T[,]>(_data.Clone());

    /// <summary>
    /// Gets the number of columns in the matrix.
    /// </summary>
    public int Columns { get; }
    /// <summary>
    /// Gets the number of rows in the matrix.
    /// </summary>
    public int Rows { get; }
    /// <summary>
    /// Gets the rank of the matrix (that is, the number of linearly independent rows or columns, or equivalently, the number of non-zero rows in the row-echelon form).
    /// </summary>
    public int Rank
    {
        get
        {
            if (!IsRowEchelonForm)
            {
                return GetRowEchelonForm().Rank;
            }

            // Count the number of non-zero rows
            var count = 0;
            for (var i = 0; i < Rows; i++)
            {
                for (var j = 0; j < Columns; j++)
                {
                    if (!_data[i, j].Equals(T.Zero))
                    {
                        count++;
                        break;
                    }
                }
            }
            return count;
        }
    }
    /// <summary>
    /// Gets whether the matrix is square.
    /// </summary>
    public bool IsSquare { get; }
    /// <summary>
    /// Gets whether the matrix is in upper-triangular form (that is, all elements below the diagonal are zero).
    /// </summary>
    public bool IsUpperTriangularForm
    {
        get
        {
            if (!IsSquare)
            {
                return false;
            }

            for (var i = 1; i < Rows; i++)
            {
                for (var j = 0; j < i; j++)
                {
                    if (!_data[i, j].Equals(T.Zero))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
    /// <summary>
    /// Gets whether the matrix is in lower-triangular form (that is, all elements above the diagonal are zero).
    /// </summary>
    public bool IsLowerTriangularForm
    {
        get
        {
            if (!IsSquare)
            {
                return false;
            }

            for (var i = 0; i < Rows; i++)
            {
                for (var j = i + 1; j < Columns; j++)
                {
                    if (!_data[i, j].Equals(T.Zero))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
    /// <summary>
    /// Gets whether the matrix is in row-echelon form.
    /// </summary>
    public bool IsRowEchelonForm
    {
        get
        {
            if (Rows == 1)
            {
                return true;
            }
            // If the matrix is upper-triangular, it's also square and thus in row-echelon form, since that's the same thing for square matrices
            if (IsUpperTriangularForm)
            {
                return true;
            }

            var colCount = Columns;
            var rowCount = Rows;
            IReadOnlySpanProvider<T> rosProvider = null;
            try
            {
                _data.TryGetReadOnlySpan(out rosProvider, out var ros);

                bool IsAllZerosFrom(ReadOnlySpan<T> span, int row)
                {
                    if (row >= rowCount)
                    {
                        return true;
                    }

                    // Get a span over the remaining rows from the current row
                    var rowSpan = span[(row * colCount)..];
                    // Check if all elements are zero
                    for (var i = 0; i < rowSpan.Length; i++)
                    {
                        if (!rowSpan[i].Equals(T.Zero))
                        {
                            return false;
                        }
                    }
                    return true;
                }

                // We'll track the column index of the previous row's pivot.
                var pivotCol = -1;
                for (var r = 0; r < Rows; r++)
                {
                    // Find the leftmost non-zero entry in this row
                    var firstNonZero = -1;
                    for (var c = 0; c < Columns; c++)
                    {
                        if (_data[r, c] != T.Zero)
                        {
                            firstNonZero = c;
                            break;
                        }
                    }

                    if (firstNonZero < 0)
                    {
                        // This row is all zero; the rest must also be zero if it's REF
                        return IsAllZerosFrom(ros, r + 1);
                    }
                    else
                    {
                        // Condition: pivot must be strictly to the right compared to previous row's pivot
                        if (firstNonZero <= pivotCol)
                        {
                            return false;
                        }

                        pivotCol = firstNonZero;

                        // Ensure all entries below this pivot in the same column are zero
                        for (var rr = r + 1; rr < Rows; rr++)
                        {
                            if (_data[rr, pivotCol] != T.Zero)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            finally
            {
                rosProvider?.Dispose();
            }
        }
    }
    /// <summary>
    /// Gets whether the matrix is in reduced row-echelon form (that is, it must be in row-echelon form and all leading coefficients must be <c>1</c>).
    /// All matrices in reduced row-echelon form are also in row-echelon form, but not vice versa.
    /// </summary>
    public bool IsReducedRowEchelonForm
    {
        get
        {
            // Must first be in row echelon form
            if (!IsRowEchelonForm)
            {
                return false;
            }

            for (var r = 0; r < Rows; r++)
            {
                // Find the leftmost non-zero entry in this row
                var firstNonZero = -1;
                for (var c = 0; c < Columns; c++)
                {
                    if (_data[r, c] != T.Zero)
                    {
                        firstNonZero = c;
                        break;
                    }
                }

                // If row is all zero, nothing more to check for RREF in this row
                if (firstNonZero < 0)
                {
                    continue;
                }

                // Check pivot = 1
                if (_data[r, firstNonZero] != T.One)
                {
                    return false;
                }

                // Check it's the only non-zero in that column
                for (var rr = 0; rr < Rows; rr++)
                {
                    if (rr != r && _data[rr, firstNonZero] != T.Zero)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Calculates the determinant of the matrix.
    /// Invariably throws an <see cref="InvalidOperationException"/> if this instance does not represent a square matrix.
    /// </summary>
    public T Determinant
    {
        get
        {
            if (!IsSquare)
            {
                throw new InvalidOperationException("The matrix must be square to calculate the determinant.");
            }
            // Fast paths for up to 3x3
            return Columns switch
            {
                1 => _data[0, 0],
                2 => (_data[0, 0] * _data[1, 1]) - (_data[0, 1] * _data[1, 0]),
                // Expand along the first row
                3 => (_data[0, 0] * ((_data[1, 1] * _data[2, 2]) - (_data[1, 2] * _data[2, 1])))
                   - (_data[0, 1] * ((_data[1, 0] * _data[2, 2]) - (_data[1, 2] * _data[2, 0])))
                   + (_data[0, 2] * ((_data[1, 0] * _data[2, 1]) - (_data[1, 1] * _data[2, 0]))),
                // Otherwise fall back to row-reduction
                _ => DetRowReduce(),
            };
        }
    }
    private T DetRowReduce()
    {
        var n = _data.GetLength(0);
        var mat = System.Runtime.CompilerServices.Unsafe.As<T[,]>(_data.Clone());
        var det = T.One;

        for (var i = 0; i < n; i++)
        {
            // Pivot selection (partial pivot)
            var pivotRow = i;
            var pivotVal = T.Abs(mat[i, i]);
            for (var r = i + 1; r < n; r++)
            {
                var val = T.Abs(mat[r, i]);
                if (val > pivotVal)
                {
                    pivotVal = val;
                    pivotRow = r;
                }
            }
            // Swap rows if needed
            if (pivotRow != i)
            {
                for (var c = 0; c < n; c++)
                {
                    (mat[i, c], mat[pivotRow, c]) = (mat[pivotRow, c], mat[i, c]);
                }
                det = -det;
            }
            // If pivot is zero => determinant is zero
            if (mat[i, i] == T.Zero)
            {
                return T.Zero;
            }

            // Multiply by pivot on diagonal
            det *= mat[i, i];

            // Eliminate below pivot
            for (var r = i + 1; r < n; r++)
            {
                var factor = mat[r, i] / mat[i, i];
                for (var c = i; c < n; c++)
                {
                    mat[r, c] -= factor * mat[i, c];
                }
            }
        }

        return det;
    }

    /// <summary>
    /// Adds two <see cref="Matrix{T}"/>s.
    /// </summary>
    /// <param name="left">The first matrix to add.</param>
    /// <param name="right">The second matrix to add.</param>
    /// <returns>The result of the addition.</returns>
    public static Matrix<T> operator +(Matrix<T> left, Matrix<T> right)
    {
        if (left.Rows != right.Rows || left.Columns != right.Columns)
        {
            throw new InvalidOperationException("The matrices must have the same size to multiply element-wise.");
        }

        var mat = System.Runtime.CompilerServices.Unsafe.As<T[,]>(left._data.Clone());
        for (var i = 0; i < left.Rows; i++)
        {
            for (var j = 0; j < left.Columns; j++)
            {
                mat[i, j] += right[i, j];
            }
        }
        return new Matrix<T>(mat);
    }
    /// <summary>
    /// Subtracts a <see cref="Matrix{T}"/> from another.
    /// </summary>
    /// <param name="left">The matrix to subtract from.</param>
    /// <param name="right">The matrix to subtract.</param>
    /// <returns>The result of the subtraction.</returns>
    public static Matrix<T> operator -(Matrix<T> left, Matrix<T> right)
    {
        if (left.Rows != right.Rows || left.Columns != right.Columns)
        {
            throw new InvalidOperationException("The matrices must have the same size to multiply element-wise.");
        }

        var mat = System.Runtime.CompilerServices.Unsafe.As<T[,]>(left._data.Clone());
        for (var i = 0; i < left.Rows; i++)
        {
            for (var j = 0; j < left.Columns; j++)
            {
                mat[i, j] -= right[i, j];
            }
        }
        return new Matrix<T>(mat);
    }
    /// <summary>
    /// Multiplies two <see cref="Matrix{T}"/>s.
    /// The number of columns in the left matrix must be equal to the number of rows in the right matrix.
    /// </summary>
    /// <param name="left">The first matrix to multiply.</param>
    /// <param name="right">The second matrix to multiply.</param>
    /// <returns></returns>
    public static Matrix<T> operator *(Matrix<T> left, Matrix<T> right)
    {
        // This is different from multiplying two vectors together, since when one thinks of multiplying two matrices, it's almost always typical matrix multiplication
        // Another indication that the Hadamard product is not the default is that it's sometimes called "naive matrix multiplication" or "element-wise matrix multiplication"

        if (left.Columns != right.Rows)
        {
            throw new InvalidOperationException("The number of columns in the left matrix must be equal to the number of rows in the right matrix.");
        }

        var result = new T[left.Rows, right.Columns];
        for (var i = 0; i < left.Rows; i++)
        {
            for (var j = 0; j < right.Columns; j++)
            {
                var sum = T.Zero;
                for (var k = 0; k < left.Columns; k++)
                {
                    sum += left._data[i, k] * right._data[k, j];
                }
                result[i, j] = sum;
            }
        }
        return new Matrix<T>(result);
    }
    /// <summary>
    /// Multiplies a <see cref="Matrix{T}"/> by a scalar.
    /// </summary>
    /// <param name="left">The matrix to multiply.</param>
    /// <param name="right">The scalar to multiply by.</param>
    /// <returns>The result of the multiplication.</returns>
    public static Matrix<T> operator *(Matrix<T> left, T right)
    {
        var mat = System.Runtime.CompilerServices.Unsafe.As<T[,]>(left._data.Clone());
        for (var i = 0; i < left.Rows; i++)
        {
            for (var j = 0; j < left.Columns; j++)
            {
                mat[i, j] *= right;
            }
        }
        return new Matrix<T>(mat);
    }
    /// <summary>
    /// Determines if this <see cref="Matrix{T}"/> is equal to another.
    /// </summary>
    /// <param name="left">The first <see cref="Matrix{T}"/>.</param>
    /// <param name="right">The second <see cref="Matrix{T}"/>.</param>
    /// <returns><see langword="true"/> if the matrices are equal, otherwise <see langword="false"/>.</returns>
    public static bool operator ==(Matrix<T> left, Matrix<T> right) => left.Equals(right);
    /// <summary>
    /// Determines if this <see cref="Matrix{T}"/> is not equal to another.
    /// </summary>
    /// <param name="left">The first <see cref="Matrix{T}"/>.</param>
    /// <param name="right">The second <see cref="Matrix{T}"/>.</param>
    /// <returns><see langword="true"/> if the matrices are not equal, otherwise <see langword="false"/>.</returns>
    public static bool operator !=(Matrix<T> left, Matrix<T> right) => !(left == right);
    /// <summary>
    /// "Divides" a <see cref="Matrix{T}"/> by another.
    /// </summary>
    /// <param name="left">The matrix to divide.</param>
    /// <param name="right">The matrix to divide by.</param>
    /// <returns>The result of the division.</returns>
    /// <remarks>
    /// Since matrix division is not well defined, this is equivalent to multiplying the left matrix by the inverse of the right matrix.
    /// This fails if the right matrix is not invertible.
    /// </remarks>
    public static Matrix<T> operator /(Matrix<T> left, Matrix<T> right) => left * right.Inverse();
    /// <summary>
    /// "Divides" a <see cref="Matrix{T}"/> by a scalar.
    /// </summary>
    /// <param name="left">The matrix to divide.</param>
    /// <param name="right">The scalar to divide by.</param>
    /// <returns>The result of the division.</returns>
    public static Matrix<T> operator /(Matrix<T> left, T right)
    {
        var mat = System.Runtime.CompilerServices.Unsafe.As<T[,]>(left._data.Clone());
        for (var i = 0; i < left.Rows; i++)
        {
            for (var j = 0; j < left.Columns; j++)
            {
                mat[i, j] /= right;
            }
        }
        return new Matrix<T>(mat);
    }

    /// <summary>
    /// Creates an identity matrix of the specified size (that is, a square matrix with ones on the diagonal and zeros elsewhere).
    /// </summary>
    /// <param name="size">The size of the identity matrix.</param>
    /// <returns>An identity matrix of the specified size.</returns>
    public static Matrix<T> Identity(int size)
    {
        var result = new T[size, size];
        result.Initialize(T.Zero);
        for (var i = 0; i < size; i++)
        {
            result[i, i] = T.One;
        }
        return new Matrix<T>(result);
    }
    /// <summary>
    /// Creates a zero matrix of the specified size (that is, a matrix with all elements set to zero).
    /// </summary>
    /// <param name="rows">The number of rows in the matrix.</param>
    /// <param name="columns">The number of columns in the matrix.</param>
    /// <returns>A zero matrix of the specified size.</returns>
    public static Matrix<T> Zero(int rows, int columns)
    {
        var result = new T[rows, columns];
        result.Initialize(T.Zero);
        return new Matrix<T>(result);
    }

    /// <summary>
    /// Creates a new <see cref="Matrix{T}"/> by bringing this matrix into row-echelon form.
    /// This does not reduce the rows; see <see cref="GetReducedRowEchelonForm"/> for that.
    /// </summary>
    /// <returns>The row-echelon form of this matrix.</returns>
    public Matrix<T> GetRowEchelonForm()
    {
        if (IsRowEchelonForm)
        {
            return this;
        }

        var rowCount = Rows;
        var colCount = Columns;
        var data = System.Runtime.CompilerServices.Unsafe.As<T[,]>(_data.Clone());

        // We perform a standard Gaussian elimination without pivot normalization.
        for (var pivotIndex = 0; pivotIndex < rowCount && pivotIndex < colCount; pivotIndex++)
        {
            // Partial pivot: find row with largest absolute pivot in the current column
            var pivotRow = pivotIndex;
            var maxVal = T.Abs(data[pivotRow, pivotIndex]);
            for (var r = pivotIndex + 1; r < rowCount; r++)
            {
                var candidate = T.Abs(data[r, pivotIndex]);
                if (candidate > maxVal)
                {
                    maxVal = candidate;
                    pivotRow = r;
                }
            }

            // If pivot is zero after searching, the entire column is zero from pivotIndex down, so skip
            if (data[pivotRow, pivotIndex] == T.Zero)
            {
                continue;
            }

            // Swap pivot row if needed
            if (pivotRow != pivotIndex)
            {
                for (var c = 0; c < colCount; c++)
                {
                    (data[pivotIndex, c], data[pivotRow, c]) = (data[pivotRow, c], data[pivotIndex, c]);
                }
            }

            // Eliminate below pivot
            for (var r = pivotIndex + 1; r < rowCount; r++)
            {
                var factor = data[r, pivotIndex] / data[pivotIndex, pivotIndex];
                if (factor != T.Zero)
                {
                    for (var c = pivotIndex; c < colCount; c++)
                    {
                        data[r, c] -= factor * data[pivotIndex, c];
                    }
                }
            }
        }

        return new Matrix<T>(data);
    }
    /// <summary>
    /// Creates a new <see cref="Matrix{T}"/> by bringing this matrix into reduced row-echelon form.
    /// </summary>
    /// <returns>The row-echelon form of this matrix.</returns>
    public Matrix<T> GetReducedRowEchelonForm()
    {
        if (!IsRowEchelonForm)
        {
            return GetRowEchelonForm().GetReducedRowEchelonForm();
        }

        // Work on a copy
        var rref = System.Runtime.CompilerServices.Unsafe.As<T[,]>(_data.Clone());
        var rowCount = Rows;
        var colCount = Columns;

        // Go from bottom row to top row to eliminate upward and normalize pivots
        for (var pivotRow = rowCount - 1; pivotRow >= 0; pivotRow--)
        {
            // Locate pivot (first non-zero entry in this row)
            var pivotCol = -1;
            for (var c = 0; c < colCount; c++)
            {
                if (rref[pivotRow, c] != T.Zero)
                {
                    pivotCol = c;
                    break;
                }
            }

            // If no pivot found, row is all zero; skip
            if (pivotCol < 0)
            {
                continue;
            }

            // Normalize this pivot to 1
            var pivotVal = rref[pivotRow, pivotCol];
            if (pivotVal != T.One)
            {
                for (var c = pivotCol; c < colCount; c++)
                {
                    rref[pivotRow, c] = rref[pivotRow, c] / pivotVal;
                }
            }

            // Eliminate above
            for (var r = pivotRow - 1; r >= 0; r--)
            {
                var factor = rref[r, pivotCol];
                if (factor != T.Zero)
                {
                    for (var c = pivotCol; c < colCount; c++)
                    {
                        rref[r, c] -= factor * rref[pivotRow, c];
                    }
                }
            }
        }

        return new Matrix<T>(rref);
    }
    /// <summary>
    /// Performs element-wise ("Hadamard") multiplication of this matrix with another.
    /// The matrices must have the same size.
    /// </summary>
    /// <param name="other">The matrix to multiply with.</param>
    /// <returns>A new <see cref="Matrix{T}"/> representing the element-wise product of this matrix and <paramref name="other"/>.</returns>
    public Matrix<T> MultiplyElementWise(Matrix<T> other)
    {
        if (Rows != other.Rows || Columns != other.Columns)
        {
            throw new InvalidOperationException("The matrices must have the same size to multiply element-wise.");
        }

        var result = new T[Rows, Columns];
        for (var i = 0; i < Rows; i++)
        {
            for (var j = 0; j < Columns; j++)
            {
                result[i, j] = _data[i, j] * other._data[i, j];
            }
        }
        return new Matrix<T>(result);
    }
    /// <summary>
    /// Creates a new <see cref="Matrix{T}"/> that is the transpose of this <see cref="Matrix{T}"/> (that is, the rows become columns and vice versa).
    /// </summary>
    /// <returns>The transpose of this <see cref="Matrix{T}"/>.</returns>
    public Matrix<T> Transpose()
    {
        var transposed = new T[Columns, Rows];
        for (var r = 0; r < Rows; r++)
        {
            for (var c = 0; c < Columns; c++)
            {
                transposed[c, r] = _data[r, c];
            }
        }
        return new Matrix<T>(transposed);
    }
    /// <summary>
    /// Creates a new <see cref="Matrix{T}"/> that is the inverse of this <see cref="Matrix{T}"/>.
    /// </summary>
    /// <returns>The inverse of this <see cref="Matrix{T}"/>.</returns>
    public Matrix<T> Inverse()
    {
        if (!IsSquare)
        {
            throw new InvalidOperationException("The matrix must be square to invert.");
        }

        var n = Rows;
        // Clone original data
        var mat = System.Runtime.CompilerServices.Unsafe.As<T[,]>(_data.Clone());
        // Create identity matrix
        var inv = Identity(n)._data;

        // Perform Gauss-Jordan elimination
        for (var i = 0; i < n; i++)
        {
            // Pivot selection (partial pivot)
            var pivotRow = i;
            var pivotVal = T.Abs(mat[pivotRow, i]);
            for (var r = i + 1; r < n; r++)
            {
                var val = T.Abs(mat[r, i]);
                if (val > pivotVal)
                {
                    pivotVal = val;
                    pivotRow = r;
                }
            }
            // If pivot is zero -> matrix is non-invertible
            if (pivotVal == T.Zero)
            {
                throw new InvalidOperationException("The matrix is singular and cannot be inverted.");
            }

            // Swap rows in both mat and inv if needed
            if (pivotRow != i)
            {
                for (var c = 0; c < n; c++)
                {
                    (mat[i, c], mat[pivotRow, c]) = (mat[pivotRow, c], mat[i, c]);
                    (inv[i, c], inv[pivotRow, c]) = (inv[pivotRow, c], inv[i, c]);
                }
            }

            // Normalize pivot row (make pivot = 1)
            var pivot = mat[i, i];
            for (var c = 0; c < n; c++)
            {
                mat[i, c] = mat[i, c] / pivot;
                inv[i, c] = inv[i, c] / pivot;
            }

            // Eliminate in other rows
            for (var r = 0; r < n; r++)
            {
                if (r != i)
                {
                    var factor = mat[r, i];
                    if (factor != T.Zero)
                    {
                        for (var c = 0; c < n; c++)
                        {
                            mat[r, c] -= factor * mat[i, c];
                            inv[r, c] -= factor * inv[i, c];
                        }
                    }
                }
            }
        }

        return new Matrix<T>(inv);
    }

    /// <summary>
    /// Creates a new <see cref="Matrix{T}"/> that is the result of swapping two rows in this matrix.
    /// </summary>
    /// <param name="row1">The first row to swap.</param>
    /// <param name="row2">The second row to swap.</param>
    /// <returns>The matrix with the specified rows swapped.</returns>
    public Matrix<T> SwapRows(int row1, int row2)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(row1, 0, nameof(row1));
        ArgumentOutOfRangeException.ThrowIfLessThan(row2, 0, nameof(row2));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row1, Rows, nameof(row1));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row2, Rows, nameof(row2));

        var mat = System.Runtime.CompilerServices.Unsafe.As<T[,]>(_data.Clone());
        for (var c = 0; c < Columns; c++)
        {
            (mat[row1, c], mat[row2, c]) = (mat[row2, c], mat[row1, c]);
        }
        return new Matrix<T>(mat);
    }
    /// <summary>
    /// Creates a new <see cref="Matrix{T}"/> that is the result of swapping two columns in this matrix.
    /// </summary>
    /// <param name="column1">The first column to swap.</param>
    /// <param name="column2">The second column to swap.</param>
    /// <returns>The matrix with the specified columns swapped.</returns>
    public Matrix<T> SwapColumns(int column1, int column2)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(column1, 0, nameof(column1));
        ArgumentOutOfRangeException.ThrowIfLessThan(column2, 0, nameof(column2));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column1, Columns, nameof(column1));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column2, Columns, nameof(column2));

        var mat = System.Runtime.CompilerServices.Unsafe.As<T[,]>(_data.Clone());
        for (var r = 0; r < Rows; r++)
        {
            (mat[r, column1], mat[r, column2]) = (mat[r, column2], mat[r, column1]);
        }
        return new Matrix<T>(mat);
    }
    /// <summary>
    /// Gets the leading coefficients of the matrix.
    /// </summary>
    /// <returns>An array of the leading coefficients of the matrix.</returns>
    /// <remarks>
    /// While generally always usable, the leading coefficients are really only useful when obtained from a matrix that is in row-echelon form.
    /// </remarks>
    public T[] GetLeadingCoefficients()
    {
        var result = new T[Rows];
        for (var i = 0; i < Rows; i++)
        {
            for (var j = 0; j < Columns; j++)
            {
                if (!_data[i, j].Equals(T.Zero))
                {
                    result[i] = _data[i, j];
                    break;
                }
            }
        }
        return result;
    }

    private readonly string _toString;
    /// <inheritdoc/>
    public override string ToString() => _toString;

    #region Interface implementations
    /// <inheritdoc/>
    public bool Equals(Matrix<T> other) => _data.SequenceEqual<T>(other._data);
    /// <summary>
    /// Checks if all passed <see cref="Matrix{T}"/>s are equal to this matrix (and as such, each other).
    /// </summary>
    /// <param name="others">The <see cref="Matrix{T}"/>s to check for equality.</param>
    /// <returns><see langword="true"/> if all <see cref="Matrix{T}"/> are equal to this matrix, otherwise <see langword="false"/>.</returns>
    public bool Equals(params ReadOnlySpan<Matrix<T>> others)
    {
        for (var i = 0; i < others.Length; i++)
        {
            if (!_data.SequenceEqual<T>(others[i]._data))
            {
                return false;
            }
        }
        return true;
    }
    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Matrix<T> other && Equals(other);
    /// <inheritdoc/>
    public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<T>.Default);
    bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        if (other is Matrix<T> matrix && matrix._data.Length == _data.Length)
        {
            if (comparer is IEqualityComparer<T> typedComparer)
            {
                return _data.SequenceEqual(matrix._data, typedComparer);
            }
            else
            {
                throw new ArgumentException("The comparer must be of type IEqualityComparer<T>.", nameof(comparer));
            }
        }

        return false;
    }
    int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        var typedComparer = comparer as IEqualityComparer<T>;

        var hc = default(HashCode);
        if (typedComparer is not null)
        {
            // Adapt the approach slightly and use a span over the last 8 elements
            IReadOnlySpanProvider<T> spanProvider = null;
            try
            {
                _data.TryGetReadOnlySpan(out spanProvider, out var ros);
                if (ros.Length >= 8)
                {
                    ros = ros[^8..];
                }
                for (var i = 0; i < ros.Length; i++)
                {
                    hc.Add(typedComparer.GetHashCode(ros[i]));
                }
            }
            finally
            {
                spanProvider?.Dispose();
            }
        }
        else
        {
            // Adapt the approach slightly and use a span over the last 8 elements
            IReadOnlySpanProvider<T> spanProvider = null;
            try
            {
                _data.TryGetReadOnlySpan(out spanProvider, out var ros);
                if (ros.Length >= 8)
                {
                    ros = ros[^8..];
                }
                for (var i = 0; i < ros.Length; i++)
                {
                    hc.Add(comparer.GetHashCode(ros[i]));
                }
            }
            finally
            {
                spanProvider?.Dispose();
            }
        }

        return hc.ToHashCode();
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => (IEnumerator<T>)_data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    /// <inheritdoc/>
    public Matrix<T> Clone() => new Matrix<T>(_data);
    #endregion
}
