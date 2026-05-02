using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Documents.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxToDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OutboxMessages",
                schema: "documents",
                table: "OutboxMessages");

            migrationBuilder.RenameTable(
                name: "OutboxMessages",
                schema: "documents",
                newName: "outbox_messages",
                newSchema: "documents");

            migrationBuilder.RenameColumn(
                name: "Type",
                schema: "documents",
                table: "outbox_messages",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "documents",
                table: "outbox_messages",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Priority",
                schema: "documents",
                table: "outbox_messages",
                newName: "priority");

            migrationBuilder.RenameColumn(
                name: "Payload",
                schema: "documents",
                table: "outbox_messages",
                newName: "payload");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "documents",
                table: "outbox_messages",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "documents",
                table: "outbox_messages",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "SentAt",
                schema: "documents",
                table: "outbox_messages",
                newName: "sent_at");

            migrationBuilder.RenameColumn(
                name: "ScheduledAt",
                schema: "documents",
                table: "outbox_messages",
                newName: "scheduled_at");

            migrationBuilder.RenameColumn(
                name: "RetryCount",
                schema: "documents",
                table: "outbox_messages",
                newName: "retry_count");

            migrationBuilder.RenameColumn(
                name: "MaxRetries",
                schema: "documents",
                table: "outbox_messages",
                newName: "max_retries");

            migrationBuilder.RenameColumn(
                name: "ErrorMessage",
                schema: "documents",
                table: "outbox_messages",
                newName: "error_message");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "documents",
                table: "outbox_messages",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CorrelationId",
                schema: "documents",
                table: "outbox_messages",
                newName: "correlation_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_outbox_messages",
                schema: "documents",
                table: "outbox_messages",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_outbox_messages",
                schema: "documents",
                table: "outbox_messages");

            migrationBuilder.RenameTable(
                name: "outbox_messages",
                schema: "documents",
                newName: "OutboxMessages",
                newSchema: "documents");

            migrationBuilder.RenameColumn(
                name: "type",
                schema: "documents",
                table: "OutboxMessages",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "documents",
                table: "OutboxMessages",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "priority",
                schema: "documents",
                table: "OutboxMessages",
                newName: "Priority");

            migrationBuilder.RenameColumn(
                name: "payload",
                schema: "documents",
                table: "OutboxMessages",
                newName: "Payload");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "documents",
                table: "OutboxMessages",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "documents",
                table: "OutboxMessages",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "sent_at",
                schema: "documents",
                table: "OutboxMessages",
                newName: "SentAt");

            migrationBuilder.RenameColumn(
                name: "scheduled_at",
                schema: "documents",
                table: "OutboxMessages",
                newName: "ScheduledAt");

            migrationBuilder.RenameColumn(
                name: "retry_count",
                schema: "documents",
                table: "OutboxMessages",
                newName: "RetryCount");

            migrationBuilder.RenameColumn(
                name: "max_retries",
                schema: "documents",
                table: "OutboxMessages",
                newName: "MaxRetries");

            migrationBuilder.RenameColumn(
                name: "error_message",
                schema: "documents",
                table: "OutboxMessages",
                newName: "ErrorMessage");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "documents",
                table: "OutboxMessages",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "correlation_id",
                schema: "documents",
                table: "OutboxMessages",
                newName: "CorrelationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OutboxMessages",
                schema: "documents",
                table: "OutboxMessages",
                column: "Id");
        }
    }
}
