using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MeAjudaAi.Modules.Providers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "meajudaai_providers");

            migrationBuilder.CreateTable(
                name: "providers",
                schema: "meajudaai_providers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    legal_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fantasy_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    website = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    complement = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    neighborhood = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    zip_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    country = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    verification_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    suspension_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_providers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document",
                schema: "meajudaai_providers",
                columns: table => new
                {
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    document_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document", x => new { x.provider_id, x.id });
                    table.ForeignKey(
                        name: "FK_document_providers_provider_id",
                        column: x => x.provider_id,
                        principalSchema: "meajudaai_providers",
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "provider_services",
                schema: "meajudaai_providers",
                columns: table => new
                {
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_services", x => new { x.provider_id, x.service_id });
                    table.ForeignKey(
                        name: "FK_provider_services_providers_provider_id",
                        column: x => x.provider_id,
                        principalSchema: "meajudaai_providers",
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "qualification",
                schema: "meajudaai_providers",
                columns: table => new
                {
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    issuing_organization = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    issue_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    document_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_qualification", x => new { x.provider_id, x.id });
                    table.ForeignKey(
                        name: "FK_qualification_providers_provider_id",
                        column: x => x.provider_id,
                        principalSchema: "meajudaai_providers",
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_provider_id_document_type",
                schema: "meajudaai_providers",
                table: "document",
                columns: new[] { "provider_id", "document_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_provider_services_provider_service",
                schema: "meajudaai_providers",
                table: "provider_services",
                columns: new[] { "provider_id", "service_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_provider_services_service_id",
                schema: "meajudaai_providers",
                table: "provider_services",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_providers_is_deleted",
                schema: "meajudaai_providers",
                table: "providers",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_providers_name",
                schema: "meajudaai_providers",
                table: "providers",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_providers_status",
                schema: "meajudaai_providers",
                table: "providers",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_providers_type",
                schema: "meajudaai_providers",
                table: "providers",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_providers_user_id",
                schema: "meajudaai_providers",
                table: "providers",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_providers_verification_status",
                schema: "meajudaai_providers",
                table: "providers",
                column: "verification_status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document",
                schema: "meajudaai_providers");

            migrationBuilder.DropTable(
                name: "provider_services",
                schema: "meajudaai_providers");

            migrationBuilder.DropTable(
                name: "qualification",
                schema: "meajudaai_providers");

            migrationBuilder.DropTable(
                name: "providers",
                schema: "meajudaai_providers");
        }
    }
}
