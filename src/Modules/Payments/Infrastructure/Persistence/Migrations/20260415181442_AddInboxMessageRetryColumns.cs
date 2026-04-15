using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInboxMessageRetryColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_inbox_messages_created_at",
                schema: "payments",
                table: "inbox_messages");

            migrationBuilder.DropIndex(
                name: "IX_inbox_messages_processed_at",
                schema: "payments",
                table: "inbox_messages");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "payments",
                table: "transactions",
                newName: "updated_at");

            migrationBuilder.AddColumn<int>(
                name: "max_retries",
                schema: "payments",
                table: "inbox_messages",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<DateTime>(
                name: "next_attempt_at",
                schema: "payments",
                table: "inbox_messages",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "retry_count",
                schema: "payments",
                table: "inbox_messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_processed_at_created_at",
                schema: "payments",
                table: "inbox_messages",
                columns: new[] { "processed_at", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_inbox_messages_processed_at_created_at",
                schema: "payments",
                table: "inbox_messages");

            migrationBuilder.DropColumn(
                name: "max_retries",
                schema: "payments",
                table: "inbox_messages");

            migrationBuilder.DropColumn(
                name: "next_attempt_at",
                schema: "payments",
                table: "inbox_messages");

            migrationBuilder.DropColumn(
                name: "retry_count",
                schema: "payments",
                table: "inbox_messages");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "payments",
                table: "transactions",
                newName: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_created_at",
                schema: "payments",
                table: "inbox_messages",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_processed_at",
                schema: "payments",
                table: "inbox_messages",
                column: "processed_at");
        }
    }
}
