using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArticlesApp.Data.Migrations
{
    public partial class userextend : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Privacy",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Privacy",
                table: "AspNetUsers");
        }
    }
}
