using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Providers.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeProviderConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_providers_provider_id",
                schema: "providers",
                table: "document");

            migrationBuilder.DropForeignKey(
                name: "FK_provider_services_providers_provider_id",
                schema: "providers",
                table: "provider_services");

            migrationBuilder.DropForeignKey(
                name: "FK_qualification_providers_provider_id",
                schema: "providers",
                table: "qualification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_qualification",
                schema: "providers",
                table: "qualification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_providers",
                schema: "providers",
                table: "providers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_provider_services",
                schema: "providers",
                table: "provider_services");

            migrationBuilder.DropPrimaryKey(
                name: "PK_document",
                schema: "providers",
                table: "document");

            migrationBuilder.AddPrimaryKey(
                name: "pk_qualification",
                schema: "providers",
                table: "qualification",
                columns: new[] { "provider_id", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_providers",
                schema: "providers",
                table: "providers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_provider_services",
                schema: "providers",
                table: "provider_services",
                columns: new[] { "provider_id", "service_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_document",
                schema: "providers",
                table: "document",
                columns: new[] { "provider_id", "id" });

            migrationBuilder.AddForeignKey(
                name: "fk_document_providers_provider_id",
                schema: "providers",
                table: "document",
                column: "provider_id",
                principalSchema: "providers",
                principalTable: "providers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_provider_services_providers_provider_id",
                schema: "providers",
                table: "provider_services",
                column: "provider_id",
                principalSchema: "providers",
                principalTable: "providers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_qualification_providers_provider_id",
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
                name: "fk_document_providers_provider_id",
                schema: "providers",
                table: "document");

            migrationBuilder.DropForeignKey(
                name: "fk_provider_services_providers_provider_id",
                schema: "providers",
                table: "provider_services");

            migrationBuilder.DropForeignKey(
                name: "fk_qualification_providers_provider_id",
                schema: "providers",
                table: "qualification");

            migrationBuilder.DropPrimaryKey(
                name: "pk_qualification",
                schema: "providers",
                table: "qualification");

            migrationBuilder.DropPrimaryKey(
                name: "pk_providers",
                schema: "providers",
                table: "providers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_provider_services",
                schema: "providers",
                table: "provider_services");

            migrationBuilder.DropPrimaryKey(
                name: "pk_document",
                schema: "providers",
                table: "document");

            migrationBuilder.AddPrimaryKey(
                name: "PK_qualification",
                schema: "providers",
                table: "qualification",
                columns: new[] { "provider_id", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_providers",
                schema: "providers",
                table: "providers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_provider_services",
                schema: "providers",
                table: "provider_services",
                columns: new[] { "provider_id", "service_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_document",
                schema: "providers",
                table: "document",
                columns: new[] { "provider_id", "id" });

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
                name: "FK_provider_services_providers_provider_id",
                schema: "providers",
                table: "provider_services",
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
    }
}
