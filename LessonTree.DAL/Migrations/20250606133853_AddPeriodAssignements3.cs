using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPeriodAssignements3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeriodAssignment_Courses_CourseId",
                table: "PeriodAssignment");

            migrationBuilder.DropIndex(
                name: "IX_PeriodAssignment_CourseId",
                table: "PeriodAssignment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PeriodAssignment_CourseId",
                table: "PeriodAssignment",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodAssignment_Courses_CourseId",
                table: "PeriodAssignment",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }
    }
}
