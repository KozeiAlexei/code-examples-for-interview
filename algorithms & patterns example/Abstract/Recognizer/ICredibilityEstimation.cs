namespace VoiceRecognizeMark.Abstract.Recognizer
{
    public interface ICredibilityEstimation<TCredibilityMark, TParams>
    {
        TCredibilityMark Estimate(TParams estimationParams);
    }
}
