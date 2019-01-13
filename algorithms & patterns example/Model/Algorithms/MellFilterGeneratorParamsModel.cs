namespace VoiceRecognizeMark.Model.Algorithms
{
    public class MellFilterGeneratorParamsModel
    {
        public int MFCCCount { get; set; }

        public int FilterLength { get; set; }

        public int Frequency { get; set; }
        public int FrequencyMin { get; set; }
        public int FrequencyMax { get; set; }
    }
}
