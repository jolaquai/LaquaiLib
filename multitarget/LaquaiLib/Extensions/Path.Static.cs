using System.Buffers;

namespace LaquaiLib.Extensions;

#pragma warning disable IDE0058 // Expression value is never used

public static class PathExtensions
{
    extension(Path)
    {
        /// <summary>
        /// If required, surrounds the specified <see langword="string"/> with the specified <paramref name="quoteChar"/>.
        /// </summary>
        /// <param name="value">The <see langword="string"/> to quote.</param>
        /// <param name="quoteChar">The character to use for quoting. Default is <c>"</c>.</param>
        /// <returns>The quoted <see langword="string"/>, or the original <see langword="string"/> if it was already quoted.</returns>
        public static string EnsureQuoted(string value, char quoteChar = '"')
        {
            if (string.IsNullOrEmpty(value))
            {
                return new string(quoteChar, 2);
            }
            if (IsQuoted(value))
            {
                return value;
            }
            if (value.AsSpan().IndexOf(' ') == -1)
            {
                // No need to quote, no spaces
                return value;
            }

            var len = value.Length;
            var startsWith = value.StartsWith(quoteChar);
            var endsWith = value.EndsWith(quoteChar);
            if (!startsWith)
                len++;
            if (!endsWith)
                len++;

            return string.Create(len, (value, quoteChar), (span, state) =>
            {
                var (val, qc) = state;
                var idx = 0;
                if (!startsWith)
                {
                    span[0] = qc;
                    idx++;
                }
                val.AsSpan().CopyTo(span[idx..]);
                idx += val.Length;
                if (!endsWith)
                {
                    span[^1] = qc;
                }
            });
        }

        /// <summary>
        /// Ensures that the specified path ends with a directory separator character.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>The original path if it already ends with a directory separator character, otherwise the path with a directory separator character appended.</returns>
        public static string EnsureEndingDirectorySeparator(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return Path.DirectorySeparatorChar.ToString();
            }

            if (!path.EndsWith(Path.DirectorySeparatorChar) && !path.EndsWith(Path.AltDirectorySeparatorChar))
            {
                return path + Path.DirectorySeparatorChar;
            }
            return path;
        }

        /// <summary>
        /// Trims the specified <paramref name="quoteChar"/> from the start and end of the specified <see langword="string"/>.
        /// </summary>
        /// <param name="value">The <see langword="string"/> to trim.</param>
        /// <param name="quoteChar">The character to trim. Default is <c>"</c>.</param>
        /// <returns>The trimmed <see langword="string"/>.</returns>
        public static string TrimQuotes(string value, char quoteChar = '"') => TrimQuotes(value.AsSpan(), quoteChar).ToString();
        /// <summary>
        /// Trims the specified <paramref name="quoteChar"/> from the start and end of the specified <see cref="ReadOnlySpan{T}"/> of <see langword="char"/>.
        /// </summary>
        /// <param name="value">The <see cref="ReadOnlySpan{T}"/> of <see langword="char"/> to trim.</param>
        /// <param name="quoteChar">The character to trim. Default is <c>"</c>.</param>
        /// <returns>The trimmed <see cref="ReadOnlySpan{T}"/> of <see langword="char"/>.</returns>
        public static ReadOnlySpan<char> TrimQuotes(ReadOnlySpan<char> value, char quoteChar = '"') => value.Trim(quoteChar);

        /// <summary>
        /// Gets whether the specified <see langword="string"/> is surrounded by the specified <paramref name="quoteChar"/>.
        /// </summary>
        /// <param name="value">The <see langword="string"/> to check.</param>
        /// <param name="quoteChar">The character to check for. Default is <c>"</c>.</param>
        /// <returns><see langword="true"/> if the <see langword="string"/> is surrounded by the specified <paramref name="quoteChar"/>, otherwise <see langword="false"/>.</returns>
        public static bool IsQuoted(string value, char quoteChar = '"') => IsQuoted(value.AsSpan(), quoteChar);
        /// <summary>
        /// Gets whether the specified <see cref="ReadOnlySpan{T}"/> of <see langword="char"/> is surrounded by the specified <paramref name="quoteChar"/>.
        /// </summary>
        /// <param name="value">The <see cref="ReadOnlySpan{T}"/> of <see langword="char"/> to check.</param>
        /// <param name="quoteChar">The character to check for. Default is <c>"</c>.</param>
        /// <returns>True if the <see cref="ReadOnlySpan{T}"/> of <see langword="char"/> is surrounded by the specified <paramref name="quoteChar"/>, otherwise <see langword="false"/>.</returns>
        public static bool IsQuoted(ReadOnlySpan<char> value, char quoteChar = '"') => value.Length >= 2 && value[0] == quoteChar && value[^1] == quoteChar;

        /// <summary>
        /// Determines whether the specified path is a Universal Naming Convention (UNC) path.
        /// </summary>
        /// <param name="path">The file or directory path to evaluate.</param>
        /// <returns><see langword="true"/> if the specified path is a UNC path, otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUncPath(string path) => IsUncPath(path.AsSpan());
        /// <summary>
        /// Determines whether the specified path is a Universal Naming Convention (UNC) path.
        /// </summary>
        /// <param name="path">The file or directory path to evaluate.</param>
        /// /// <returns><see langword="true"/> if the specified path is a UNC path, otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUncPath(ReadOnlySpan<char> path) => path.StartsWith(@"\\") && path.Length > 2 && path[2] != Path.DirectorySeparatorChar && path[2] != Path.AltDirectorySeparatorChar;

        /// <summary>
        /// Attempts to resolve a mapped drive letter to its corresponding UNC path.
        /// </summary>
        /// <param name="path">The path that may contain a mapped drive letter.</param>
        /// <returns>A path <see langword="string"/> with a mapped drive letter replaced with its corresponding UNC path, if the drive letter is mapped, otherwise the original <paramref name="path"/>.</returns>
        public static string ResolveMappedDrive(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path))
            {
                return path;
            }
            var root = Path.GetPathRoot(path);
            if (root is null || root.StartsWith(@"\\", StringComparison.Ordinal))
            {
                return path;
            }

            // 1) Mapped network drive -> UNC
            var size = 0;
            var ret = Interop.Mpr.WNetGetUniversalName(path, Interop.Mpr.UNIVERSAL_NAME_INFO_LEVEL, null, ref size);
            if (ret == Mpr.ERROR_MORE_DATA && size > nint.Size)
            {
                var buf = ArrayPool<byte>.Shared.Rent(size);
                try
                {
                    Array.Clear(buf, 0, size); // ensure zeroed for safe pointer read
                    ret = Interop.Mpr.WNetGetUniversalName(path, Interop.Mpr.UNIVERSAL_NAME_INFO_LEVEL, buf, ref size);
                    if (ret == 0)
                    {
                        // UNIVERSAL_NAME_INFO: first pointer is LPWSTR
                        var strPtr = nint.Size == 8 ? (nint)BitConverter.ToInt64(buf, 0) : BitConverter.ToInt32(buf, 0);
                        if (strPtr != 0)
                        {
                            var unc = Marshal.PtrToStringUni(strPtr);
                            if (!string.IsNullOrEmpty(unc))
                            {
                                return unc;
                            }
                        }
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buf);
                }
            }
            if (ret is not (0 or Interop.Mpr.ERROR_NOT_CONNECTED))
            {
                return path;
            }

            // 2) SUBST drive resolution
            var drive = root.TrimEnd('\\'); // "X:"
            var targetBuf = ArrayPool<char>.Shared.Rent(512);
            try
            {
                var chars = Interop.Kernel32.QueryDosDevice(drive, targetBuf, targetBuf.Length);
                if (chars != 0)
                {
                    var target = new string(targetBuf, 0, (int)chars);
                    const string prefix = @"\??\";
                    if (target.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        var realRoot = target.AsSpan(prefix.Length).ToString(); // e.g., "C:\Real\Path"
                        return realRoot + path.Substring(root.Length);
                    }
                }
            }
            finally
            {
                ArrayPool<char>.Shared.Return(targetBuf);
            }

            return path;
        }

        /// <inheritdoc cref="GetParts(ReadOnlySpan{char}, out ReadOnlySpan{char}, out ReadOnlySpan{char}, out ReadOnlySpan{char}, out ReadOnlySpan{char}, out ReadOnlySpan{char})"/>
        public static void GetParts(string path, out string drive, out string directory, out string fileName, out string fileNameWithoutExtension, out string extension)
        {
            GetParts(path.AsSpan(), out var d, out var dir, out var fn, out var fnwoext, out var ext);
            drive = d.ToString();
            directory = dir.ToString();
            fileName = fn.ToString();
            fileNameWithoutExtension = fnwoext.ToString();
            extension = ext.ToString();
        }
        /// <summary>
        /// Splits the specified <paramref name="path"/> into its components.
        /// </summary>
        /// <param name="path">The path to split.</param>
        /// <param name="drive">A reference to the variable that receives the drive component of the path, including the trailing backslash, if any. For UNC paths, this is the server name with leading and trailing backslashes, but without the root share name.</param>
        /// <param name="directory">A reference to the variable that receives the directory component of the path, including the trailing directory separator character, if any.</param>
        /// <param name="fileName">A reference to the variable that receives the file name component of the path, including the extension, if any.</param>
        /// <param name="fileNameWithoutExtension">A reference to the variable that receives the file name component of the path, without the extension.</param>
        /// <param name="extension">A reference to the variable that receives the extension component of the path, including the leading dot.</param>
        public static void GetParts(ReadOnlySpan<char> path, out ReadOnlySpan<char> drive, out ReadOnlySpan<char> directory, out ReadOnlySpan<char> fileName, out ReadOnlySpan<char> fileNameWithoutExtension, out ReadOnlySpan<char> extension)
        {
            drive = default;
            directory = default;
            fileName = default;
            fileNameWithoutExtension = default;
            extension = default;

            if (path.Length == 0)
            {
                return;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool IsSep(char c) => c is '\\' or '/';

            // Drive
            var idx = 0;
            if (path.Length >= 2 && IsSep(path[0]) && IsSep(path[1]))
            {
                // UNC: \\server\share\...
                var i = 2;
                while (i < path.Length && !IsSep(path[i]))
                {
                    i++;
                }
                if (i <= path.Length)
                {
                    // include trailing backslash if present in input
                    if (i < path.Length && IsSep(path[i]))
                    {
                        i++;
                    }

                    drive = path[..i];   // "\\server" or "\\server\"
                    idx = i;
                }
            }
            else if (path.Length >= 2 && path[1] == ':')
            {
                var i = 2;
                if (i < path.Length && IsSep(path[i]))
                {
                    i++;
                }

                drive = path[..i]; // "C:" or "C:\"
                idx = i;
            }
            else
            {
                drive = [];
                idx = 0;
            }

            // Remainder after drive
            var rest = path[idx..];

            // Directory + fileName
            var lastSep = -1;
            for (var i = rest.Length - 1; i >= 0; i--)
            {
                if (IsSep(rest[i])) { lastSep = i; break; }
            }

            if (lastSep >= 0)
            {
                if (lastSep == rest.Length - 1)
                {
                    directory = rest; // ends with separator => no file name
                    fileName = [];
                }
                else
                {
                    directory = rest[..(lastSep + 1)];
                    fileName = rest[(lastSep + 1)..];
                }
            }
            else
            {
                directory = [];
                fileName = rest;
            }

            // File name without extension + extension
            if (fileName.Length == 0)
            {
                fileNameWithoutExtension = [];
                extension = [];
                return;
            }

            var lastDot = -1;
            for (var i = fileName.Length - 1; i >= 0; i--)
            {
                if (fileName[i] == '.')
                {
                    lastDot = i;
                    break;
                }
            }

            if (lastDot > 0)
            {
                fileNameWithoutExtension = fileName[..lastDot];
                extension = fileName[lastDot..]; // includes the dot; "." if dot is last
            }
            else
            {
                fileNameWithoutExtension = fileName;
                extension = [];
            }
        }
    }
}
