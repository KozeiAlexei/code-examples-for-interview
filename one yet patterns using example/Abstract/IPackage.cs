namespace ParallelGZIP.Abstract
{
    public interface IPackage
    {
        byte[] PackageData { get; set; }
        long PackageNumber { get; }
    }
}
