using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Bookings_Fixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "bookings");

            migrationBuilder.CreateTable(
                name: "bookings",
                schema: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_date = table.Column<DateOnly>(type: "date", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "provider_schedules",
                schema: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    time_zone_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_schedules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "provider_availabilities",
                schema: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<string>(type: "text", nullable: false),
                    provider_schedule_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_availabilities", x => x.id);
                    table.ForeignKey(
                        name: "FK_provider_availabilities_provider_schedules_provider_schedul~",
                        column: x => x.provider_schedule_id,
                        principalSchema: "bookings",
                        principalTable: "provider_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "provider_availability_slots",
                schema: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    availability_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_availability_slots", x => x.id);
                    table.ForeignKey(
                        name: "FK_provider_availability_slots_provider_availabilities_availab~",
                        column: x => x.availability_id,
                        principalSchema: "bookings",
                        principalTable: "provider_availabilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_client_id",
                schema: "bookings",
                table: "bookings",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_provider_id_booking_date_status",
                schema: "bookings",
                table: "bookings",
                columns: new[] { "provider_id", "booking_date", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_status",
                schema: "bookings",
                table: "bookings",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_provider_availabilities_day_of_week_provider_schedule_id",
                schema: "bookings",
                table: "provider_availabilities",
                columns: new[] { "day_of_week", "provider_schedule_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_provider_availabilities_provider_schedule_id",
                schema: "bookings",
                table: "provider_availabilities",
                column: "provider_schedule_id");

            migrationBuilder.CreateIndex(
                name: "IX_provider_availability_slots_availability_id",
                schema: "bookings",
                table: "provider_availability_slots",
                column: "availability_id");

            migrationBuilder.CreateIndex(
                name: "IX_provider_schedules_provider_id",
                schema: "bookings",
                table: "provider_schedules",
                column: "provider_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bookings",
                schema: "bookings");

            migrationBuilder.DropTable(
                name: "provider_availability_slots",
                schema: "bookings");

            migrationBuilder.DropTable(
                name: "provider_availabilities",
                schema: "bookings");

            migrationBuilder.DropTable(
                name: "provider_schedules",
                schema: "bookings");
        }
    }
}
