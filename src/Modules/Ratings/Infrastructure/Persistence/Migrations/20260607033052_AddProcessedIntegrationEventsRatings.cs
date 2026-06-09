using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedIntegrationEventsRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "processed_integration_events",
                schema: "ratings",
                columns: table => new
                {
                    CorrelationId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_integration_events", x => x.CorrelationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processed_integration_events",
                schema: "ratings");
        }
    }
}
