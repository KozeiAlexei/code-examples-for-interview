using System;
using System.Collections.Generic;

using ParallelGZIP.Abstract;

namespace ParallelGZIP.Concrete
{
    public class PackageCollection : IPackageCollection
    {
        private int optimal_capacity = (2 << 2) * Environment.ProcessorCount;

        private Dictionary<long, IPackage> collection;
        
        public PackageCollection()
        {
            collection = new Dictionary<long, IPackage>(optimal_capacity);       
        }

        public void AddPackage(IPackage package)
        {
            lock (collection)
            {
                collection.Add(package.PackageNumber, package);
            }
        }

        public IPackage GetPackage(long number)
        {
            IPackage res = null;
            
            lock(collection)
            {
                if (collection.TryGetValue(number, out res))
                    collection.Remove(number);
            }

            return res;
        }
    }
}
