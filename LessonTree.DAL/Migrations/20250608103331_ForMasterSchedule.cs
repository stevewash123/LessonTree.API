using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ForMasterSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Courses_CourseId",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "SpecialCode",
                table: "ScheduleEvents");

            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                table: "Schedules",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "ScheduleEvents",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventCategory",
                table: "ScheduleEvents",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "ScheduleEvents",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Courses_CourseId",
                table: "Schedules",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Courses_CourseId",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "ScheduleEvents");

            migrationBuilder.DropColumn(
                name: "EventCategory",
                table: "ScheduleEvents");

            migrationBuilder.DropColumn(
                name: "EventType",
                table: "ScheduleEvents");

            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                table: "Schedules",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialCode",
                table: "ScheduleEvents",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Courses_CourseId",
                table: "Schedules",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
