using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleEventsAndPeriodSupportDebugged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleEvent_Lessons_LessonId",
                table: "ScheduleEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleEvent_Schedules_ScheduleId",
                table: "ScheduleEvent");

            migrationBuilder.DropTable(
                name: "ScheduleDays");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduleEvent",
                table: "ScheduleEvent");

            migrationBuilder.RenameTable(
                name: "ScheduleEvent",
                newName: "ScheduleEvents");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleEvent_LessonId",
                table: "ScheduleEvents",
                newName: "IX_ScheduleEvents_LessonId");

            migrationBuilder.AddColumn<string>(
                name: "SchoolYear",
                table: "UserConfigurations",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PeriodAssignment",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectionName",
                table: "PeriodAssignment",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduleEvents",
                table: "ScheduleEvents",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleEvents_Lessons_LessonId",
                table: "ScheduleEvents",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleEvents_Schedules_ScheduleId",
                table: "ScheduleEvents",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleEvents_Lessons_LessonId",
                table: "ScheduleEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleEvents_Schedules_ScheduleId",
                table: "ScheduleEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduleEvents",
                table: "ScheduleEvents");

            migrationBuilder.DropColumn(
                name: "SchoolYear",
                table: "UserConfigurations");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PeriodAssignment");

            migrationBuilder.DropColumn(
                name: "SectionName",
                table: "PeriodAssignment");

            migrationBuilder.RenameTable(
                name: "ScheduleEvents",
                newName: "ScheduleEvent");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleEvents_LessonId",
                table: "ScheduleEvent",
                newName: "IX_ScheduleEvent_LessonId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduleEvent",
                table: "ScheduleEvent",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ScheduleDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LessonId = table.Column<int>(type: "INTEGER", nullable: true),
                    ScheduleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SpecialCode = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleDays_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScheduleDays_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDays_LessonId",
                table: "ScheduleDays",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDays_ScheduleId",
                table: "ScheduleDays",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleEvent_Lessons_LessonId",
                table: "ScheduleEvent",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleEvent_Schedules_ScheduleId",
                table: "ScheduleEvent",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
