using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnsureDocumentIsPrimaryColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add is_primary column if it doesn't exist
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_schema = 'providers' 
                        AND table_name = 'document' 
                        AND column_name = 'is_primary'
                    ) THEN
                        ALTER TABLE providers.document 
                        ADD COLUMN is_primary boolean NOT NULL DEFAULT false;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove is_primary column if it exists
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_schema = 'providers' 
                        AND table_name = 'document' 
                        AND column_name = 'is_primary'
                    ) THEN
                        ALTER TABLE providers.document 
                        DROP COLUMN is_primary;
                    END IF;
                END $$;
            ");
        }
    }
}
