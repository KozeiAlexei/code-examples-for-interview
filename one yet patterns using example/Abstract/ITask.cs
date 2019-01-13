namespace ParallelGZIP.Abstract
{
    public interface ITask
    {
        void Start();
        void Stop();

        bool TaskCompleted { get; }
    }
}
