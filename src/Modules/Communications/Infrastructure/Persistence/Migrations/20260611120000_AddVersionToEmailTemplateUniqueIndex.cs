using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionToEmailTemplateUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_email_templates_template_key_language_override_key",
                schema: "communications",
                table: "email_templates");

            migrationBuilder.CreateIndex(
                name: "ix_email_templates_template_key_language_override_key_version",
                schema: "communications",
                table: "email_templates",
                columns: new[] { "template_key", "language", "override_key", "version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_email_templates_template_key_language_override_key_version",
                schema: "communications",
                table: "email_templates");

            migrationBuilder.CreateIndex(
                name: "ix_email_templates_template_key_language_override_key",
                schema: "communications",
                table: "email_templates",
                columns: new[] { "template_key", "language", "override_key" },
                unique: true);
        }
    }
}
