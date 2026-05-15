using System.IO;
using System.IO.Compression;
using MessagePack;
using MessagePack.Resolvers;

public static class MemSerlizerUtils
{
    private static readonly MessagePackSerializerOptions _options =
        MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance);

    public static T Deserialize<T>(this byte[] mem) where T : BasicObj
    {
        var decompressed = Decompress(mem);
        return MessagePackSerializer.Deserialize<T>(decompressed, _options);
    }

    public static byte[] Serialize<T>(this T data) where T : BasicObj
    {
        var packed = MessagePackSerializer.Serialize(data, _options);
        return Compress(packed);
    }

    public static byte[] Compress(byte[] data)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
            {
                deflateStream.Write(data, 0, data.Length);
            }
            return memoryStream.ToArray();
        }
    }

    public static byte[] Decompress(byte[] data)
    {
        using (MemoryStream decompressedStream = new MemoryStream())
        {
            using (MemoryStream compressStream = new MemoryStream(data))
            {
                using (DeflateStream deflateStream = new DeflateStream(compressStream, CompressionMode.Decompress))
                {
                    deflateStream.CopyTo(decompressedStream);
                }
            }
            return decompressedStream.ToArray();
        }
    }
}
