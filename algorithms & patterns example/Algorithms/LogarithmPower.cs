using System;

using VoiceRecognizeMark.Abstract;
using VoiceRecognizeMark.Model.Algorithms;

namespace VoiceRecognizeMark.Algorithms
{
    public class LogarithmPower : IAlgorithm<LogarithmPowerParamsModel, double[]>
    {
        public double[] Execute(LogarithmPowerParamsModel algorithmParams)
        {
            var power = new double[algorithmParams.MFCCCount]; 
            for (int m = 0; m < algorithmParams.MFCCCount; m++)
            {
                power[m] = 0.0;

                for (int k = 0; k < algorithmParams.Signal.Length; k++)
                    power[m] += algorithmParams.MellFilters[m, k] * Math.Pow(algorithmParams.Signal[k], 2);

                power[m] = Math.Log(power[m]);
            }

            return power;
        }
    }
}
