using System;


namespace VoiceRecognizeMark.NoiseReduction
{
    public class ThresholdNoiseReduction
    {
        private double threshold = default(double);

        public ThresholdNoiseReduction(double threshold)
        {
            this.threshold = threshold;
        }

        public double[] Process(double[] signal)
        {
            var processed = new double[signal.Length];
            for(int i = 0; i < signal.Length; i++)
            {
                if (Math.Abs(signal[i]) > threshold)
                    processed[i] = signal[i];
            }

            return processed;
        }
    }
}
