using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomationAPI.Migrations
{
    /// <inheritdoc />
    public partial class updateWorkFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "Target",
                table: "AutomationRules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "AutomationRules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Target",
                table: "AutomationRules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
