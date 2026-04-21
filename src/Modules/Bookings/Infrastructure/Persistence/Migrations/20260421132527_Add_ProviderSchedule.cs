using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_ProviderSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "provider_schedules",
                schema: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                    start_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
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
