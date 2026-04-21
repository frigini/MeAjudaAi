using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalEventIdToInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_inbox_messages_processed_at_created_at",
                schema: "payments",
                table: "inbox_messages");

            migrationBuilder.DropColumn(
                name: "started_at",
                schema: "payments",
                table: "subscriptions");

            migrationBuilder.AddColumn<string>(
                name: "external_event_id",
                schema: "payments",
                table: "inbox_messages",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_external_event_id",
                schema: "payments",
                table: "inbox_messages",
                column: "external_event_id",
                unique: true,
                filter: "external_event_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_processed_at_next_attempt_at_created_at",
                schema: "payments",
                table: "inbox_messages",
                columns: new[] { "processed_at", "next_attempt_at", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_inbox_messages_external_event_id",
                schema: "payments",
                table: "inbox_messages");

            migrationBuilder.DropIndex(
                name: "IX_inbox_messages_processed_at_next_attempt_at_created_at",
                schema: "payments",
                table: "inbox_messages");

            migrationBuilder.DropColumn(
                name: "external_event_id",
                schema: "payments",
                table: "inbox_messages");

            migrationBuilder.AddColumn<DateTime>(
                name: "started_at",
                schema: "payments",
                table: "subscriptions",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_processed_at_created_at",
                schema: "payments",
                table: "inbox_messages",
                columns: new[] { "processed_at", "created_at" });
        }
    }
}
