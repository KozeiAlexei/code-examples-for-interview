using System;

using VoiceRecognizeMark.Abstract;

namespace VoiceRecognizeMark.Algorithms
{
    public class HammingWindow : IAlgorithm<double[], double[]>
    {
        public double[] Execute(double[] signal)
        {
            var smoothedFrames = new double[signal.Length];
            for (int i = 0; i < signal.Length; i++)
                smoothedFrames[i] = signal[i] * (0.53386 - 0.46164 * Math.Cos(2 * Math.PI * i / (signal.Length - 1)));

            return smoothedFrames;
        }
    }
}
