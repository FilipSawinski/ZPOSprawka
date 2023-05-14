using Microsoft.EntityFrameworkCore.Migrations;

namespace ZPOSprawka.Data.Migrations
{
    public partial class AddLeaderToCourseGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LeaderId",
                table: "CourseGroups",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CourseGroups_LeaderId",
                table: "CourseGroups",
                column: "LeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseGroups_AspNetUsers_LeaderId",
                table: "CourseGroups",
                column: "LeaderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseGroups_AspNetUsers_LeaderId",
                table: "CourseGroups");

            migrationBuilder.DropIndex(
                name: "IX_CourseGroups_LeaderId",
                table: "CourseGroups");

            migrationBuilder.DropColumn(
                name: "LeaderId",
                table: "CourseGroups");
        }
    }
}
