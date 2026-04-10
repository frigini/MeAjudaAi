using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnableSnakeCaseNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_outbox_messages",
                schema: "communications",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_CorrelationId",
                schema: "communications",
                table: "outbox_messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_email_templates",
                schema: "communications",
                table: "email_templates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_communication_logs",
                schema: "communications",
                table: "communication_logs");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "communications",
                table: "outbox_messages",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Priority",
                schema: "communications",
                table: "outbox_messages",
                newName: "priority");

            migrationBuilder.RenameColumn(
                name: "Payload",
                schema: "communications",
                table: "outbox_messages",
                newName: "payload");

            migrationBuilder.RenameColumn(
                name: "Channel",
                schema: "communications",
                table: "outbox_messages",
                newName: "channel");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "communications",
                table: "outbox_messages",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "communications",
                table: "outbox_messages",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "SentAt",
                schema: "communications",
                table: "outbox_messages",
                newName: "sent_at");

            migrationBuilder.RenameColumn(
                name: "ScheduledAt",
                schema: "communications",
                table: "outbox_messages",
                newName: "scheduled_at");

            migrationBuilder.RenameColumn(
                name: "RetryCount",
                schema: "communications",
                table: "outbox_messages",
                newName: "retry_count");

            migrationBuilder.RenameColumn(
                name: "MaxRetries",
                schema: "communications",
                table: "outbox_messages",
                newName: "max_retries");

            migrationBuilder.RenameColumn(
                name: "ErrorMessage",
                schema: "communications",
                table: "outbox_messages",
                newName: "error_message");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "communications",
                table: "outbox_messages",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CorrelationId",
                schema: "communications",
                table: "outbox_messages",
                newName: "correlation_id");

            migrationBuilder.RenameIndex(
                name: "IX_outbox_messages_Status_ScheduledAt_Priority_CreatedAt",
                schema: "communications",
                table: "outbox_messages",
                newName: "ix_outbox_messages_status_scheduled_at_priority_created_at");

            migrationBuilder.RenameColumn(
                name: "Version",
                schema: "communications",
                table: "email_templates",
                newName: "version");

            migrationBuilder.RenameColumn(
                name: "Subject",
                schema: "communications",
                table: "email_templates",
                newName: "subject");

            migrationBuilder.RenameColumn(
                name: "Language",
                schema: "communications",
                table: "email_templates",
                newName: "language");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "communications",
                table: "email_templates",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "communications",
                table: "email_templates",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TextBody",
                schema: "communications",
                table: "email_templates",
                newName: "text_body");

            migrationBuilder.RenameColumn(
                name: "TemplateKey",
                schema: "communications",
                table: "email_templates",
                newName: "template_key");

            migrationBuilder.RenameColumn(
                name: "OverrideKey",
                schema: "communications",
                table: "email_templates",
                newName: "override_key");

            migrationBuilder.RenameColumn(
                name: "IsSystemTemplate",
                schema: "communications",
                table: "email_templates",
                newName: "is_system_template");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                schema: "communications",
                table: "email_templates",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "HtmlBody",
                schema: "communications",
                table: "email_templates",
                newName: "html_body");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "communications",
                table: "email_templates",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_email_templates_TemplateKey_Language_OverrideKey",
                schema: "communications",
                table: "email_templates",
                newName: "ix_email_templates_template_key_language_override_key");

            migrationBuilder.RenameColumn(
                name: "Recipient",
                schema: "communications",
                table: "communication_logs",
                newName: "recipient");

            migrationBuilder.RenameColumn(
                name: "Channel",
                schema: "communications",
                table: "communication_logs",
                newName: "channel");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "communications",
                table: "communication_logs",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "communications",
                table: "communication_logs",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TemplateKey",
                schema: "communications",
                table: "communication_logs",
                newName: "template_key");

            migrationBuilder.RenameColumn(
                name: "OutboxMessageId",
                schema: "communications",
                table: "communication_logs",
                newName: "outbox_message_id");

            migrationBuilder.RenameColumn(
                name: "IsSuccess",
                schema: "communications",
                table: "communication_logs",
                newName: "is_success");

            migrationBuilder.RenameColumn(
                name: "ErrorMessage",
                schema: "communications",
                table: "communication_logs",
                newName: "error_message");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "communications",
                table: "communication_logs",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CorrelationId",
                schema: "communications",
                table: "communication_logs",
                newName: "correlation_id");

            migrationBuilder.RenameColumn(
                name: "AttemptCount",
                schema: "communications",
                table: "communication_logs",
                newName: "attempt_count");

            migrationBuilder.RenameIndex(
                name: "IX_communication_logs_Recipient",
                schema: "communications",
                table: "communication_logs",
                newName: "ix_communication_logs_recipient");

            migrationBuilder.RenameIndex(
                name: "IX_communication_logs_CreatedAt",
                schema: "communications",
                table: "communication_logs",
                newName: "ix_communication_logs_created_at");

            migrationBuilder.RenameIndex(
                name: "IX_communication_logs_CorrelationId",
                schema: "communications",
                table: "communication_logs",
                newName: "ix_communication_logs_correlation_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_outbox_messages",
                schema: "communications",
                table: "outbox_messages",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_email_templates",
                schema: "communications",
                table: "email_templates",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_communication_logs",
                schema: "communications",
                table: "communication_logs",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_correlation_id",
                schema: "communications",
                table: "outbox_messages",
                column: "correlation_id",
                unique: true,
                filter: "\"correlation_id\" IS NOT Null");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_outbox_messages",
                schema: "communications",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "ix_outbox_messages_correlation_id",
                schema: "communications",
                table: "outbox_messages");

            migrationBuilder.DropPrimaryKey(
                name: "pk_email_templates",
                schema: "communications",
                table: "email_templates");

            migrationBuilder.DropPrimaryKey(
                name: "pk_communication_logs",
                schema: "communications",
                table: "communication_logs");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "communications",
                table: "outbox_messages",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "priority",
                schema: "communications",
                table: "outbox_messages",
                newName: "Priority");

            migrationBuilder.RenameColumn(
                name: "payload",
                schema: "communications",
                table: "outbox_messages",
                newName: "Payload");

            migrationBuilder.RenameColumn(
                name: "channel",
                schema: "communications",
                table: "outbox_messages",
                newName: "Channel");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "communications",
                table: "outbox_messages",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "communications",
                table: "outbox_messages",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "sent_at",
                schema: "communications",
                table: "outbox_messages",
                newName: "SentAt");

            migrationBuilder.RenameColumn(
                name: "scheduled_at",
                schema: "communications",
                table: "outbox_messages",
                newName: "ScheduledAt");

            migrationBuilder.RenameColumn(
                name: "retry_count",
                schema: "communications",
                table: "outbox_messages",
                newName: "RetryCount");

            migrationBuilder.RenameColumn(
                name: "max_retries",
                schema: "communications",
                table: "outbox_messages",
                newName: "MaxRetries");

            migrationBuilder.RenameColumn(
                name: "error_message",
                schema: "communications",
                table: "outbox_messages",
                newName: "ErrorMessage");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "communications",
                table: "outbox_messages",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "correlation_id",
                schema: "communications",
                table: "outbox_messages",
                newName: "CorrelationId");

            migrationBuilder.RenameIndex(
                name: "ix_outbox_messages_status_scheduled_at_priority_created_at",
                schema: "communications",
                table: "outbox_messages",
                newName: "IX_outbox_messages_Status_ScheduledAt_Priority_CreatedAt");

            migrationBuilder.RenameColumn(
                name: "version",
                schema: "communications",
                table: "email_templates",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "subject",
                schema: "communications",
                table: "email_templates",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "language",
                schema: "communications",
                table: "email_templates",
                newName: "Language");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "communications",
                table: "email_templates",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "communications",
                table: "email_templates",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "text_body",
                schema: "communications",
                table: "email_templates",
                newName: "TextBody");

            migrationBuilder.RenameColumn(
                name: "template_key",
                schema: "communications",
                table: "email_templates",
                newName: "TemplateKey");

            migrationBuilder.RenameColumn(
                name: "override_key",
                schema: "communications",
                table: "email_templates",
                newName: "OverrideKey");

            migrationBuilder.RenameColumn(
                name: "is_system_template",
                schema: "communications",
                table: "email_templates",
                newName: "IsSystemTemplate");

            migrationBuilder.RenameColumn(
                name: "is_active",
                schema: "communications",
                table: "email_templates",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "html_body",
                schema: "communications",
                table: "email_templates",
                newName: "HtmlBody");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "communications",
                table: "email_templates",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_email_templates_template_key_language_override_key",
                schema: "communications",
                table: "email_templates",
                newName: "IX_email_templates_TemplateKey_Language_OverrideKey");

            migrationBuilder.RenameColumn(
                name: "recipient",
                schema: "communications",
                table: "communication_logs",
                newName: "Recipient");

            migrationBuilder.RenameColumn(
                name: "channel",
                schema: "communications",
                table: "communication_logs",
                newName: "Channel");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "communications",
                table: "communication_logs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "communications",
                table: "communication_logs",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "template_key",
                schema: "communications",
                table: "communication_logs",
                newName: "TemplateKey");

            migrationBuilder.RenameColumn(
                name: "outbox_message_id",
                schema: "communications",
                table: "communication_logs",
                newName: "OutboxMessageId");

            migrationBuilder.RenameColumn(
                name: "is_success",
                schema: "communications",
                table: "communication_logs",
                newName: "IsSuccess");

            migrationBuilder.RenameColumn(
                name: "error_message",
                schema: "communications",
                table: "communication_logs",
                newName: "ErrorMessage");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "communications",
                table: "communication_logs",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "correlation_id",
                schema: "communications",
                table: "communication_logs",
                newName: "CorrelationId");

            migrationBuilder.RenameColumn(
                name: "attempt_count",
                schema: "communications",
                table: "communication_logs",
                newName: "AttemptCount");

            migrationBuilder.RenameIndex(
                name: "ix_communication_logs_recipient",
                schema: "communications",
                table: "communication_logs",
                newName: "IX_communication_logs_Recipient");

            migrationBuilder.RenameIndex(
                name: "ix_communication_logs_created_at",
                schema: "communications",
                table: "communication_logs",
                newName: "IX_communication_logs_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_communication_logs_correlation_id",
                schema: "communications",
                table: "communication_logs",
                newName: "IX_communication_logs_CorrelationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_outbox_messages",
                schema: "communications",
                table: "outbox_messages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_email_templates",
                schema: "communications",
                table: "email_templates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_communication_logs",
                schema: "communications",
                table: "communication_logs",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_CorrelationId",
                schema: "communications",
                table: "outbox_messages",
                column: "CorrelationId",
                unique: true,
                filter: "\"CorrelationId\" IS NOT Null");
        }
    }
}
