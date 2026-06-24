using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"providers\".\"document\" ALTER COLUMN \"id\" DROP IDENTITY;");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                schema: "providers",
                table: "document",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "providers",
                table: "document",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.Sql("ALTER TABLE \"providers\".\"document\" ALTER COLUMN \"id\" ADD GENERATED ALWAYS AS IDENTITY;");
        }
    }
}
