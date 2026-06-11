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
                WHERE is_active = true");

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
