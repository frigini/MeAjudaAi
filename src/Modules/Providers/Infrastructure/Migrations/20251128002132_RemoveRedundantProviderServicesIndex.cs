using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Providers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantProviderServicesIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_provider_services_provider_service",
                schema: "meajudaai_providers",
                table: "provider_services");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_provider_services_provider_service",
                schema: "meajudaai_providers",
                table: "provider_services",
                columns: new[] { "provider_id", "service_id" },
                unique: true);
        }
    }
}
