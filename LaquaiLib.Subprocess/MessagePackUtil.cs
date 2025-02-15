using LaquaiLib.Extensions;

using MessagePack;

namespace LaquaiLib.Subprocess;

internal static class MessagePackUtil
{
    // Allocate a shared buffer to use when B64-encoding to avoid allocating an intermediary array when callers are only interested in the B64 string
    private static readonly Lock _bufferLock = new Lock();
    private static readonly MemoryStream _b64Buffer = new MemoryStream();

    public static byte[] Serialize(object obj) => MessagePackSerializer.Typeless.Serialize(obj);
    public static void Serialize(object obj, Stream output) => MessagePackSerializer.Typeless.Serialize(output, obj);
    public static string SerializeBase64(object obj)
    {
        lock (_bufferLock)
        {
            _b64Buffer.SetLength(0);
            Serialize(obj, _b64Buffer);
            var underlying = _b64Buffer.AsSpan(0, (int)_b64Buffer.Position);
            return Convert.ToBase64String(underlying);
        }
    }

    public static object Deserialize(byte[] data) => MessagePackSerializer.Typeless.Deserialize(data);
    public static object Deserialize(Stream input) => MessagePackSerializer.Typeless.Deserialize(input);
    public static T Deserialize<T>(byte[] data) => (T)MessagePackSerializer.Typeless.Deserialize(data);
    public static T Deserialize<T>(Stream input) => (T)MessagePackSerializer.Typeless.Deserialize(input);
}
