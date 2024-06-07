using System.Management;

namespace LaquaiLib.Streams;

/// <summary>
/// Contains factory methods that produce either <see cref="MemoryStream"/> or <see cref="FileStream"/> instances, depending on the size of the data expected to be written to it.
/// </summary>
public static class MemoryOrFileStream
{
    /// <summary>
    /// The number of bytes at which the stream will switch from a <see cref="MemoryStream"/> to a <see cref="FileStream"/>.
    /// </summary>
    /// <remarks>
    /// You may freely change this value at runtime. Its initial value is 1/512th of the total physical memory of the system (e.g., if your system has 32 GB of total physical memory, this will initially have the value <c>32768 / 512 = 64 MB</c>). If total physical memory cannot be retrieved, the value will default to 64 MB.
    /// </remarks>
    public static int Cutoff { get; set; } = ResetCutoff();
    /// <summary>
    /// Resets the <see cref="Cutoff"/> to the initial value. See the documentation of <see cref="Cutoff"/> for more information.
    /// </summary>
    /// <returns>The new value of <see cref="Cutoff"/>.</returns>
    public static int ResetCutoff()
    {
        try
        {
            using (var computerSystem = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
            {
                foreach (var obj in computerSystem.Get())
                {
                    return Cutoff = Convert.ToInt32(obj["TotalPhysicalMemory"]) / 64;
                }
            }
        }
        catch
        {
        }
        return Cutoff = 64 * 1024 * 1024; // 64 MB
    }

    /// <summary>
    /// Initializes a new <see cref="Stream"/> with the given expected payload size.
    /// </summary>
    /// <param name="payloadSize">The expected size of the payload to be written to this stream. If it exceeds a set <see cref="Cutoff"/>, the internal <see cref="Stream"/> is created as a <see cref="FileStream"/>.</param>
    /// <returns>The created <see cref="Stream"/>.</returns>
    public static Stream Create(int payloadSize)
    {
        return payloadSize >= Cutoff
            ? new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.Asynchronous)
            : new MemoryStream(payloadSize);
    }
    /// <summary>
    /// Initializes a new <see cref="Stream"/> from the specified <paramref name="other"/> <see cref="Stream"/>.
    /// Its data from its current position to the end will be copied to the new <see cref="Stream"/>. Both streams' positions will be advanced by the number of bytes copied.
    /// </summary>
    /// <param name="other">The <see cref="Stream"/> to copy the data from.</param>
    /// <param name="fromBeginning">Whether to seek <paramref name="other"/> to its beginning before copying the data.</param>
    /// <returns>The created <see cref="Stream"/>.</returns>
    public static Stream Create(Stream other, bool fromBeginning = false)
    {
        if (fromBeginning)
        {
            other.Seek(0, SeekOrigin.Begin);
        }
        var stream = Create((int)other.Length);
        other.CopyTo(stream);
        return stream;
    }
    /// <summary>
    /// Initializes a new <see cref="Stream"/> from the specified <paramref name="buffer"/>.
    /// Its data is copied to the new <see cref="Stream"/> and its position advanced by the number of bytes copied.
    /// </summary>
    /// <param name="buffer">The buffer to copy the data from.</param>
    /// <param name="offset">The index in <paramref name="buffer"/> at which to start copying.</param>
    /// <param name="length">The number of bytes to copy from <paramref name="buffer"/>.</param>
    /// <param name="canResize">If the created <see cref="Stream"/> is a <see cref="MemoryStream"/>, whether it can resize itself if the data exceeds its initial capacity.</param>
    /// <param name="canWrite">If the created <see cref="Stream"/> is a <see cref="MemoryStream"/>, whether it is created as writable.</param>
    /// <returns>The created <see cref="Stream"/>.</returns>
    /// <remarks>
    /// <para/><paramref name="canResize"/> and <paramref name="canWrite"/> are ignored if the number of bytes that would be copied exceeds <see cref="Cutoff"/>.
    /// <para/>Calling this constructor with just <paramref name="buffer"/> matches the behavior of <see cref="MemoryStream(byte[])"/>, except if its length exceeds <see cref="Cutoff"/>, in which case a <see cref="FileStream"/> is created.
    /// </remarks>
    public static Stream Create(byte[] buffer, int offset = 0, int length = -1, bool canResize = false, bool canWrite = true)
    {
        if (length == -1)
        {
            length = buffer.Length;
        }
        var realLength = length - offset;
        if (realLength < Cutoff && !canResize)
        {
            return new MemoryStream(buffer, offset, length, canWrite);
        }

        var stream = Create(realLength);
        stream.Write(buffer, offset, length);
        return stream;
    }
}
