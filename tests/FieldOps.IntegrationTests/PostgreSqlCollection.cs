namespace FieldOps.IntegrationTests;

[CollectionDefinition(
    "PostgreSQL integration",
    DisableParallelization = true)]
public sealed class PostgreSqlCollection
    : ICollectionFixture<PostgreSqlDatabaseFixture>
{
}
