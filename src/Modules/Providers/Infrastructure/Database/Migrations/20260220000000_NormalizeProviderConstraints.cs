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
            // Renomear chaves estrangeiras
            migrationBuilder.Sql("ALTER TABLE providers.document RENAME CONSTRAINT \"FK_document_providers_provider_id\" TO fk_document_providers_provider_id;");
            migrationBuilder.Sql("ALTER TABLE providers.provider_services RENAME CONSTRAINT \"FK_provider_services_providers_provider_id\" TO fk_provider_services_providers_provider_id;");
            migrationBuilder.Sql("ALTER TABLE providers.qualification RENAME CONSTRAINT \"FK_qualification_providers_provider_id\" TO fk_qualification_providers_provider_id;");

            // Renomear chaves primárias
            migrationBuilder.Sql("ALTER TABLE providers.qualification RENAME CONSTRAINT \"PK_qualification\" TO pk_qualification;");
            
            migrationBuilder.Sql("ALTER TABLE providers.providers RENAME CONSTRAINT \"PK_providers\" TO pk_providers;");
            
            migrationBuilder.Sql("ALTER TABLE providers.provider_services RENAME CONSTRAINT \"PK_provider_services\" TO pk_provider_services;");
            
            migrationBuilder.Sql("ALTER TABLE providers.document RENAME CONSTRAINT \"PK_document\" TO pk_document;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reversão de Up: renomear de minúsculas para maiúsculas

            // Renomear chaves estrangeiras
            migrationBuilder.Sql("ALTER TABLE providers.document RENAME CONSTRAINT fk_document_providers_provider_id TO \"FK_document_providers_provider_id\";");
            migrationBuilder.Sql("ALTER TABLE providers.provider_services RENAME CONSTRAINT fk_provider_services_providers_provider_id TO \"FK_provider_services_providers_provider_id\";");
            migrationBuilder.Sql("ALTER TABLE providers.qualification RENAME CONSTRAINT fk_qualification_providers_provider_id TO \"FK_qualification_providers_provider_id\";");

            // Renomear chaves primárias
            migrationBuilder.Sql("ALTER TABLE providers.qualification RENAME CONSTRAINT pk_qualification TO \"PK_qualification\";");
            
            migrationBuilder.Sql("ALTER TABLE providers.providers RENAME CONSTRAINT pk_providers TO \"PK_providers\";");
            
            migrationBuilder.Sql("ALTER TABLE providers.provider_services RENAME CONSTRAINT pk_provider_services TO \"PK_provider_services\";");
            
            migrationBuilder.Sql("ALTER TABLE providers.document RENAME CONSTRAINT pk_document TO \"PK_document\";");
        }
    }
}
