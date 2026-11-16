using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProvaDagua.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "flood_certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddressId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerificationCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PeakWaterLevelCm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    FloodStartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FloodClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsEligibleForAid = table.Column<bool>(type: "boolean", nullable: false),
                    IssuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flood_certificates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "flood_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SensorId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PeakWaterLevelCm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DismissedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flood_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "readings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SensorId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WaterLevelCm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    RainfallMmPerHour = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    MeasuredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_readings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_flood_certificates_FloodEventId_AddressId",
                table: "flood_certificates",
                columns: new[] { "FloodEventId", "AddressId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_flood_certificates_VerificationCode",
                table: "flood_certificates",
                column: "VerificationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_flood_events_SensorId",
                table: "flood_events",
                column: "SensorId",
                unique: true,
                filter: "\"Status\" IN ('Suspected', 'Confirmed')");

            migrationBuilder.CreateIndex(
                name: "IX_readings_SensorId_MeasuredAt",
                table: "readings",
                columns: new[] { "SensorId", "MeasuredAt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flood_certificates");

            migrationBuilder.DropTable(
                name: "flood_events");

            migrationBuilder.DropTable(
                name: "readings");
        }
    }
}
