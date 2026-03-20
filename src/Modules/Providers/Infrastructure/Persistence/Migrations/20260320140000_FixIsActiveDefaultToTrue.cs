using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixIsActiveDefaultToTrue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE providers.providers 
                SET is_active = true 
                WHERE is_active = false;
            ");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "providers",
                table: "providers",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                schema: "providers",
                table: "providers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
