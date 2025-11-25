using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderServicesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "provider_services",
                schema: "providers",
                columns: table => new
                {
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_services", x => new { x.provider_id, x.service_id });
                    table.ForeignKey(
                        name: "FK_provider_services_providers_provider_id",
                        column: x => x.provider_id,
                        principalSchema: "providers",
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_provider_services_provider_service",
                schema: "providers",
                table: "provider_services",
                columns: new[] { "provider_id", "service_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_provider_services_service_id",
                schema: "providers",
                table: "provider_services",
                column: "service_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "provider_services",
                schema: "providers");
        }
    }
}
