namespace FieldOps.IntegrationTests;

[CollectionDefinition(
    "Workflow API PostgreSQL integration",
    DisableParallelization = true)]
public sealed class WorkflowApiPostgreSqlCollection
    : ICollectionFixture<PostgreSqlDatabaseFixture>
{
}
