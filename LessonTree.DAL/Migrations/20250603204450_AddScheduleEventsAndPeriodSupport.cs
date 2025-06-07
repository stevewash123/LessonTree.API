using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleEventsAndPeriodSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PeriodsPerDay",
                table: "UserConfigurations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PeriodAssignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserConfigurationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Period = table.Column<int>(type: "INTEGER", nullable: false),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: true),
                    Room = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    BackgroundColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    FontColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodAssignment_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PeriodAssignment_UserConfigurations_UserConfigurationId",
                        column: x => x.UserConfigurationId,
                        principalTable: "UserConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ScheduleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Period = table.Column<int>(type: "INTEGER", nullable: false),
                    LessonId = table.Column<int>(type: "INTEGER", nullable: true),
                    SpecialCode = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleEvent_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScheduleEvent_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeriodAssignment_CourseId",
                table: "PeriodAssignment",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodAssignment_UserConfigurationId",
                table: "PeriodAssignment",
                column: "UserConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleEvent_LessonId",
                table: "ScheduleEvent",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleEvents_Schedule_Date_Period",
                table: "ScheduleEvent",
                columns: new[] { "ScheduleId", "Date", "Period" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeriodAssignment");

            migrationBuilder.DropTable(
                name: "ScheduleEvent");

            migrationBuilder.DropColumn(
                name: "PeriodsPerDay",
                table: "UserConfigurations");
        }
    }
}
