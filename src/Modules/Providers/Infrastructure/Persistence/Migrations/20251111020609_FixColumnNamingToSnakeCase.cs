using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixColumnNamingToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_providers_ProviderId",
                schema: "providers",
                table: "document");

            migrationBuilder.DropForeignKey(
                name: "FK_qualification_providers_ProviderId",
                schema: "providers",
                table: "qualification");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "providers",
                table: "qualification",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ProviderId",
                schema: "providers",
                table: "qualification",
                newName: "provider_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "providers",
                table: "providers",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "providers",
                table: "providers",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "providers",
                table: "document",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ProviderId",
                schema: "providers",
                table: "document",
                newName: "provider_id");

            migrationBuilder.RenameIndex(
                name: "IX_document_ProviderId_document_type",
                schema: "providers",
                table: "document",
                newName: "IX_document_provider_id_document_type");

            migrationBuilder.AddForeignKey(
                name: "FK_document_providers_provider_id",
                schema: "providers",
                table: "document",
                column: "provider_id",
                principalSchema: "providers",
                principalTable: "providers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_qualification_providers_provider_id",
                schema: "providers",
                table: "qualification",
                column: "provider_id",
                principalSchema: "providers",
                principalTable: "providers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_providers_provider_id",
                schema: "providers",
                table: "document");

            migrationBuilder.DropForeignKey(
                name: "FK_qualification_providers_provider_id",
                schema: "providers",
                table: "qualification");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "providers",
                table: "qualification",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "provider_id",
                schema: "providers",
                table: "qualification",
                newName: "ProviderId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "providers",
                table: "providers",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "providers",
                table: "providers",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "providers",
                table: "document",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "provider_id",
                schema: "providers",
                table: "document",
                newName: "ProviderId");

            migrationBuilder.RenameIndex(
                name: "IX_document_provider_id_document_type",
                schema: "providers",
                table: "document",
                newName: "IX_document_ProviderId_document_type");

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
    }
}
