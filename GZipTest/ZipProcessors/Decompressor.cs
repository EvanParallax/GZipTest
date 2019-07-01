using GZipTest.Utils;
using GZipTest.Utils.Readers;
using GZipTest.Utils.Writers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace GZipTest.ZipProcessors
{
    class Decompressor : IZipProcessor
    {

        private readonly DecompressReader reader;

        private readonly DecompressWriter writer;

        private bool isCanceled;

        public Decompressor(string readPath, string destPath)
        {
            reader = new DecompressReader(readPath);

            writer = new DecompressWriter(destPath);

            writer.Cancel += Writer_Cancel;

            isCanceled = false;
        }

        private void Writer_Cancel(object sender, EventArgs e)
        {
            isCanceled = true;
        }

        public void Perform()
        {
            while (!isCanceled)
            {
                var result = reader.ReadBytes();

                if (result.bytesToRead <= 0)
                    break;

                var item = Utils.ZipUtil.Decompress(result.bytes, result.bytesToRead);
                writer.WriteBytes(new ZipBlock(item, result.bytesToRead, result.shardNumber));
            }
        }

        public bool Process()
        {
            Console.WriteLine("operation started");

            var watch = Stopwatch.StartNew();

            ParallelDecompress();

            watch.Stop();

            Console.WriteLine($"Elapsed milliseconds: {watch.ElapsedMilliseconds}");

            Console.WriteLine("operation finished");

            return !isCanceled;
        }

        private void ParallelDecompress()
        {
            var workingThreads = new Thread[Environment.ProcessorCount];
            for (int i = 0; i < workingThreads.Count(); i++)
            {
                workingThreads[i] = new Thread(Perform);
                workingThreads[i].Name = $"Reading thread {i}";
                workingThreads[i].Start();
            }

            var writingThread = new Thread(writer.WriteResult);
            writingThread.Name = "writing Thread";
            writingThread.Start();

            foreach (var item in workingThreads)
                item.Join();

            writer.IsProcessingCompleted = true;

            writingThread.Join();
        }


        public void Dispose()
        {
            reader.Dispose();
            writer.Dispose();
        }
    }
}
