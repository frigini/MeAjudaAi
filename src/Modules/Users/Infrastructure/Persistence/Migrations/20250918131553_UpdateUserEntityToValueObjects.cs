using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserEntityToValueObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_created_at",
                schema: "users",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_deleted_created",
                schema: "users",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_deleted_email",
                schema: "users",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_deleted_username",
                schema: "users",
                table: "users");
        }
    }
}
