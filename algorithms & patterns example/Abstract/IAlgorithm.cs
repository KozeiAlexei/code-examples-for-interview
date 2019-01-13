namespace VoiceRecognizeMark.Abstract
{
    public interface IAlgorithm<TInput, TOutput>
    {
        TOutput Execute(TInput algorithmParams);
    }
}
