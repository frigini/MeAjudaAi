using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCorrelationIdToOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_ScheduledAt",
                schema: "communications",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_Status_Priority_CreatedAt",
                schema: "communications",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "IX_email_templates_TemplateKey_Language_OverrideKey",
                schema: "communications",
                table: "email_templates");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "communications",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                schema: "communications",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ScheduledAt",
                schema: "communications",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "communications",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                schema: "communications",
                table: "outbox_messages",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "communications",
                table: "email_templates",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "communications",
                table: "email_templates",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "communications",
                table: "communication_logs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "communications",
                table: "communication_logs",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_CorrelationId",
                schema: "communications",
                table: "outbox_messages",
                column: "CorrelationId",
                unique: true,
                filter: "\"CorrelationId\" IS NOT Null");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_Status_ScheduledAt_Priority_CreatedAt",
                schema: "communications",
                table: "outbox_messages",
                columns: new[] { "Status", "ScheduledAt", "Priority", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_email_templates_TemplateKey_Language_OverrideKey",
                schema: "communications",
                table: "email_templates",
                columns: new[] { "TemplateKey", "Language", "OverrideKey" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_CorrelationId",
                schema: "communications",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_Status_ScheduledAt_Priority_CreatedAt",
                schema: "communications",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "IX_email_templates_TemplateKey_Language_OverrideKey",
                schema: "communications",
                table: "email_templates");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                schema: "communications",
                table: "outbox_messages");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "communications",
                table: "outbox_messages",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                schema: "communications",
                table: "outbox_messages",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ScheduledAt",
                schema: "communications",
                table: "outbox_messages",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "communications",
                table: "outbox_messages",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "communications",
                table: "email_templates",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "communications",
                table: "email_templates",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "communications",
                table: "communication_logs",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "communications",
                table: "communication_logs",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_ScheduledAt",
                schema: "communications",
                table: "outbox_messages",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_Status_Priority_CreatedAt",
                schema: "communications",
                table: "outbox_messages",
                columns: new[] { "Status", "Priority", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_email_templates_TemplateKey_Language_OverrideKey",
                schema: "communications",
                table: "email_templates",
                columns: new[] { "TemplateKey", "Language", "OverrideKey" },
                unique: true);
        }
    }
}
