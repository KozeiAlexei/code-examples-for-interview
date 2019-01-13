namespace VoiceRecognizeMark.Model.Algorithms
{
    public class LogarithmPowerParamsModel
    {
        public double[] Signal { get; set; }

        public double [,] MellFilters { get; set; }


        public int MFCCCount { get; set; }
    }
}
