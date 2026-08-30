using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomationAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkSpace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "AutomationRules",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkSpaces",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkSpaces", x => x.ID);
                    table.ForeignKey(
                        name: "FK_WorkSpaces_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkSpaceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Folders_WorkSpaces_WorkSpaceId",
                        column: x => x.WorkSpaceId,
                        principalTable: "WorkSpaces",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRules_FolderId",
                table: "AutomationRules",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_WorkSpaceId",
                table: "Folders",
                column: "WorkSpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSpaces_UserId",
                table: "WorkSpaces",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AutomationRules_Folders_FolderId",
                table: "AutomationRules",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AutomationRules_Folders_FolderId",
                table: "AutomationRules");

            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropTable(
                name: "WorkSpaces");

            migrationBuilder.DropIndex(
                name: "IX_AutomationRules_FolderId",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "AutomationRules");
        }
    }
}
