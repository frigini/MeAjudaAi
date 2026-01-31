using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Locations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGeoFieldsToAllowedCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "latitude",
                schema: "locations",
                table: "allowed_cities",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                schema: "locations",
                table: "allowed_cities",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "service_radius_km",
                schema: "locations",
                table: "allowed_cities",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "latitude",
                schema: "locations",
                table: "allowed_cities");

            migrationBuilder.DropColumn(
                name: "longitude",
                schema: "locations",
                table: "allowed_cities");

            migrationBuilder.DropColumn(
                name: "service_radius_km",
                schema: "locations",
                table: "allowed_cities");
        }
    }
}
