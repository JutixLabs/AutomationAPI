using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomationAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDelay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DelayAmount",
                table: "WorkFlowSteps",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DelayUnit",
                table: "WorkFlowSteps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FalseStepId",
                table: "WorkFlowSteps",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBranchStep",
                table: "WorkFlowSteps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TrueStepId",
                table: "WorkFlowSteps",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RuleConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleStepId = table.Column<int>(type: "int", nullable: false),
                    Field = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Operator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RuleConditions_WorkFlowSteps_RuleStepId",
                        column: x => x.RuleStepId,
                        principalTable: "WorkFlowSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RuleConditions_RuleStepId",
                table: "RuleConditions",
                column: "RuleStepId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RuleConditions");

            migrationBuilder.DropColumn(
                name: "DelayAmount",
                table: "WorkFlowSteps");

            migrationBuilder.DropColumn(
                name: "DelayUnit",
                table: "WorkFlowSteps");

            migrationBuilder.DropColumn(
                name: "FalseStepId",
                table: "WorkFlowSteps");

            migrationBuilder.DropColumn(
                name: "IsBranchStep",
                table: "WorkFlowSteps");

            migrationBuilder.DropColumn(
                name: "TrueStepId",
                table: "WorkFlowSteps");
        }
    }
}
