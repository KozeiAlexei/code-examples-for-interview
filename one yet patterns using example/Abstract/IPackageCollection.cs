namespace ParallelGZIP.Abstract
{
    public interface IPackageCollection
    {
        void AddPackage(IPackage package);

        IPackage GetPackage(long number);
    }
}
