namespace ImpactLab.Core.Persistence.Migrations;

public static class V10MigrationRegistry
{
    public static IEnumerable<ISchemaMigration> Create() =>
        [
            new ExperimentEnvelopeMigration_1_2(),
            new ExperimentEnvelopeMigration_2_3(),
            new WorkspaceEnvelopeMigration_1_2(),
            new WorkspaceEnvelopeMigration_2_3(),
            new ScenarioEnvelopeMigration_2_3(),
        ];
}
