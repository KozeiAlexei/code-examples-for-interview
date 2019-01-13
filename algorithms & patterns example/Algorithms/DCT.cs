using System;

using VoiceRecognizeMark.Abstract;

namespace VoiceRecognizeMark.Algorithms
{
    public class DCT : IAlgorithm<double[], double[]>
    {
        public double[] Execute(double[] source)
        {
            var dctResult = new double[source.Length];

            for (int n = 0; n < source.Length; n++)
            {
                dctResult[n] = 0;
                for (int m = 0; m < source.Length; m++)
                    dctResult[n] += source[m] * Math.Cos(Math.PI * n * (m + 0.5) / source.Length);
            }

            return dctResult;
        }
    }
}
