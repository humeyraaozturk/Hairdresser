using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Hairdresser.Migrations
{
    /// <inheritdoc />
    public partial class AddServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Duration", "Price" },
                values: new object[] { 1, 700.0 });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Duration", "Price" },
                values: new object[] { 3, 1500.0 });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Duration", "Name", "Price" },
                values: new object[] { 2, "Saç Bakımı", 1000.0 });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "Duration", "Name", "Price" },
                values: new object[,]
                {
                    { 4, 2, "Profesyonel Makyaj", 1000.0 },
                    { 5, 1, "Nail Art", 1500.0 },
                    { 6, 3, "Cilt Bakımı", 1500.0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Duration", "Price" },
                values: new object[] { 0, 0.0 });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Duration", "Price" },
                values: new object[] { 0, 0.0 });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Duration", "Name", "Price" },
                values: new object[] { 0, "Fön", 0.0 });
        }
    }
}
