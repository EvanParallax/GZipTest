using System;
using System.IO;

namespace GZipTest.Utils.Readers
{
    public class DecompressReader : IDisposable
    {
        protected readonly FileStream readStream;

        protected readonly object readSync;

        protected int shardNumber;

        public DecompressReader(string path)
        {
            readStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            readSync = new object();

            shardNumber = 0;
        }

        public (int bytesToRead, int shardNumber, byte[] bytes) ReadBytes()
        {
            byte[] shardLengthByte = new byte[4];

            byte[] shardNumberByte = new byte[4];

            byte[] buff;

            int bytesToRead;

            lock (readSync)
            {
                readStream.Read(shardLengthByte, 0, 4);

                readStream.Read(shardNumberByte, 0, 4);

                var shardLength = BitConverter.ToInt32(shardLengthByte, 0);

                buff = new byte[shardLength];

                bytesToRead = readStream.Read(buff, 0, shardLength);
            }
            var shardNumber = BitConverter.ToInt32(shardNumberByte, 0);

            return (bytesToRead, shardNumber, buff);
        }

        public void Dispose()
        {
            readStream.Dispose();
        }
    }
}
