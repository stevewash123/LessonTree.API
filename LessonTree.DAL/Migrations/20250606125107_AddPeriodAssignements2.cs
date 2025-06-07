using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPeriodAssignements2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeriodAssignment_Courses_CourseId1",
                table: "PeriodAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_PeriodAssignment_Courses_CourseId_Positive_Only",
                table: "PeriodAssignment");

            migrationBuilder.DropIndex(
                name: "IX_PeriodAssignment_CourseId1",
                table: "PeriodAssignment");

            migrationBuilder.DropColumn(
                name: "CourseId1",
                table: "PeriodAssignment");

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodAssignment_Courses_CourseId",
                table: "PeriodAssignment",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeriodAssignment_Courses_CourseId",
                table: "PeriodAssignment");

            migrationBuilder.AddColumn<int>(
                name: "CourseId1",
                table: "PeriodAssignment",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeriodAssignment_CourseId1",
                table: "PeriodAssignment",
                column: "CourseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodAssignment_Courses_CourseId1",
                table: "PeriodAssignment",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodAssignment_Courses_CourseId_Positive_Only",
                table: "PeriodAssignment",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
