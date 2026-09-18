namespace ImpactLab.Core.Scenarios;

public static class ScenarioCloner
{
    public static ScenarioDefinition Clone(ScenarioDefinition source)
    {
        var clone = new ScenarioDefinition
        {
            Id = source.Id,
            Name = source.Name,
            CellSizeMeters = source.CellSizeMeters,
            Settings = source.Settings with { },
            ContactSettings = source.ContactSettings with { },
        };
        foreach (var p in source.Parts)
            clone.Parts.Add(
                new ScenarioPartDefinition
                {
                    Id = p.Id,
                    Name = p.Name,
                    GeometryKind = p.GeometryKind,
                    GeometryParameters = p.GeometryParameters.Clone(),
                    MaterialId = p.MaterialId,
                    Behavior = p.Behavior,
                    FixMinimumZPlane = p.FixMinimumZPlane,
                }
            );
        foreach (var b in source.RigidBodies)
            clone.RigidBodies.Add(
                new ScenarioRigidBodyDefinition
                {
                    Id = b.Id,
                    Name = b.Name,
                    GeometryKind = b.GeometryKind,
                    GeometryParameters = b.GeometryParameters.Clone(),
                    MaterialId = b.MaterialId,
                    InitialVelocity = b.InitialVelocity,
                    InitialAngularVelocity = b.InitialAngularVelocity,
                }
            );
        clone.Contacts.AddRange(source.Contacts);
        return clone;
    }
}
