namespace ImpactLab.Core.Continuum.Meshing.External;

public interface IExternalTetraMesherAdapter
{
    string Id { get; }
    ExternalTetraMesherResult Generate(
        ExternalTetraMesherRequest request,
        CancellationToken ct = default
    );
}
