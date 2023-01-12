using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArticlesApp.Data.Migrations
{
    public partial class test : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberRequests");

            migrationBuilder.DropTable(
                name: "Memberships");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Articles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Articles_GroupId",
                table: "Articles",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Groups_GroupId",
                table: "Articles",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Groups_GroupId",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_GroupId",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Articles");

            migrationBuilder.CreateTable(
                name: "MemberRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    DateRequested = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsInWait = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberRequests", x => new { x.Id, x.UserId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_MemberRequests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberRequests_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Memberships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    DateJoined = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsModerator = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memberships", x => new { x.Id, x.UserId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_Memberships_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Memberships_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberRequests_GroupId",
                table: "MemberRequests",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberRequests_UserId",
                table: "MemberRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_GroupId",
                table: "Memberships",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_UserId",
                table: "Memberships",
                column: "UserId");
        }
    }
}
