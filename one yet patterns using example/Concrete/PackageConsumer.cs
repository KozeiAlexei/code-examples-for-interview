using System;
using System.IO;
using System.IO.Compression;

using ParallelGZIP.Abstract;

namespace ParallelGZIP.Concrete
{
    public class PackageConsumer : IPackageConsumer, ITask
    {
        private static uint working_consumers = 0;

        private ITask producer_task;
        private IPackageProducer producer;
        private IPackageCollection collection;

        private bool consuming_completed;
        private bool consuming_comletion;

        private delegate IPackage ConsumeFunc(IPackage src);
        private ConsumeFunc proc_func;

        public PackageConsumer(IPackageProducer producer, IPackageCollection collection, CompressionMode mode)
        {
            this.producer = producer;
            this.collection = collection;
            producer_task = (ITask)producer;

            consuming_completed = consuming_comletion = false;

            if (mode == CompressionMode.Compress)
                proc_func = CompressPackage;
            else
                proc_func = DecompressPackage;

            working_consumers++;           
        }

        public void Start()
        {
            try
            {
                while (!producer_task.TaskCompleted && !consuming_comletion)
                {
                    IPackage tmp_package = producer.GetPackage();
                    if (tmp_package != null)
                        collection.AddPackage(proc_func(tmp_package));
                }
                consuming_completed = !consuming_comletion;
                working_consumers--;
            }
            catch (OutOfMemoryException ex)
            {
                Console.WriteLine("Out of memory exception!Program will be closed...");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public IPackage CompressPackage(IPackage src)
        {
            using (MemoryStream ms = new MemoryStream(src.PackageData.Length))
            {
                using (GZipStream gs = new GZipStream(ms, CompressionMode.Compress))
                {
                    gs.Write(src.PackageData, 0, src.PackageData.Length);
                }
                src.PackageData = ms.ToArray();
                BitConverter.GetBytes(src.PackageData.Length)
                            .CopyTo(src.PackageData, 4);
            }
            return src;
        }

        public IPackage DecompressPackage(IPackage src)
        {
            using (MemoryStream in_ms = new MemoryStream(src.PackageData))
            {
                using (GZipStream gz = new GZipStream(in_ms, CompressionMode.Decompress))
                {
                    using (MemoryStream out_ms = new MemoryStream())
                    {
                        gz.CopyTo(out_ms);
                        src.PackageData = out_ms.ToArray();
                    }
                }
            }
            return src;
        }

        public bool TaskCompleted
        {
            get { return consuming_completed; }
        }

        public void Stop()
        {
            consuming_comletion = true;
        }

        public static uint WorkingConsumers
        {
            get { return working_consumers; }
        }
    }
}
