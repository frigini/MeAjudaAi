using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Locations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "locations");

            migrationBuilder.CreateTable(
                name: "allowed_cities",
                schema: "locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    city_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state_sigla = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    ibge_code = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_allowed_cities", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_allowed_cities_city_name_state_sigla",
                schema: "locations",
                table: "allowed_cities",
                columns: new[] { "city_name", "state_sigla" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_allowed_cities_ibge_code",
                schema: "locations",
                table: "allowed_cities",
                column: "ibge_code",
                unique: true,
                filter: "ibge_code IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_allowed_cities_is_active",
                schema: "locations",
                table: "allowed_cities",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "allowed_cities",
                schema: "locations");
        }
    }
}
