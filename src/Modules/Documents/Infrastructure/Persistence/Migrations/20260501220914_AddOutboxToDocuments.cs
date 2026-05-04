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
            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    max_retries = table.Column<int>(type: "integer", nullable: false),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_status_scheduled_priority",
                schema: "documents",
                table: "outbox_messages",
                columns: new[] { "status", "scheduled_at", "priority", "created_at" });
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
