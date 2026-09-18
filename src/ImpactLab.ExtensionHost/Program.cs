using ImpactLab.ExtensionHost;

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};
try
{
    return await new ExtensionHostRuntime().RunAsync(Console.In, Console.Out, cts.Token);
}
catch (OperationCanceledException)
{
    return 2;
}
catch (Exception ex)
{
    await Console.Error.WriteLineAsync(ex.ToString());
    return 1;
}
