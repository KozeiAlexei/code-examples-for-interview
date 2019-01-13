using System;
using System.Threading;
using System.IO.Compression;

using ParallelGZIP.Abstract;
using ParallelGZIP.Concrete;

namespace ParallelGZIP
{
    public class ParallelCompression : ITask
    {
        private PackageProducer producer;
        private PackageConsumer[] consumer;

        private PackageCollection collection;
        private PackageWriter writer;

        private Thread producer_thread;
        private Thread writer_thread;
        private Thread[] consumer_threads;

        public ParallelCompression(string input_filename, string output_filename, CompressionMode mode)
        {
            collection = new PackageCollection();

            producer = new PackageProducer(input_filename, mode);
            producer_thread = new Thread(new ThreadStart(() =>
            {
                producer.Start();
            }));

            consumer = new PackageConsumer[Environment.ProcessorCount >> 1];
            consumer_threads = new Thread[Environment.ProcessorCount >> 1];

            for (int i = 0; i < consumer.Length; i++)
                consumer[i] = new PackageConsumer(producer, collection, mode);

            for (int i = 0; i < consumer_threads.Length; i++)
            {
                int __f = i;
                consumer_threads[i] = new Thread(new ThreadStart(() => consumer[__f].Start()));
            }

            writer = new PackageWriter(collection, output_filename, producer);
            writer_thread = new Thread(new ThreadStart(() =>
            {
                writer.Start();
            }));
        }

        public void Start()
        {
            producer_thread.Start();
            for (int i = 0; i < consumer_threads.Length; i++)
                consumer_threads[i].Start();
            writer_thread.Start();
        }

        public void Stop()
        {
            producer.Stop();

            for (int i = 0; i < consumer.Length; i++)
                consumer[i].Stop();

            writer.Stop();
        }

        public bool TaskCompleted
        {
            get
            {
                bool res = true;

                res &= producer.TaskCompleted;
                for (int i = 0; i < consumer.Length; i++)
                    res &= consumer[i].TaskCompleted;

                res &= writer.TaskCompleted;

                return res;
            }
        }
    }
}
