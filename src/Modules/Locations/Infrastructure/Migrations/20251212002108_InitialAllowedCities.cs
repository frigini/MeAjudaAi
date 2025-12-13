using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Locations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialAllowedCities : Migration
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StateSigla = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    IbgeCode = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_allowed_cities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllowedCities_CityName_State",
                schema: "locations",
                table: "allowed_cities",
                columns: new[] { "CityName", "StateSigla" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AllowedCities_IbgeCode",
                schema: "locations",
                table: "allowed_cities",
                column: "IbgeCode",
                unique: true,
                filter: "\"IbgeCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AllowedCities_IsActive",
                schema: "locations",
                table: "allowed_cities",
                column: "IsActive");
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
