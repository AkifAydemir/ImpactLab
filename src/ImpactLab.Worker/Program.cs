namespace ImpactLab.Worker;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (
            args.Length == 0
            || !string.Equals(args[0], "worker", StringComparison.OrdinalIgnoreCase)
        )
        {
            Console.Error.WriteLine("usage: ImpactLab.Worker worker");
            return WorkerExitCodes.InvalidRequest;
        }
        try
        {
            using var host = new WorkerHost(Console.In, Console.Out);
            return await host.RunAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return WorkerExitCodes.Unhandled;
        }
    }
}
