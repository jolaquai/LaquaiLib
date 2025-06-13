using System.Diagnostics;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides Extension Methods for the <see cref="StackTrace"/> Type.
/// </summary>
public static class StackTraceExtensions
{
    [UnsafeAccessor(UnsafeAccessorKind.Field)] private static extern ref StackFrame[] _stackFrames(this StackTrace _);
    extension(StackTrace stackTrace)
    {
        /// <summary>
        /// Gets a <see cref="ReadOnlyMemory{T}"/> slice of <see cref="StackFrame"/>s from the <paramref name="stackTrace"/>.
        /// <paramref name="start"/> counts from the end of the <see cref="StackFrame"/> array, to match the behavior of <see cref="StackTrace.GetFrame"/>.
        /// </summary>
        /// <param name="stackTrace">The <see cref="StackTrace"/> to get the <see cref="StackFrame"/>s from.</param>
        /// <param name="start">The start index of the slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>A <see cref="ReadOnlyMemory{T}"/> slice of <see cref="StackFrame"/>s from the <paramref name="stackTrace"/>.</returns>
        /// <remarks>
        /// This directly accesses the underlying <see cref="StackFrame"/> array of the <paramref name="stackTrace"/>.
        /// </remarks>
        public ReadOnlyMemory<StackFrame> GetFrames(int start = 0, int? length = null)
        {
            // Explicitly throw away the ref since even slicing a ReadOnlyMemory<T> would mutate the original array
            var frames = _stackFrames(stackTrace);
            if (Unsafe.IsNullRef(ref frames))
            {
                return default;
            }
            frames = frames[stackTrace.FrameCount..];

            return new ReadOnlyMemory<StackFrame>(frames, start, length ?? frames.Length);
        }
        /// <summary>
        /// Gets the last <see cref="StackFrame"/> from the <paramref name="stackTrace"/>, which typically represents the stack frame where the <paramref name="stackTrace"/> instance was created.
        /// </summary>
        /// <param name="stackTrace">The <see cref="StackTrace"/> to get the last <see cref="StackFrame"/> from.</param>
        /// <returns>The last <see cref="StackFrame"/> from the <paramref name="stackTrace"/>.</returns>
        public StackFrame GetLastFrame() => stackTrace.GetFrame(stackTrace.FrameCount - 1);
    }
}
