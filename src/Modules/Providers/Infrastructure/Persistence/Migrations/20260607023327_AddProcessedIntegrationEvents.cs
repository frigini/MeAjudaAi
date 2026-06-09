using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedIntegrationEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "processed_integration_events",
                schema: "providers",
                columns: table => new
                {
                    correlation_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processed_integration_events", x => x.correlation_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_providers_deleted_created",
                schema: "providers",
                table: "providers",
                columns: new[] { "is_deleted", "created_at" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_providers_deleted_status_created",
                schema: "providers",
                table: "providers",
                columns: new[] { "is_deleted", "status", "created_at" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_providers_deleted_type_created",
                schema: "providers",
                table: "providers",
                columns: new[] { "is_deleted", "type", "created_at" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_providers_deleted_verification_created",
                schema: "providers",
                table: "providers",
                columns: new[] { "is_deleted", "verification_status", "created_at" },
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processed_integration_events",
                schema: "providers");

            migrationBuilder.DropIndex(
                name: "ix_providers_deleted_created",
                schema: "providers",
                table: "providers");

            migrationBuilder.DropIndex(
                name: "ix_providers_deleted_status_created",
                schema: "providers",
                table: "providers");

            migrationBuilder.DropIndex(
                name: "ix_providers_deleted_type_created",
                schema: "providers",
                table: "providers");

            migrationBuilder.DropIndex(
                name: "ix_providers_deleted_verification_created",
                schema: "providers",
                table: "providers");
        }
    }
}
