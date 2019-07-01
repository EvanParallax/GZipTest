namespace GZipTest.Utils
{
    public class ZipBlock
    {
        public int Number;
        public byte[] Data;
        public int ActualLength;

        public ZipBlock(byte[] d, int length, int number)
        {
            Number = number;
            Data = d;
            ActualLength = length;
        }
    }
}
