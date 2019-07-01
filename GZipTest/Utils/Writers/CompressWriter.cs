using System;
using System.IO;
using System.Threading;

namespace GZipTest.Utils.Writers
{
    public class CompressWriter : IDisposable
    {
        private readonly FileStream destStream;

        private readonly object writeSync;

        private readonly string path;

        private bool isCanceled;

        public event EventHandler Cancel;

        public CompressWriter(string path)
        {
            this.path = path;

            destStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

            writeSync = new object();

            isCanceled = false;
        }

        public bool WriteResult(ZipBlock block)
        {
            var byteLengthByte = BitConverter.GetBytes(block.ActualLength);

            var shardNumberByte = BitConverter.GetBytes(block.Number);

            lock (writeSync)
            {
                try
                {
                    destStream.Write(byteLengthByte, 0, 4);

                    destStream.Write(shardNumberByte, 0, 4);

                    destStream.Write(block.Data, 0, block.ActualLength);
                }
                catch (IOException ex)
                {
                    isCanceled = true;
                    Console.WriteLine($"{Thread.CurrentThread.Name}: Not enough free space to write result");
                    Cancel.Invoke(this, EventArgs.Empty);
                    return false;
                }
            }
            return true;
        }

        public void Dispose()
        {
            destStream.Dispose();

            if (isCanceled)
                File.Delete(path);
        }
    }
}
