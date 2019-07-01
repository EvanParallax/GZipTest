using GZipTest.Utils;
using GZipTest.Utils.Readers;
using GZipTest.Utils.Writers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace GZipTest.ZipProcessors
{
    class Compressor : IZipProcessor
    {
        private readonly CompressReader reader;

        private readonly CompressWriter writer;

        private bool isCanceled;

        public Compressor(string readPath, string destPath)
        {
            reader = new CompressReader(readPath);

            writer = new CompressWriter(destPath);

            isCanceled = false;

            writer.Cancel += Writer_Cancel;
        }

        private void Writer_Cancel(object sender, EventArgs e)
        {
            isCanceled = true;
        }

        private void Perform()
        {
            while (!isCanceled)
            {
                var result = reader.ReadBytes();

                if (result.bytesToRead <= 0)
                    break;

                var item = ZipUtil.Compress(result.bytes, result.bytesToRead);

                if (!writer.WriteResult(new ZipBlock(item, item.Length, result.shardNumber)))
                    break;
            }
        }

        public bool Process()
        {
            Console.WriteLine("operation started");

            var watch = Stopwatch.StartNew();

            ParallelCompress();

            watch.Stop();

            Console.WriteLine($"Elapsed milliseconds: {watch.ElapsedMilliseconds}");

            Console.WriteLine("operation finished");

            return !isCanceled;
        }


        private void ParallelCompress()
        {
            var workingThreads = new Thread[Environment.ProcessorCount];

            for (int i = 0; i < workingThreads.Count(); i++)
            {
                workingThreads[i] = new Thread(Perform);
                workingThreads[i].Name = $"Working thread {i}";
                workingThreads[i].Start();
            }

            foreach (var item in workingThreads)
                item.Join();

            if (isCanceled)
            {
                Console.WriteLine("operation canceled");

            }
        }

        public void Dispose()
        {
            reader.Dispose();
            writer.Dispose();
        }
    }
}
