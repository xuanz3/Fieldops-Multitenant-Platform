namespace FieldOps.IntegrationTests;

[CollectionDefinition(
    "Business API PostgreSQL integration",
    DisableParallelization = true)]
public sealed class BusinessApiPostgreSqlCollection
    : ICollectionFixture<PostgreSqlDatabaseFixture>
{
}
