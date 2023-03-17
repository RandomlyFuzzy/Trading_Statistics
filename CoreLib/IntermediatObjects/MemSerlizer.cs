using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
public static class MemSerlizerUtils
{
    private const string prefix = "";//"AAEAAAD/////AQAAAAAAAAAMAgAAAD5Db3JlTGliLCBWZXJzaW9uPTEuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49bnVsbAUBAAAAC";

    public static T Deserialize<T>(this byte[] mem) where T : BasicObj
    {
        string s = prefix + Encoding.ASCII.GetString(mem);
        mem = (System.Convert.FromBase64String(s));
        mem = Decompress(mem);
        BinaryFormatter formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011 // Type or member is obsolete
        return formatter.Deserialize(new MemoryStream(mem)) as T;
#pragma warning restore SYSLIB0011 // Type or member is obsolete
    }

    public static byte[] Serialize<T>(this T data) where T : BasicObj
    {
        MemoryStream mem = new MemoryStream();
        BinaryFormatter formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011 // Type or member is obsolete
        formatter.Serialize(mem, data);
#pragma warning restore SYSLIB0011 // Type or member is obsolete

        var ret = mem.ToArray();
        ret = Compress(ret);
        string b64 = System.Convert.ToBase64String(ret);
        b64 = b64.Substring(prefix.Length);
        ret = Encoding.ASCII.GetBytes(b64);
        return ret;
    }

    public static byte[] Compress(byte[] data)
    {
        byte[] compressArray = data;
        try
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                {
                    deflateStream.Write(data, 0, data.Length);
                }
                compressArray = memoryStream.ToArray();
            }
        }
        catch (Exception exception)
        {
            // do something !
        }
        return compressArray;
    }

    public static byte[] Decompress(byte[] data)
    {
        byte[] decompressedArray = data;
        try
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
                decompressedArray = decompressedStream.ToArray();
            }
        }
        catch (Exception exception)
        {
            // do something !
        }

        return decompressedArray;
    }











    //    public static T Deserialize<T>(this byte[] mem) where T : BasicObj
    //    {
    //        MemoryStream memStream = new MemoryStream();
    //        memStream.Write(mem);
    //        using var decompressor = new GZipStream(memStream, CompressionMode.Decompress);
    //        long len = decompressor.Length;
    //        byte[] data = new byte[len];
    //        int l = decompressor.Read(data, 0, data.Length);
    //        BinaryFormatter formatter = new BinaryFormatter();
    //#pragma warning disable SYSLIB0011 // Type or member is obsolete
    //        return formatter.Deserialize(new MemoryStream(data)) as T;
    //#pragma warning restore SYSLIB0011 // Type or member is obsolete
    //    }

    //    public static byte[] Serialize<T>(this T data) where T : BasicObj
    //    {
    //        MemoryStream mem = new MemoryStream();
    //        BinaryFormatter formatter = new BinaryFormatter();
    //#pragma warning disable SYSLIB0011 // Type or member is obsolete
    //        formatter.Serialize(mem, data);
    //#pragma warning restore SYSLIB0011 // Type or member is obsolete
    //        using var compressor = new GZipStream(mem, CompressionLevel.SmallestSize, leaveOpen: true);
    //        var mem2 = mem.ToArray();
    //        compressor.Write(mem2, 0, mem2.Count());

    //        byte[] dat = new byte[compressor.Length];
    //        compressor.Read(dat, 0, dat.Length);
    //        return dat;
    //    }

}