using VoiceRecognizeMark.Abstract;

namespace VoiceRecognizeMark.Algorithms
{
    public class PreEmphasis : IAlgorithm<double[], double[]>
    {
        public double[] Execute(double[] signal)
        {
            var processed = new double[signal.Length];

            processed[0] = signal[0];
            for (int i = 1; i < processed.Length; i++)
                processed[i] = signal[i] - 0.95 * signal[i - 1];

            return processed;
        }
    }
}
