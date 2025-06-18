using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixCourseIdShadowProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleEvents_Lessons_LessonId",
                table: "ScheduleEvents");

            migrationBuilder.AddColumn<int>(
                name: "LessonId1",
                table: "ScheduleEvents",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleEvents_LessonId1",
                table: "ScheduleEvents",
                column: "LessonId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleEvents_Lessons_LessonId",
                table: "ScheduleEvents",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleEvents_Lessons_LessonId1",
                table: "ScheduleEvents",
                column: "LessonId1",
                principalTable: "Lessons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleEvents_Lessons_LessonId",
                table: "ScheduleEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleEvents_Lessons_LessonId1",
                table: "ScheduleEvents");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleEvents_LessonId1",
                table: "ScheduleEvents");

            migrationBuilder.DropColumn(
                name: "LessonId1",
                table: "ScheduleEvents");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleEvents_Lessons_LessonId",
                table: "ScheduleEvents",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id");
        }
    }
}
