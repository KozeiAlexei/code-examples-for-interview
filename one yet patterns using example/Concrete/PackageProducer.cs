using System;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Collections.Generic;

using ParallelGZIP.Abstract;
using System.Threading;

namespace ParallelGZIP.Concrete
{
    public class PackageProducer : IPackageProducer, ITask
    {
        private const int GZIP_HEADER_LENGTH = 8;

        private int original_package_size = (2 << 7) * Environment.SystemPageSize;
        private int package_count_threshold = 2 * Environment.ProcessorCount;

        private delegate void ReadFunc();

        private FileStream stream;
        private ReadFunc read_func;
        private Queue<IPackage> package_queue;

        private bool producing_completed;
        private bool producing_completion;

        private long package_count;

        private int gzip_package_size = 0;
        private byte[] tmp_gzip_buffer = new byte[GZIP_HEADER_LENGTH];

        public PackageProducer(string file_name, CompressionMode mode)
        {
            try
            {
                stream = new FileStream(file_name, FileMode.Open, FileAccess.Read);
                package_queue = new Queue<IPackage>(package_count_threshold);

                producing_completion = producing_completed = false;
                if (mode == CompressionMode.Compress)
                    read_func = ReadOriginalPackage;
                else
                    read_func = ReadCompressedPackage;
            }
            catch(FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        public void Start()
        {
            try
            {
                read_func();
                producing_completed = !producing_completion;
            }
            catch(OutOfMemoryException ex)
            {
                Console.WriteLine("Out of memory exception!Program will be closed...");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {              
                stream.Close();
            }
        }

        private void ReadOriginalPackage()
        {
            package_count = stream.Length / original_package_size;
            for(long i = 1; i <= package_count; i++)
            {
                byte[] tmp = new byte[original_package_size];
                stream.Read(tmp, 0, original_package_size);

                while (package_queue.Count >= package_count_threshold) { }
                package_queue.Enqueue(new BytePackage(tmp, i));
            }
            if(stream.Position < stream.Length)
            {
                byte[] tmp = new byte[original_package_size];
                int readed = stream.Read(tmp, 0, original_package_size);

                while (package_queue.Count >= package_count_threshold) { }
                package_queue.Enqueue(new BytePackage(tmp.Take(readed).ToArray(), ++package_count));
            }
        }

        private void ReadCompressedPackage()
        {
            while (stream.Position < stream.Length && !producing_completion)
            {
                while (package_queue.Count >= package_count_threshold) { }
                package_queue.Enqueue(ReadGZIPPackage(++package_count));
            }
        }

        private BytePackage ReadGZIPPackage(long package_number)
        {
            stream.Read(tmp_gzip_buffer, 0, 8);
            gzip_package_size = BitConverter.ToInt32(tmp_gzip_buffer, 4);

            byte[] tmp = new byte[gzip_package_size];
            tmp_gzip_buffer.CopyTo(tmp, 0);
            stream.Read(tmp, 8, gzip_package_size - 8);

            return new BytePackage(tmp, package_number);
        }

        public IPackage GetPackage()
        {
            IPackage package = null;
            lock(package_queue)
            {
                if (package_queue.Count > 0)
                    package = package_queue.Dequeue();
            }
            return package;
        }

        public long PackageCount { get { return package_count; } }

        public void Stop()
        {
            producing_completion = true;
        }

        public bool TaskCompleted
        {
            get
            {
                bool flag = true;
                if (package_queue.Count > 0)
                    flag = false;
                return (flag & producing_completed) | producing_completion;
            }
        }
    }
}
