using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomationAPI.Migrations
{
    /// <inheritdoc />
    public partial class TrelloWatcherTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrelloWatchedBoards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BoardId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BoardName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrelloWebhookId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrelloWatchedBoards", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrelloWatchedBoards");
        }
    }
}
