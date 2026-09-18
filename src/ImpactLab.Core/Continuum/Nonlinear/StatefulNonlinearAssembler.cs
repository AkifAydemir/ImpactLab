using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Continuum.Plasticity;
using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Nonlinear;

public sealed class StatefulNonlinearAssembler : INonlinearContinuumAssembler
{
    private readonly J2ReturnMapper _mapper;
    private ElementTrialState[] _trial = [];
    private PlasticState[] _committed = [];

    public StatefulNonlinearAssembler(J2ReturnMapper mapper) => _mapper = mapper;

    public NonlinearAssemblyResult Assemble(
        TetrahedralMesh mesh,
        ReadOnlySpan<double> u,
        IReadOnlyList<PlasticState> committed,
        double dt,
        double time,
        CancellationToken ct = default
    )
    {
        if (_committed.Length != mesh.Elements.Count)
            _committed =
                committed.Count == mesh.Elements.Count
                    ? committed.ToArray()
                    : Enumerable.Repeat(PlasticState.Zero, mesh.Elements.Count).ToArray();
        _trial = new ElementTrialState[mesh.Elements.Count];
        var triplets = new SparseTripletBuilder(mesh.Nodes.Count * 3, mesh.Nodes.Count * 3);
        var internalForce = new double[mesh.Nodes.Count * 3];
        double elasticEnergy = 0;
        double plasticDissipation = 0;
        var plasticElements = 0;
        foreach (var element in mesh.Elements)
        {
            ct.ThrowIfCancellationRequested();
            var kinematics = TetraElementKinematics.FromDisplacement(mesh, element, u);
            var yieldStress = element.Material.YoungModulusPa * element.Material.YieldStrain;
            IHardeningLaw hardening = new LinearIsotropicHardening(
                yieldStress,
                element.Material.TangentModulusPa
            );
            var update = _mapper.Update(
                element.Material,
                kinematics.Strain,
                _committed[element.Id],
                hardening
            );
            var volume = TetraElementGeometry.Volume(mesh, element);
            var plasticJ = update.DissipationDensityJPerM3 * volume;
            _trial[element.Id] = new ElementTrialState(
                update.Stress,
                kinematics.Strain,
                update.State,
                plasticJ,
                293.15
            );
            var tangent = IsotropicElasticity.Matrix(
                element.Material,
                update.ConsistentTangentScale
            );
            ElementTangentScatter.Add(mesh, element, tangent, triplets);
            ElementInternalForceScatter.Add(mesh, element, update.Stress, internalForce);
            elasticEnergy += 0.5 * volume * StressStrainProduct(update.Stress, kinematics.Strain);
            plasticDissipation += plasticJ;
            if (
                update.State.EquivalentPlasticStrain
                > _committed[element.Id].EquivalentPlasticStrain
            )
                plasticElements++;
        }
        return new NonlinearAssemblyResult(
            triplets.BuildCsr(),
            internalForce,
            elasticEnergy,
            plasticDissipation,
            plasticElements
        );
    }

    public IReadOnlyList<PlasticState> Commit()
    {
        _committed = _trial.Select(x => x.Plastic).ToArray();
        return _committed;
    }

    public void Revert() => _trial = [];

    private static double StressStrainProduct(StressTensor6 stress, StrainTensor6 strain) =>
        stress.XX * strain.XX
        + stress.YY * strain.YY
        + stress.ZZ * strain.ZZ
        + stress.XY * strain.XY
        + stress.YZ * strain.YZ
        + stress.ZX * strain.ZX;
}
