namespace FieldOps.IntegrationTests;

[CollectionDefinition(
    "Evidence API PostgreSQL integration",
    DisableParallelization = true)]
public sealed class EvidenceApiPostgreSqlCollection
    : ICollectionFixture<
        PostgreSqlDatabaseFixture>
{
}
