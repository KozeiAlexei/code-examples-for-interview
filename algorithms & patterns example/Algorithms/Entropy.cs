using System;
using System.Linq;
using System.Collections.Generic;

namespace VoiceRecognizeMark.Algorithms
{
    public static class EntropyHelper
    {
        public static double GetVolume(double amplitude) => 20.0 * Math.Log(amplitude) / Math.Log(10);

        public static double RMS(double[] signal)
        {
            var result = 0.0;
            foreach (var signalEntry in signal)
                result += signalEntry * signalEntry;

            return Math.Sqrt(result / signal.Length);
        }

        public static double Entropy(double[] signal, int binsCount, double minValue, double maxValue)
        {
            //var entropy = 0.0;

            //var binSize = Math.Abs(maxValue - minValue) / binsCount;

            //var p = new double[binsCount];

            ////Расчет вероятностей
            //var index = default(int);
            //for (int i = 0; i < signal.Length; i++)
            //{
            //    var value = signal[i];

            //    index = (int)Math.Floor((value - minValue) / binSize);
            //    if (index > binsCount)
            //        index = binsCount - 1;

            //    p[index] += 1.0;
            //}

            ////Нормализация
            //var size = signal.Length;
            //for (int i = 0; i < binsCount; i++)
            //    p[i] /= size;

            ////Энтропия
            //foreach (var entry in p)
            //    entropy += entry * Math.Log(entry, 2);

            //entropy = -entropy;

            //return entropy;

            return Entropy(signal);
        }

        private static double Entropy(double[] signal)
        {
            var probabilities = new Dictionary<double, int>(signal.Length);
            foreach (var signalEntry in signal)
            {
                if (probabilities.ContainsKey(signalEntry))
                    probabilities[signalEntry]++;
                else
                    probabilities.Add(signalEntry, 1);
            }

            var probabilitiesNormalized = probabilities.Select(p => (double)p.Value / signal.Length);

            var entropy = 0.0;
            foreach (var probability in probabilitiesNormalized)
                entropy += probability * Math.Log(probability, 2);

            return -entropy;
        }
    }
}
