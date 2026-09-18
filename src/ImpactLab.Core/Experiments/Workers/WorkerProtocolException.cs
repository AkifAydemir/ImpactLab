namespace ImpactLab.Core.Experiments.Workers;

public sealed class WorkerProtocolException : IOException
{
    public WorkerProtocolException(string message)
        : base(message) { }
}
