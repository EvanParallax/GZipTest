using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GZipTest.Utils.Writers
{
    public class DecompressWriter : IDisposable
    {
        private readonly FileStream destStream;

        private readonly object writeSync;

        private readonly Dictionary<int, (byte[] bytes, int length)> decompressedShards;

        public bool IsProcessingCompleted { get; set; }

        private readonly string path;

        private bool isCanceled;

        public event EventHandler Cancel;

        public DecompressWriter(string path)
        {
            this.path = path;

            destStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

            writeSync = new object();

            decompressedShards = new Dictionary<int, (byte[] bytes, int length)>();

            IsProcessingCompleted = false;

            isCanceled = false;
        }

        public void WriteBytes(ZipBlock block)
        {
            lock (writeSync)
                decompressedShards.Add(block.Number, (block.Data, block.ActualLength));
        }

        public void WriteResult()
        {
            var shardNumber = 1;

            while (!IsProcessingCompleted || !(decompressedShards.Count == 0))
            {
                (byte[] bytes, int length) buf;
                bool result;

                lock (writeSync)
                {
                    result = decompressedShards.TryGetValue(shardNumber, out buf);
                }
                if (result)
                {
                    var byteLength = BitConverter.GetBytes(decompressedShards[shardNumber].bytes.Length);

                    try
                    {
                        destStream.Write(decompressedShards[shardNumber].bytes, 0, decompressedShards[shardNumber].bytes.Length);
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"{Thread.CurrentThread.Name}: Not enough free space to write result");
                        Cancel.Invoke(this, EventArgs.Empty);
                        isCanceled = true;
                        break;
                    }

                    lock (writeSync)
                        decompressedShards.Remove(shardNumber);

                    shardNumber++;
                }
                else
                    Thread.Sleep(50);
            }
        }

        public void Dispose()
        {
            destStream.Dispose();

            if (isCanceled)
                File.Delete(path);
        }
    }
}
