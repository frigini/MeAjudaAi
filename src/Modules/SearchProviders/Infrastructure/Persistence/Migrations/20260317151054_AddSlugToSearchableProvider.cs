using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugToSearchableProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Adicionar a coluna como NULLABLE
            migrationBuilder.AddColumn<string>(
                name: "slug",
                schema: "search_providers",
                table: "searchable_providers",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            // 2. Executar um backfill UPDATE
            // Cria um slug rudimentar a partir do nome e anexa os primeiros 8 caracteres do ProviderId (no formato "N" / sem hífens)
            migrationBuilder.Sql(@"
                UPDATE search_providers.searchable_providers
                SET slug = LOWER(REPLACE(name, ' ', '-')) || '-' || SUBSTRING(REPLACE(provider_id::text, '-', ''), 1, 8)
                WHERE slug IS NULL OR slug = '';
            ");

            // 3. Alterar a coluna para NOT NULL após o preenchimento dos dados
            migrationBuilder.AlterColumn<string>(
                name: "slug",
                schema: "search_providers",
                table: "searchable_providers",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "slug",
                schema: "search_providers",
                table: "searchable_providers");
        }
    }
}
