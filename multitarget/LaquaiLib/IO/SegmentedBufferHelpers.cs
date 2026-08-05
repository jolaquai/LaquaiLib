namespace LaquaiLib.IO;

internal static class SegmentedBufferHelpers
{
    public static (int SegmentIndex, int OffsetInSegment) AbsoluteToRelative<T>(ReadOnlySpan<T[]> segments, long index)
    {
        // Fast zero-seek path
        if (index == 0)
            return (0, 0);

        long length = 0;
        for (var i = 0; i < segments.Length; i++)
        {
            var len = segments[i].Length;
            length += len;
            if (index < length)
                return (i, (int)(index - (length - len)));
        }
        throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range of the buffer.");
    }

    public static long RelativeToAbsolute<T>(ReadOnlySpan<T[]> segments, int segment, int index)
    {
        long length = 0;
        for (var i = 0; i < segment; i++)
            length += segments[i].Length;
        length += index;
        return length;
    }
}