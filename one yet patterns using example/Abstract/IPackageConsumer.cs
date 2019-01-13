namespace ParallelGZIP.Abstract
{
    public interface IPackageConsumer
    {
        IPackage CompressPackage(IPackage src);
        IPackage DecompressPackage(IPackage src);
    }
}
