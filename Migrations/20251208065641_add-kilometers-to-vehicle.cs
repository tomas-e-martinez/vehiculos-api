using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vehiculos_api.Migrations
{
    /// <inheritdoc />
    public partial class addkilometerstovehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kilometers",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kilometers",
                table: "Vehicles");
        }
    }
}
