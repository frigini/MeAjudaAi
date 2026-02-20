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
            // Rename Foreign Keys
            migrationBuilder.Sql("ALTER TABLE providers.document RENAME CONSTRAINT \"FK_document_providers_provider_id\" TO fk_document_providers_provider_id;");
            migrationBuilder.Sql("ALTER TABLE providers.provider_services RENAME CONSTRAINT \"FK_provider_services_providers_provider_id\" TO fk_provider_services_providers_provider_id;");
            migrationBuilder.Sql("ALTER TABLE providers.qualification RENAME CONSTRAINT \"FK_qualification_providers_provider_id\" TO fk_qualification_providers_provider_id;");

            // Rename Primary Keys
            migrationBuilder.Sql("ALTER TABLE providers.qualification RENAME CONSTRAINT \"PK_qualification\" TO pk_qualification;");
            migrationBuilder.Sql("ALTER INDEX providers.\"PK_qualification\" RENAME TO pk_qualification;");
            
            migrationBuilder.Sql("ALTER TABLE providers.providers RENAME CONSTRAINT \"PK_providers\" TO pk_providers;");
            migrationBuilder.Sql("ALTER INDEX providers.\"PK_providers\" RENAME TO pk_providers;");
            
            migrationBuilder.Sql("ALTER TABLE providers.provider_services RENAME CONSTRAINT \"PK_provider_services\" TO pk_provider_services;");
            migrationBuilder.Sql("ALTER INDEX providers.\"PK_provider_services\" RENAME TO pk_provider_services;");
            
            migrationBuilder.Sql("ALTER TABLE providers.document RENAME CONSTRAINT \"PK_document\" TO pk_document;");
            migrationBuilder.Sql("ALTER INDEX providers.\"PK_document\" RENAME TO pk_document;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse of Up: rename lowercase back to uppercase

            // Rename Foreign Keys
            migrationBuilder.Sql("ALTER TABLE providers.document RENAME CONSTRAINT fk_document_providers_provider_id TO \"FK_document_providers_provider_id\";");
            migrationBuilder.Sql("ALTER TABLE providers.provider_services RENAME CONSTRAINT fk_provider_services_providers_provider_id TO \"FK_provider_services_providers_provider_id\";");
            migrationBuilder.Sql("ALTER TABLE providers.qualification RENAME CONSTRAINT fk_qualification_providers_provider_id TO \"FK_qualification_providers_provider_id\";");

            // Rename Primary Keys
            migrationBuilder.Sql("ALTER TABLE providers.qualification RENAME CONSTRAINT pk_qualification TO \"PK_qualification\";");
            // The index must be renamed without schema qualification since it's an object acting in the current search_path or schema scope
            migrationBuilder.Sql("ALTER INDEX providers.pk_qualification RENAME TO \"PK_qualification\";");
            
            migrationBuilder.Sql("ALTER TABLE providers.providers RENAME CONSTRAINT pk_providers TO \"PK_providers\";");
            migrationBuilder.Sql("ALTER INDEX providers.pk_providers RENAME TO \"PK_providers\";");
            
            migrationBuilder.Sql("ALTER TABLE providers.provider_services RENAME CONSTRAINT pk_provider_services TO \"PK_provider_services\";");
            migrationBuilder.Sql("ALTER INDEX providers.pk_provider_services RENAME TO \"PK_provider_services\";");
            
            migrationBuilder.Sql("ALTER TABLE providers.document RENAME CONSTRAINT pk_document TO \"PK_document\";");
            migrationBuilder.Sql("ALTER INDEX providers.pk_document RENAME TO \"PK_document\";");
        }
    }
}
