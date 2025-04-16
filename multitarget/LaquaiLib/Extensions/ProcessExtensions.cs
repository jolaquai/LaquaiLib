using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Process"/> Type.
/// </summary>
public static partial class ProcessExtensions
{
    private static partial class Interop
    {
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetProcessAffinityMask(nint hProcess, nint dwProcessAffinityMask);
    }

    // lotsa funny bit shifting in this one

    /// <summary>
    /// Sets the processor affinity mask for the specified <see cref="Process"/>, including the specified processors.
    /// </summary>
    /// <param name="process">The <see cref="Process"/> instance.</param>
    /// <param name="mask">A bit mask that specifies the set of processors on which the threads of the process can run. A value equivalent to <c>0</c> re-allows all processors.</param>
    /// <param name="discard">Whether to discard the bits higher than the number of logical processors available to the system. Prevents <see cref="ArgumentOutOfRangeException"/>, but may cause invalid inputs to be accepted, still <see langword="true"/> by default, however.</param>
    /// <returns><see langword="true"/> if the new affinity mask could be set, otherwise <see langword="false"/>.</returns>
    public static bool SetAffinity(this Process process, ulong mask, bool discard = true)
    {
        if (mask == 0ul)
        {
            mask = ~0ul;
        }
        if (discard)
        {
            // shifting left, then subtracting 1 is basically a ~0 mask up to bit shifted left to
            mask &= (1ul << Environment.ProcessorCount) - 1;
        }
        if (1ul << Environment.ProcessorCount <= mask)
        {
            throw new ArgumentOutOfRangeException(nameof(mask), $"The bit mask '0b{mask.AsBinary()}' specifies more than the {Environment.ProcessorCount} logical processors available to the system.");
        }
        if (process.HasExited)
        {
            throw new InvalidOperationException("The specified process has exited.");
        }
        return Interop.SetProcessAffinityMask(process.Handle, (nint)mask);
    }
    /// <summary>
    /// Sets the processor affinity mask for the specified <see cref="Process"/>, excluding the specified processors.
    /// Only the lower bits within <see cref="Environment.ProcessorCount"/> are considered.
    /// </summary>
    /// <param name="process">The <see cref="Process"/> instance.</param>
    /// <param name="mask">A bit mask that specifies the set of processors on which the threads of the process may not run.</param>
    /// <returns><see langword="true"/> if the new affinity mask could be set, otherwise <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SetNegativeAffinity(this Process process, ulong mask) => process.SetAffinity(~mask);
    /// <summary>
    /// Sets a processor affinity mask for the specified <see cref="Process"/> that disallows use of the first quarter of logical processors.
    /// This is sometimes practical when there are enough logical processors available. The system usually assigns many background processes to the first few logical processes, so this may help increase responsiveness of CPU-heavy applications.
    /// </summary>
    /// <param name="process">The <see cref="Process"/> instance.</param>
    /// <returns><see langword="true"/> if the new affinity mask could be set, otherwise <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // same as above, plus an additional right-shift by 2 to divide by 4
    public static bool SetGamingAffinity(this Process process) => process.SetNegativeAffinity((1ul << (Environment.ProcessorCount >> 2)) - 1);
}
