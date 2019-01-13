using System;
using System.IO;

using ParallelGZIP.Abstract;

namespace ParallelGZIP.Concrete
{
    public class PackageWriter : ITask
    {
        private FileStream stream;

        private IPackageCollection collection;
        private IPackageProducer producer;

        private long cur_package;

        private bool writing_completed;
        private bool writing_completion;

        public PackageWriter(IPackageCollection collection, string file_name, IPackageProducer producer)
        {
            this.collection = collection;
            this.producer = producer;

            cur_package = 1;
            writing_completed = writing_completion = false;

            stream = new FileStream(file_name, FileMode.Create, FileAccess.Write);
        }

        public void Start()
        {
            try
            {
                while ((PackageConsumer.WorkingConsumers > 0 && !writing_completion) || cur_package <= producer.PackageCount)
                {
                    IPackage package = collection.GetPackage(cur_package);
                    if (package != null)
                    {
                        stream.Write(package.PackageData, 0, package.PackageData.Length);
                        cur_package++;
                    }
                }
                writing_completed = !writing_completion;
            }
            catch (OutOfMemoryException ex)
            {
                Console.WriteLine("Out of memory exception!Program will be closed...");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {           
                stream.Close();
            }
        }

        public void Stop()
        {
            writing_completion = true;
        }

        public bool TaskCompleted
        {
            get { return writing_completed; }
        }
    }
}
