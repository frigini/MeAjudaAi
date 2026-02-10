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

            migrationBuilder.AddColumn<string>(
                name: "service_name",
                schema: "providers",
                table: "provider_services",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
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
