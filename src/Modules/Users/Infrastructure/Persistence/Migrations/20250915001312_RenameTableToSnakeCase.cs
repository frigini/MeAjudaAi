using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameTableToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                schema: "users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "users",
                newName: "users",
                newSchema: "users");

            migrationBuilder.RenameColumn(
                name: "Username",
                schema: "users",
                table: "users",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Email",
                schema: "users",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "users",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "users",
                table: "users",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "LastName",
                schema: "users",
                table: "users",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "KeycloakId",
                schema: "users",
                table: "users",
                newName: "keycloak_id");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                schema: "users",
                table: "users",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                schema: "users",
                table: "users",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                schema: "users",
                table: "users",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "users",
                table: "users",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Username",
                schema: "users",
                table: "users",
                newName: "ix_users_username");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                schema: "users",
                table: "users",
                newName: "ix_users_email");

            migrationBuilder.RenameIndex(
                name: "IX_Users_KeycloakId",
                schema: "users",
                table: "users",
                newName: "ix_users_keycloak_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                schema: "users",
                table: "users",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                schema: "users",
                table: "users");

            migrationBuilder.RenameTable(
                name: "users",
                schema: "users",
                newName: "Users",
                newSchema: "users");

            migrationBuilder.RenameColumn(
                name: "username",
                schema: "users",
                table: "Users",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "email",
                schema: "users",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "users",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "users",
                table: "Users",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "last_name",
                schema: "users",
                table: "Users",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "keycloak_id",
                schema: "users",
                table: "Users",
                newName: "KeycloakId");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                schema: "users",
                table: "Users",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "first_name",
                schema: "users",
                table: "Users",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                schema: "users",
                table: "Users",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "users",
                table: "Users",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_users_username",
                schema: "users",
                table: "Users",
                newName: "IX_Users_Username");

            migrationBuilder.RenameIndex(
                name: "ix_users_email",
                schema: "users",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.RenameIndex(
                name: "ix_users_keycloak_id",
                schema: "users",
                table: "Users",
                newName: "IX_Users_KeycloakId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                schema: "users",
                table: "Users",
                column: "Id");
        }
    }
}
