using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Catalogs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalogs");

            migrationBuilder.CreateTable(
                name: "service_categories",
                schema: "catalogs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "services",
                schema: "catalogs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_services", x => x.id);
                    table.ForeignKey(
                        name: "fk_services_category",
                        column: x => x.category_id,
                        principalSchema: "catalogs",
                        principalTable: "service_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_service_categories_display_order",
                schema: "catalogs",
                table: "service_categories",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "ix_service_categories_is_active",
                schema: "catalogs",
                table: "service_categories",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_service_categories_name",
                schema: "catalogs",
                table: "service_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_services_category_display_order",
                schema: "catalogs",
                table: "services",
                columns: new[] { "category_id", "display_order" });

            migrationBuilder.CreateIndex(
                name: "ix_services_category_id",
                schema: "catalogs",
                table: "services",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_services_is_active",
                schema: "catalogs",
                table: "services",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_services_name",
                schema: "catalogs",
                table: "services",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "services",
                schema: "catalogs");

            migrationBuilder.DropTable(
                name: "service_categories",
                schema: "catalogs");
        }
    }
}
