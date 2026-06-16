using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPartialIndexForActiveEmailTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_email_templates_template_key_language_override_key_version",
                schema: "communications",
                table: "email_templates");

            // Índice único parcial: apenas uma versão ativa por template_key + language + override_key
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX ix_email_templates_active_per_key_language_override
                ON communications.email_templates (template_key, language, override_key)
                WHERE is_active = true AND override_key IS NOT NULL");

            // Índice único parcial para override_key NULL: apenas uma versão ativa por template_key + language
            // Necessário porque PostgreSQL trata NULLs como distintos em índices únicos
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX ix_email_templates_active_per_key_language_no_override
                ON communications.email_templates (template_key, language)
                WHERE is_active = true AND override_key IS NULL");

            // Índice para buscar todas as versões de um template específico
            migrationBuilder.CreateIndex(
                name: "ix_email_templates_template_key_language_version",
                schema: "communications",
                table: "email_templates",
                columns: new[] { "template_key", "language", "version" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_email_templates_active_per_key_language_override",
                schema: "communications",
                table: "email_templates");

            migrationBuilder.Sql("DROP INDEX IF EXISTS communications.ix_email_templates_active_per_key_language_no_override");

            migrationBuilder.DropIndex(
                name: "ix_email_templates_template_key_language_version",
                schema: "communications",
                table: "email_templates");

            migrationBuilder.CreateIndex(
                name: "ix_email_templates_template_key_language_override_key_version",
                schema: "communications",
                table: "email_templates",
                columns: new[] { "template_key", "language", "override_key", "version" },
                unique: true);
        }
    }
}
