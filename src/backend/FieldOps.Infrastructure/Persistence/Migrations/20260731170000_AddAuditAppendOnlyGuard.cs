using FieldOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FieldOps.Infrastructure.Persistence.Migrations;

[DbContext(typeof(FieldOpsDbContext))]
[Migration(
    "20260731170000_AddAuditAppendOnlyGuard")]
public sealed class AddAuditAppendOnlyGuard
    : Migration
{
    protected override void Up(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE OR REPLACE FUNCTION prevent_audit_event_mutation()
            RETURNS trigger
            AS $$
            BEGIN
                RAISE EXCEPTION 'audit_events are append-only';
            END;
            $$
            LANGUAGE plpgsql;

            CREATE TRIGGER audit_events_append_only
            BEFORE UPDATE OR DELETE
            ON audit_events
            FOR EACH ROW
            EXECUTE FUNCTION prevent_audit_event_mutation();
            """);
    }

    protected override void Down(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP TRIGGER IF EXISTS audit_events_append_only
            ON audit_events;

            DROP FUNCTION IF EXISTS prevent_audit_event_mutation();
            """);
    }
}
