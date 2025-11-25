using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameSearchSchemaToSearchProviders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if table exists in old 'search' schema and rename it
            // For new databases, InitialCreate will create in 'search_providers' directly
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.tables 
                        WHERE table_schema = 'search' 
                        AND table_name = 'searchable_providers'
                    ) THEN
                        -- Create new schema if it doesn't exist
                        CREATE SCHEMA IF NOT EXISTS search_providers;
                        
                        -- Move table to new schema
                        ALTER TABLE search.searchable_providers SET SCHEMA search_providers;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse: move table back to 'search' schema
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.tables 
                        WHERE table_schema = 'search_providers' 
                        AND table_name = 'searchable_providers'
                    ) THEN
                        -- Create old schema if it doesn't exist
                        CREATE SCHEMA IF NOT EXISTS search;
                        
                        -- Move table back
                        ALTER TABLE search_providers.searchable_providers SET SCHEMA search;
                    END IF;
                END $$;
            ");
        }
    }
}
