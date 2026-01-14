using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "search_providers");

<<<<<<<< HEAD:src/Modules/SearchProviders/Infrastructure/Persistence/Migrations/20260113172631_InitialCreate.cs
            // PostGIS extension já existe na imagem postgis/postgis  
            // Removido AlterDatabase().Annotation para evitar erro 57P01
            // migrationBuilder.AlterDatabase()
            //     .Annotation("Npgsql:PostgresExtension:postgis", ",,");
========
            // PostGIS extension is created by SimpleDatabaseFixture in tests
            // and by infrastructure/database/modules/search_providers/01-permissions.sql in production
            // Removed: migrationBuilder.AlterDatabase().Annotation("Npgsql:PostgresExtension:postgis", ",,");
>>>>>>>> 9fc57cdf2e6fd05b534ab067161471b4c19bf763:src/Modules/SearchProviders/Infrastructure/Persistence/Migrations/20251226182933_InitialCreate.cs

            migrationBuilder.CreateTable(
                name: "searchable_providers",
                schema: "search_providers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    location = table.Column<Point>(type: "geography(Point, 4326)", nullable: false),
                    average_rating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    total_reviews = table.Column<int>(type: "integer", nullable: false),
                    subscription_tier = table.Column<int>(type: "integer", nullable: false),
                    service_ids = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_searchable_providers", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_searchable_providers_is_active",
                schema: "search_providers",
                table: "searchable_providers",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_searchable_providers_location",
                schema: "search_providers",
                table: "searchable_providers",
                column: "location")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ix_searchable_providers_provider_id",
                schema: "search_providers",
                table: "searchable_providers",
                column: "provider_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_searchable_providers_search_ranking",
                schema: "search_providers",
                table: "searchable_providers",
                columns: new[] { "is_active", "subscription_tier", "average_rating" });

            migrationBuilder.CreateIndex(
                name: "ix_searchable_providers_service_ids",
                schema: "search_providers",
                table: "searchable_providers",
                column: "service_ids")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_searchable_providers_subscription_tier",
                schema: "search_providers",
                table: "searchable_providers",
                column: "subscription_tier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "searchable_providers",
                schema: "search_providers");
        }
    }
}
