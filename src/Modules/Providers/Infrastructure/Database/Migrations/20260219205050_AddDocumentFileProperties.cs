using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Providers.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentFileProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_document_provider_id_document_type",
                schema: "providers",
                table: "document",
                newName: "ix_document_provider_id_document_type");

            migrationBuilder.AddColumn<string>(
                name: "file_name",
                schema: "providers",
                table: "document",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "file_url",
                schema: "providers",
                table: "document",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "file_name",
                schema: "providers",
                table: "document");

            migrationBuilder.DropColumn(
                name: "file_url",
                schema: "providers",
                table: "document");

            migrationBuilder.RenameIndex(
                name: "ix_document_provider_id_document_type",
                schema: "providers",
                table: "document",
                newName: "IX_document_provider_id_document_type");
        }
    }
}
