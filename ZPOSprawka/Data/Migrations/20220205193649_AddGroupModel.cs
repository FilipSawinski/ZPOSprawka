using Microsoft.EntityFrameworkCore.Migrations;

namespace ZPOSprawka.Data.Migrations
{
    public partial class AddGroupModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CourseGroupId",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CourseGroups",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    isActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CourseGroupId",
                table: "AspNetUsers",
                column: "CourseGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_CourseGroups_CourseGroupId",
                table: "AspNetUsers",
                column: "CourseGroupId",
                principalTable: "CourseGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_CourseGroups_CourseGroupId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CourseGroups");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CourseGroupId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CourseGroupId",
                table: "AspNetUsers");
        }
    }
}
