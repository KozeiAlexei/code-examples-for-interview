using System;
using System.Threading;
using System.IO.Compression;

namespace ParallelGZIP
{
    internal class Program
    {
        internal static ParallelCompression pzip;
        internal static int Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CtrlCHandler);
            try
            {
                if (args.Length < 3)
                    throw new ArgumentException("Invalid count of startup parameters!");

                string mode = args[0];
                string input_fn = args[1];
                string output_fn = args[2];

                CompressionMode compr_mode;
                if (mode == "compress")
                    compr_mode = CompressionMode.Compress;
                else if (mode == "decompress")
                    compr_mode = CompressionMode.Decompress;
                else
                    throw new ArgumentException("Invalid startup parameter: compression mode");

                pzip = new ParallelCompression(input_fn, output_fn, compr_mode);
                Thread compr_thread = new Thread(new ThreadStart(() => pzip.Start()));
                compr_thread.Start();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        internal static void CtrlCHandler(object sender, ConsoleCancelEventArgs args)
        {
            pzip?.Stop();
        }
    }
}
