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
            migrationBuilder.EnsureSchema(
                name: "search_providers");

            migrationBuilder.RenameTable(
                name: "searchable_providers",
                schema: "search",
                newName: "searchable_providers",
                newSchema: "search_providers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "search");

            migrationBuilder.RenameTable(
                name: "searchable_providers",
                schema: "search_providers",
                newName: "searchable_providers",
                newSchema: "search");
        }
    }
}
