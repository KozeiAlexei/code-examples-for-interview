namespace VoiceRecognizeMark.Abstract.Recognizer
{
    public interface IMFCCBuilder<TParams, TMFCC>
    {
        TMFCC Build(TParams builderParams);
    }
}
