using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_AspNetUsers_UserId",
                table: "Schedule");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_Courses_CourseId",
                table: "Schedule");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleDay_Lessons_LessonId",
                table: "ScheduleDay");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleDay_Schedule_ScheduleId",
                table: "ScheduleDay");

            migrationBuilder.DropForeignKey(
                name: "FK_UserConfiguration_AspNetUsers_UserId",
                table: "UserConfiguration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserConfiguration",
                table: "UserConfiguration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduleDay",
                table: "ScheduleDay");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Schedule",
                table: "Schedule");

            migrationBuilder.RenameTable(
                name: "UserConfiguration",
                newName: "UserConfigurations");

            migrationBuilder.RenameTable(
                name: "ScheduleDay",
                newName: "ScheduleDays");

            migrationBuilder.RenameTable(
                name: "Schedule",
                newName: "Schedules");

            migrationBuilder.RenameIndex(
                name: "IX_UserConfiguration_UserId",
                table: "UserConfigurations",
                newName: "IX_UserConfigurations_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleDay_ScheduleId",
                table: "ScheduleDays",
                newName: "IX_ScheduleDays_ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleDay_LessonId",
                table: "ScheduleDays",
                newName: "IX_ScheduleDays_LessonId");

            migrationBuilder.RenameIndex(
                name: "IX_Schedule_UserId",
                table: "Schedules",
                newName: "IX_Schedules_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Schedule_CourseId",
                table: "Schedules",
                newName: "IX_Schedules_CourseId");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "ScheduleDays",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Schedules",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "Schedules",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Schedules",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "TeachingDays",
                table: "Schedules",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserConfigurations",
                table: "UserConfigurations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduleDays",
                table: "ScheduleDays",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Schedules",
                table: "Schedules",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleDays_Lessons_LessonId",
                table: "ScheduleDays",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleDays_Schedules_ScheduleId",
                table: "ScheduleDays",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_AspNetUsers_UserId",
                table: "Schedules",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Courses_CourseId",
                table: "Schedules",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserConfigurations_AspNetUsers_UserId",
                table: "UserConfigurations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleDays_Lessons_LessonId",
                table: "ScheduleDays");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleDays_Schedules_ScheduleId",
                table: "ScheduleDays");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_AspNetUsers_UserId",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Courses_CourseId",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_UserConfigurations_AspNetUsers_UserId",
                table: "UserConfigurations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserConfigurations",
                table: "UserConfigurations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Schedules",
                table: "Schedules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduleDays",
                table: "ScheduleDays");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "TeachingDays",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "ScheduleDays");

            migrationBuilder.RenameTable(
                name: "UserConfigurations",
                newName: "UserConfiguration");

            migrationBuilder.RenameTable(
                name: "Schedules",
                newName: "Schedule");

            migrationBuilder.RenameTable(
                name: "ScheduleDays",
                newName: "ScheduleDay");

            migrationBuilder.RenameIndex(
                name: "IX_UserConfigurations_UserId",
                table: "UserConfiguration",
                newName: "IX_UserConfiguration_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_UserId",
                table: "Schedule",
                newName: "IX_Schedule_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_CourseId",
                table: "Schedule",
                newName: "IX_Schedule_CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleDays_ScheduleId",
                table: "ScheduleDay",
                newName: "IX_ScheduleDay_ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleDays_LessonId",
                table: "ScheduleDay",
                newName: "IX_ScheduleDay_LessonId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserConfiguration",
                table: "UserConfiguration",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Schedule",
                table: "Schedule",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduleDay",
                table: "ScheduleDay",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_AspNetUsers_UserId",
                table: "Schedule",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_Courses_CourseId",
                table: "Schedule",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleDay_Lessons_LessonId",
                table: "ScheduleDay",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleDay_Schedule_ScheduleId",
                table: "ScheduleDay",
                column: "ScheduleId",
                principalTable: "Schedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserConfiguration_AspNetUsers_UserId",
                table: "UserConfiguration",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
