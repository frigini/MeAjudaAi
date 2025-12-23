using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneNumberToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "phone_country_code",
                schema: "meajudaai_users",
                table: "users",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true,
                defaultValue: "BR");

            migrationBuilder.AddColumn<string>(
                name: "phone_number",
                schema: "meajudaai_users",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "phone_country_code",
                schema: "meajudaai_users",
                table: "users");

            migrationBuilder.DropColumn(
                name: "phone_number",
                schema: "meajudaai_users",
                table: "users");
        }
    }
}
