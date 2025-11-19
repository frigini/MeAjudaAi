using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameSchemaToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if old PascalCase schema exists and rename it to snake_case
            // This handles databases created with the old schema name
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Check if the old schema exists
                    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'ServiceCatalogs') THEN
                        -- Check if the new schema doesn't exist yet
                        IF NOT EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'service_catalogs') THEN
                            -- Rename the schema
                            ALTER SCHEMA ""ServiceCatalogs"" RENAME TO ""service_catalogs"";
                        ELSE
                            -- Both schemas exist - this is an error state
                            RAISE EXCEPTION 'Both ServiceCatalogs and service_catalogs schemas exist. Manual intervention required.';
                        END IF;
                    END IF;
                    -- If old schema doesn't exist, do nothing (schema was created correctly from the start)
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rename back to PascalCase if rolling back
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'service_catalogs') THEN
                        IF NOT EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'ServiceCatalogs') THEN
                            ALTER SCHEMA ""service_catalogs"" RENAME TO ""ServiceCatalogs"";
                        END IF;
                    END IF;
                END $$;
            ");
        }
    }
}
