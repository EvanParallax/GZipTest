using GZipTest.ZipProcessors;
using System;
using System.Collections.Generic;

namespace GZipTest
{
    class Program
    {
        private static readonly Dictionary<string, Func<string, string, int>> actions = new Dictionary<string, Func<string, string, int>>
        {
            { "compress", Compress},
            { "decompress", Decompress }
        };

        private static int Compress(string compressFile, string compressedFile)
        {
            try
            {
                using (IZipProcessor c = new Compressor(compressFile, compressedFile))
                    if (c.Process())
                        return 0;
                    else
                        return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        private static int Decompress(string compressFile, string compressedFile)
        {
            try
            {
                using (IZipProcessor d = new Decompressor(compressFile, compressedFile))
                    if (d.Process())
                        return 0;
                    else
                        return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        static int Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Wrong arguments count");
                Console.Read();
                return 1;
            }

            Func<string, string, int> work = null;

            if (actions.TryGetValue(args[0], out work))
                return work.Invoke(args[1], args[2]);
            else
            {
                Console.WriteLine("wrong command");
                return 1;
            }
        }
    }
}
