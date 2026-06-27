using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCityIdToSearchableProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "city_id",
                schema: "search_providers",
                table: "searchable_providers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_searchable_providers_city",
                schema: "search_providers",
                table: "searchable_providers",
                column: "city");

            migrationBuilder.CreateIndex(
                name: "ix_searchable_providers_city_id",
                schema: "search_providers",
                table: "searchable_providers",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "ix_searchable_providers_state",
                schema: "search_providers",
                table: "searchable_providers",
                column: "state");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_searchable_providers_city",
                schema: "search_providers",
                table: "searchable_providers");

            migrationBuilder.DropIndex(
                name: "ix_searchable_providers_city_id",
                schema: "search_providers",
                table: "searchable_providers");

            migrationBuilder.DropIndex(
                name: "ix_searchable_providers_state",
                schema: "search_providers",
                table: "searchable_providers");

            migrationBuilder.DropColumn(
                name: "city_id",
                schema: "search_providers",
                table: "searchable_providers");
        }
    }
}
