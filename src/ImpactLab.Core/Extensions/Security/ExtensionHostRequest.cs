namespace ImpactLab.Core.Extensions.Security;

public sealed record ExtensionHostRequest(string Operation, string ArgumentsJson = "{}");
