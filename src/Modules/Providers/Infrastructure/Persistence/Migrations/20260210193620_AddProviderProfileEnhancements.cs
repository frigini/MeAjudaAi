using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderProfileEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "additional_phone_numbers",
                schema: "providers",
                table: "providers",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            // Step 1: Add service_name as nullable
            migrationBuilder.AddColumn<string>(
                name: "service_name",
                schema: "providers",
                table: "provider_services",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            // Step 2: Initialize service_name with a default value
            // Cross-module name backfill removed to maintain schema isolation (Issue #231)
            migrationBuilder.Sql(@"
                UPDATE providers.provider_services
                SET service_name = 'Provider Service'
                WHERE service_name IS NULL;
            ");

            // Step 3: Make service_name non-nullable (all rows should have values now)
            migrationBuilder.AlterColumn<string>(
                name: "service_name",
                schema: "providers",
                table: "provider_services",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "additional_phone_numbers",
                schema: "providers",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "service_name",
                schema: "providers",
                table: "provider_services");
        }
    }
}
