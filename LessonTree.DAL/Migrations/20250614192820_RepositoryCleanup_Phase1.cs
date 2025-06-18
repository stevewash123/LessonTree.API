using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RepositoryCleanup_Phase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeriodAssignments_UserConfigurations_UserConfigurationId",
                table: "PeriodAssignments");

            migrationBuilder.DropIndex(
                name: "IX_PeriodAssignments_UserConfig_Period_TeachingDays",
                table: "PeriodAssignments");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "UserConfigurations");

            migrationBuilder.DropColumn(
                name: "PeriodsPerDay",
                table: "UserConfigurations");

            migrationBuilder.DropColumn(
                name: "SchoolYear",
                table: "UserConfigurations");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "UserConfigurations");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Schedules");

            migrationBuilder.RenameColumn(
                name: "TeachingDays",
                table: "Schedules",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "UserConfigurationId",
                table: "PeriodAssignments",
                newName: "ScheduleConfigurationId");

            migrationBuilder.AddColumn<int>(
                name: "ScheduleConfigurationId",
                table: "Schedules",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "SpecialPeriodType",
                table: "PeriodAssignments",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ScheduleConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SchoolYear = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PeriodsPerDay = table.Column<int>(type: "INTEGER", nullable: false),
                    TeachingDays = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsTemplate = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleConfigurations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ScheduleConfigurationId",
                table: "Schedules",
                column: "ScheduleConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodAssignments_ScheduleConfigurationId_Period",
                table: "PeriodAssignments",
                columns: new[] { "ScheduleConfigurationId", "Period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleConfigurations_UserId_IsActive",
                table: "ScheduleConfigurations",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleConfigurations_UserId_SchoolYear",
                table: "ScheduleConfigurations",
                columns: new[] { "UserId", "SchoolYear" });

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodAssignments_ScheduleConfigurations_ScheduleConfigurationId",
                table: "PeriodAssignments",
                column: "ScheduleConfigurationId",
                principalTable: "ScheduleConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_ScheduleConfigurations_ScheduleConfigurationId",
                table: "Schedules",
                column: "ScheduleConfigurationId",
                principalTable: "ScheduleConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeriodAssignments_ScheduleConfigurations_ScheduleConfigurationId",
                table: "PeriodAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_ScheduleConfigurations_ScheduleConfigurationId",
                table: "Schedules");

            migrationBuilder.DropTable(
                name: "ScheduleConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_ScheduleConfigurationId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_PeriodAssignments_ScheduleConfigurationId_Period",
                table: "PeriodAssignments");

            migrationBuilder.DropColumn(
                name: "ScheduleConfigurationId",
                table: "Schedules");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Schedules",
                newName: "TeachingDays");

            migrationBuilder.RenameColumn(
                name: "ScheduleConfigurationId",
                table: "PeriodAssignments",
                newName: "UserConfigurationId");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "UserConfigurations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PeriodsPerDay",
                table: "UserConfigurations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SchoolYear",
                table: "UserConfigurations",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "UserConfigurations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Schedules",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Schedules",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<int>(
                name: "SpecialPeriodType",
                table: "PeriodAssignments",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeriodAssignments_UserConfig_Period_TeachingDays",
                table: "PeriodAssignments",
                columns: new[] { "UserConfigurationId", "Period", "TeachingDays" });

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodAssignments_UserConfigurations_UserConfigurationId",
                table: "PeriodAssignments",
                column: "UserConfigurationId",
                principalTable: "UserConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
