using System;
using System.IO;

namespace GZipTest.Utils.Readers
{
    public class CompressReader : IDisposable
    {
        private readonly FileStream readStream;

        private readonly object readSync;

        private int shardNumber;

        public int BUFFER_SIZE { get; private set; }

        public CompressReader(string path)
        {
            readStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            readSync = new object();

            shardNumber = 0;

            BUFFER_SIZE = 4 * 1024 * 1024;
        }

        public (int bytesToRead, int shardNumber, byte[] bytes) ReadBytes()
        {
            var buff = new byte[BUFFER_SIZE];

            lock (readSync)
            {
                shardNumber++;

                return (readStream.Read(buff, 0, BUFFER_SIZE), shardNumber, buff);
            }
        }

        public void Dispose()
        {
            readStream.Dispose();
        }
    }
}
