using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderStatusColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add column with valid default value
            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "providers",
                table: "providers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "PendingBasicInfo");

            // Migrate existing data based on verification_status
            migrationBuilder.Sql(@"
                UPDATE providers.providers
                SET status = CASE verification_status
                    WHEN 'Verified' THEN 'Active'
                    WHEN 'Suspended' THEN 'Suspended'
                    WHEN 'Rejected' THEN 'Rejected'
                    WHEN 'InProgress' THEN 'PendingDocumentVerification'
                    ELSE 'PendingBasicInfo'
                END;
            ");

            migrationBuilder.CreateIndex(
                name: "ix_providers_status",
                schema: "providers",
                table: "providers",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_providers_status",
                schema: "providers",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "providers",
                table: "providers");
        }
    }
}
