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
                defaultValue: "");

            // Step 1: Add service_name as nullable
            migrationBuilder.AddColumn<string>(
                name: "service_name",
                schema: "providers",
                table: "provider_services",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            // Step 2: Backfill service_name from service catalog
            migrationBuilder.Sql(@"
                UPDATE providers.provider_services ps
                SET service_name = s.name
                FROM service_catalogs.services s
                WHERE ps.service_id = s.id
                  AND ps.service_name IS NULL;
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
