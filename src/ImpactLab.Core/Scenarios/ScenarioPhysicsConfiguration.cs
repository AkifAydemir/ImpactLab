namespace ImpactLab.Core.Scenarios;

public sealed record ScenarioPhysicsConfiguration
{
    public const string DefaultDamageModelId = "damage.material-response-v1";
    public const string DefaultRigidContactModelId = "contact.rigid-penalty-v1";
    public const string DefaultDeformableContactModelId = "contact.deformable-penalty-v1";
    public const string DefaultLatticeConstitutiveModelId = "constitutive.lattice-legacy-v1";
    public const string DefaultFragmentTopologyModelId = "failure.connected-lattice-v1";

    public string DamageModelId { get; init; } = DefaultDamageModelId;
    public string RigidContactModelId { get; init; } = DefaultRigidContactModelId;
    public string DeformableContactModelId { get; init; } = DefaultDeformableContactModelId;
    public string LatticeConstitutiveModelId { get; init; } = DefaultLatticeConstitutiveModelId;
    public string FragmentTopologyModelId { get; init; } = DefaultFragmentTopologyModelId;
    public List<string> AdditionalModelIds { get; init; } = [];

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(DamageModelId))
            throw new InvalidOperationException("A damage model id is required.");
        if (
            string.IsNullOrWhiteSpace(RigidContactModelId)
            || string.IsNullOrWhiteSpace(DeformableContactModelId)
            || string.IsNullOrWhiteSpace(LatticeConstitutiveModelId)
            || string.IsNullOrWhiteSpace(FragmentTopologyModelId)
        )
            throw new InvalidOperationException(
                "Constitutive, contact and fragment-topology model ids are required."
            );
        if (AdditionalModelIds.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("Additional physics model ids cannot be empty.");
    }
}
