using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_providers_ProviderId",
                schema: "providers",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Qualification_providers_ProviderId",
                schema: "providers",
                table: "Qualification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Qualification",
                schema: "providers",
                table: "Qualification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Document",
                schema: "providers",
                table: "Document");

            migrationBuilder.RenameTable(
                name: "Qualification",
                schema: "providers",
                newName: "qualification",
                newSchema: "providers");

            migrationBuilder.RenameTable(
                name: "Document",
                schema: "providers",
                newName: "document",
                newSchema: "providers");

            migrationBuilder.RenameIndex(
                name: "IX_Document_ProviderId_document_type",
                schema: "providers",
                table: "document",
                newName: "IX_document_ProviderId_document_type");

            migrationBuilder.AddPrimaryKey(
                name: "PK_qualification",
                schema: "providers",
                table: "qualification",
                columns: new[] { "ProviderId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_document",
                schema: "providers",
                table: "document",
                columns: new[] { "ProviderId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_document_providers_ProviderId",
                schema: "providers",
                table: "document",
                column: "ProviderId",
                principalSchema: "providers",
                principalTable: "providers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_qualification_providers_ProviderId",
                schema: "providers",
                table: "qualification",
                column: "ProviderId",
                principalSchema: "providers",
                principalTable: "providers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_providers_ProviderId",
                schema: "providers",
                table: "document");

            migrationBuilder.DropForeignKey(
                name: "FK_qualification_providers_ProviderId",
                schema: "providers",
                table: "qualification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_qualification",
                schema: "providers",
                table: "qualification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_document",
                schema: "providers",
                table: "document");

            migrationBuilder.RenameTable(
                name: "qualification",
                schema: "providers",
                newName: "Qualification",
                newSchema: "providers");

            migrationBuilder.RenameTable(
                name: "document",
                schema: "providers",
                newName: "Document",
                newSchema: "providers");

            migrationBuilder.RenameIndex(
                name: "IX_document_ProviderId_document_type",
                schema: "providers",
                table: "Document",
                newName: "IX_Document_ProviderId_document_type");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Qualification",
                schema: "providers",
                table: "Qualification",
                columns: new[] { "ProviderId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Document",
                schema: "providers",
                table: "Document",
                columns: new[] { "ProviderId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_Document_providers_ProviderId",
                schema: "providers",
                table: "Document",
                column: "ProviderId",
                principalSchema: "providers",
                principalTable: "providers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Qualification_providers_ProviderId",
                schema: "providers",
                table: "Qualification",
                column: "ProviderId",
                principalSchema: "providers",
                principalTable: "providers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
