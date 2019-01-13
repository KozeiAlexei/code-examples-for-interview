using System.Runtime.InteropServices;

using ParallelGZIP.Abstract;

namespace ParallelGZIP.Concrete
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BytePackage : IPackage
    {
        private byte[] data;
        private long number;

        public BytePackage(byte[] data, long number)
        {
            this.data = data;
            this.number = number;
        }

        public byte[] PackageData
        {
            get { return data; }
            set { data = value; }
        }

        public long PackageNumber
        {
            get { return number; }
        }
    }
}
