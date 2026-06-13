using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zn.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndSearchLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SenderIpHash",
                table: "Messages",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatorIpHash",
                table: "Blogs",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SearchLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Term = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserFullName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IpHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SearchedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_SearchedAt",
                table: "SearchLogs",
                column: "SearchedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_UserId",
                table: "SearchLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchLogs");

            migrationBuilder.DropColumn(
                name: "SenderIpHash",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "CreatorIpHash",
                table: "Blogs");
        }
    }
}
