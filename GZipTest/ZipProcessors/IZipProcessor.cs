using System;

namespace GZipTest.ZipProcessors
{
    public interface IZipProcessor : IDisposable
    {
        bool Process();
    }
}
