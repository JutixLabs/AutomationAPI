using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomationAPI.Migrations
{
    /// <inheritdoc />
    public partial class updateWorkflowStep2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Provider",
                table: "WorkFlowSteps");

            migrationBuilder.DropColumn(
                name: "ResourceName",
                table: "WorkFlowSteps");

            migrationBuilder.AlterColumn<string>(
                name: "LoopField",
                table: "WorkFlowSteps",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LoopField",
                table: "WorkFlowSteps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "WorkFlowSteps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResourceName",
                table: "WorkFlowSteps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
