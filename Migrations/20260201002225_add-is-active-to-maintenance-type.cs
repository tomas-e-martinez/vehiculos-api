using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vehiculos_api.Migrations
{
    /// <inheritdoc />
    public partial class addisactivetomaintenancetype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "MaintenanceTypes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "MaintenanceTypes");
        }
    }
}
