using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    phone_country_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true, defaultValue: "BR"),
                    keycloak_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_username_change_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_created_at",
                schema: "users",
                table: "users",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_users_deleted_created",
                schema: "users",
                table: "users",
                columns: new[] { "is_deleted", "created_at" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_users_deleted_email",
                schema: "users",
                table: "users",
                columns: new[] { "is_deleted", "email" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_users_deleted_username",
                schema: "users",
                table: "users",
                columns: new[] { "is_deleted", "username" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "users",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_keycloak_id",
                schema: "users",
                table: "users",
                column: "keycloak_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                schema: "users",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users",
                schema: "users");
        }
    }
}
