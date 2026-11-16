using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProvaDagua.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MapaSaoPauloEAlertas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000001"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000002"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000003"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000004"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-000000000001"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-000000000002"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-000000000003"));

            migrationBuilder.CreateTable(
                name: "alert_notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddressId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Phone = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    Message = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "alert_subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AddressId = table.Column<Guid>(type: "uuid", nullable: false),
                    Phone = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_subscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sensors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Neighborhood = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Zone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sensors", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "addresses",
                columns: new[] { "Id", "City", "Neighborhood", "Number", "SensorId", "Street" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0001-000000000001"), "São Paulo", "São Miguel Paulista", "120", "sensor-01", "Rua Amazonas" },
                    { new Guid("00000000-0000-0000-0001-000000000002"), "São Paulo", "São Miguel Paulista", "245", "sensor-01", "Rua Amazonas" },
                    { new Guid("00000000-0000-0000-0001-000000000003"), "São Paulo", "São Miguel Paulista", "380", "sensor-01", "Rua Amazonas" },
                    { new Guid("00000000-0000-0000-0002-000000000001"), "São Paulo", "Vila Prudente", "1200", "sensor-02", "Avenida Luiz Ignácio de Anhaia Mello" },
                    { new Guid("00000000-0000-0000-0002-000000000002"), "São Paulo", "Vila Prudente", "1450", "sensor-02", "Avenida Luiz Ignácio de Anhaia Mello" },
                    { new Guid("00000000-0000-0000-0002-000000000003"), "São Paulo", "Vila Prudente", "1700", "sensor-02", "Avenida Luiz Ignácio de Anhaia Mello" },
                    { new Guid("00000000-0000-0000-0003-000000000001"), "São Paulo", "Aricanduva", "2100", "sensor-03", "Avenida Aricanduva" },
                    { new Guid("00000000-0000-0000-0003-000000000002"), "São Paulo", "Aricanduva", "2350", "sensor-03", "Avenida Aricanduva" },
                    { new Guid("00000000-0000-0000-0003-000000000003"), "São Paulo", "Aricanduva", "2600", "sensor-03", "Avenida Aricanduva" },
                    { new Guid("00000000-0000-0000-0004-000000000001"), "São Paulo", "Itaquera", "800", "sensor-04", "Avenida Jacu-Pêssego" },
                    { new Guid("00000000-0000-0000-0004-000000000002"), "São Paulo", "Itaquera", "1050", "sensor-04", "Avenida Jacu-Pêssego" },
                    { new Guid("00000000-0000-0000-0004-000000000003"), "São Paulo", "Itaquera", "1300", "sensor-04", "Avenida Jacu-Pêssego" },
                    { new Guid("00000000-0000-0000-0005-000000000001"), "São Paulo", "Cambuci", "4500", "sensor-05", "Avenida do Estado" },
                    { new Guid("00000000-0000-0000-0005-000000000002"), "São Paulo", "Cambuci", "4800", "sensor-05", "Avenida do Estado" },
                    { new Guid("00000000-0000-0000-0005-000000000003"), "São Paulo", "Cambuci", "5100", "sensor-05", "Avenida do Estado" },
                    { new Guid("00000000-0000-0000-0006-000000000001"), "São Paulo", "Pompeia", "300", "sensor-06", "Avenida Pompeia" },
                    { new Guid("00000000-0000-0000-0006-000000000002"), "São Paulo", "Pompeia", "520", "sensor-06", "Avenida Pompeia" },
                    { new Guid("00000000-0000-0000-0006-000000000003"), "São Paulo", "Pompeia", "780", "sensor-06", "Avenida Pompeia" },
                    { new Guid("00000000-0000-0000-0007-000000000001"), "São Paulo", "Ipiranga", "1000", "sensor-07", "Avenida Doutor Ricardo Jafet" },
                    { new Guid("00000000-0000-0000-0007-000000000002"), "São Paulo", "Ipiranga", "1300", "sensor-07", "Avenida Doutor Ricardo Jafet" },
                    { new Guid("00000000-0000-0000-0007-000000000003"), "São Paulo", "Ipiranga", "1600", "sensor-07", "Avenida Doutor Ricardo Jafet" },
                    { new Guid("00000000-0000-0000-0008-000000000001"), "São Paulo", "Freguesia do Ó", "200", "sensor-08", "Avenida Antônio Munhoz Bonilha" },
                    { new Guid("00000000-0000-0000-0008-000000000002"), "São Paulo", "Freguesia do Ó", "450", "sensor-08", "Avenida Antônio Munhoz Bonilha" },
                    { new Guid("00000000-0000-0000-0008-000000000003"), "São Paulo", "Freguesia do Ó", "700", "sensor-08", "Avenida Antônio Munhoz Bonilha" }
                });

            migrationBuilder.InsertData(
                table: "sensors",
                columns: new[] { "Id", "Latitude", "Longitude", "Name", "Neighborhood", "Zone" },
                values: new object[,]
                {
                    { "sensor-01", -23.492999999999999, -46.441000000000003, "Rua Amazonas", "São Miguel Paulista", "Leste" },
                    { "sensor-02", -23.584299999999999, -46.581000000000003, "Av. Luiz Ignácio de Anhaia Mello", "Vila Prudente", "Leste" },
                    { "sensor-03", -23.564, -46.518000000000001, "Av. Aricanduva", "Aricanduva", "Leste" },
                    { "sensor-04", -23.545000000000002, -46.454000000000001, "Av. Jacu-Pêssego", "Itaquera", "Leste" },
                    { "sensor-05", -23.556000000000001, -46.619999999999997, "Av. do Estado", "Cambuci", "Centro" },
                    { "sensor-06", -23.530999999999999, -46.686, "Av. Pompeia", "Pompeia", "Oeste" },
                    { "sensor-07", -23.59, -46.609499999999997, "Av. Ricardo Jafet", "Ipiranga", "Sul" },
                    { "sensor-08", -23.497, -46.695999999999998, "Av. Antônio Munhoz Bonilha", "Freguesia do Ó", "Norte" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_alert_notifications_AddressId",
                table: "alert_notifications",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_alert_notifications_SubscriptionId_FloodEventId",
                table: "alert_notifications",
                columns: new[] { "SubscriptionId", "FloodEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alert_subscriptions_AddressId_Phone",
                table: "alert_subscriptions",
                columns: new[] { "AddressId", "Phone" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alert_notifications");

            migrationBuilder.DropTable(
                name: "alert_subscriptions");

            migrationBuilder.DropTable(
                name: "sensors");

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000001"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000002"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000003"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000001"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000002"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000003"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0003-000000000001"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0003-000000000002"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0003-000000000003"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0004-000000000001"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0004-000000000002"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0004-000000000003"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0005-000000000001"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0005-000000000002"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0005-000000000003"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0006-000000000001"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0006-000000000002"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0006-000000000003"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0007-000000000001"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0007-000000000002"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0007-000000000003"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0008-000000000001"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0008-000000000002"));

            migrationBuilder.DeleteData(
                table: "addresses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0008-000000000003"));

            migrationBuilder.InsertData(
                table: "addresses",
                columns: new[] { "Id", "City", "Neighborhood", "Number", "SensorId", "Street" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-000000000001"), "São Paulo", "Jardim das Águas", "10", "sensor-01", "Rua das Flores" },
                    { new Guid("11111111-1111-1111-1111-000000000002"), "São Paulo", "Jardim das Águas", "25", "sensor-01", "Rua das Flores" },
                    { new Guid("11111111-1111-1111-1111-000000000003"), "São Paulo", "Jardim das Águas", "40", "sensor-01", "Rua das Flores" },
                    { new Guid("11111111-1111-1111-1111-000000000004"), "São Paulo", "Jardim das Águas", "57", "sensor-01", "Rua das Flores" },
                    { new Guid("22222222-2222-2222-2222-000000000001"), "São Paulo", "Jardim das Águas", "5", "sensor-02", "Rua dos Ipês" },
                    { new Guid("22222222-2222-2222-2222-000000000002"), "São Paulo", "Jardim das Águas", "18", "sensor-02", "Rua dos Ipês" },
                    { new Guid("22222222-2222-2222-2222-000000000003"), "São Paulo", "Jardim das Águas", "33", "sensor-02", "Rua dos Ipês" }
                });
        }
    }
}
