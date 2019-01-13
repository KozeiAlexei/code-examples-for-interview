namespace VoiceRecognizeMark.Abstract.Recognizer
{
    public interface IPartitioner<TPart, TPartitioningParams>
    {
        TPart[] Partitioning(TPartitioningParams partitioningParams);
    }
}
