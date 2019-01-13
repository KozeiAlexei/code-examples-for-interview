namespace VoiceRecognizeMark.Abstract.Recognizer
{
    public interface IDigitalTimeWarping
    {
        double CalcDistance(double[] vectorA, double[] vectorB);

        double CalcDistanceVector(double[] vectorA, double[] vectorB, int resultVectorSize);
    }
}
