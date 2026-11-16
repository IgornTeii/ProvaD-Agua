using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProvaDagua.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAddresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Street = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Neighborhood = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SensorId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addresses", x => x.Id);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_addresses_SensorId",
                table: "addresses",
                column: "SensorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "addresses");
        }
    }
}
