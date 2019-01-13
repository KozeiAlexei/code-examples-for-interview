namespace ParallelGZIP.Abstract
{
    public interface IPackageProducer
    {
        IPackage GetPackage();

        long PackageCount { get; }
    }
}
