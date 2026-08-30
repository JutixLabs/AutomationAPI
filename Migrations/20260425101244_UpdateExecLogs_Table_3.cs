using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomationAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExecLogs_Table_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ExecutionLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ExecutionLogs");
        }
    }
}
