using System;

using VoiceRecognizeMark.Tools;
using VoiceRecognizeMark.Abstract;
using VoiceRecognizeMark.Model.Algorithms;

namespace VoiceRecognizeMark.Algorithms
{
    public class MellFilterGenerator : IAlgorithm<MellFilterGeneratorParamsModel, double[,]>
    {
        public double[,] Execute(MellFilterGeneratorParamsModel algorithmParams)
        {
            var pivots = new double[algorithmParams.MFCCCount + 2];

            pivots[0] = MathUtility.ToMell(algorithmParams.FrequencyMin);
            pivots[algorithmParams.MFCCCount + 1] = MathUtility.ToMell(algorithmParams.FrequencyMax);

            for (int m = 1; m < algorithmParams.MFCCCount + 1; m++)
                pivots[m] = pivots[0] + m * (pivots[algorithmParams.MFCCCount + 1] - pivots[0]) / (algorithmParams.MFCCCount + 1);

            for (int m = 0; m < algorithmParams.MFCCCount + 2; m++)
            {
                pivots[m] = MathUtility.FromMell(pivots[m]);
                pivots[m] = Math.Floor((algorithmParams.FilterLength + 1) * pivots[m] / algorithmParams.Frequency);
            }

            var filterBanks = new double[algorithmParams.MFCCCount, algorithmParams.FilterLength];

            for (int m = 1; m < algorithmParams.MFCCCount + 1; m++)
            {
                for (int k = 0; k < algorithmParams.FilterLength; k++)
                {
                    if (pivots[m - 1] <= k && k <= pivots[m])
                        filterBanks[m - 1, k] = (k - pivots[m - 1]) / (pivots[m] - pivots[m - 1]);
                    else if (pivots[m] < k && k <= pivots[m + 1])
                        filterBanks[m - 1, k] = (pivots[m + 1] - k) / (pivots[m + 1] - pivots[m]);
                    else
                        filterBanks[m - 1, k] = 0;
                }
            }

            return filterBanks;
        }
    }
}
