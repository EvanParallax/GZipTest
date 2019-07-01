using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GZipTest.Utils
{
    class ZipUtil
    {
        public static byte[] Compress(byte[] data, int actualLength)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream dstream = new GZipStream(output, CompressionLevel.Optimal, false))
                {
                    dstream.Write(data, 0, actualLength);
                }
                return output.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data, int actualLength)
        {
            using (MemoryStream input = new MemoryStream(data.Take(actualLength).ToArray()))
            {
                using (MemoryStream output = new MemoryStream())
                {
                    using (GZipStream dstream = new GZipStream(input, CompressionMode.Decompress, false))
                    {
                        dstream.CopyTo(output);
                    }
                    return output.ToArray();
                }
            }
        }
    }
}
