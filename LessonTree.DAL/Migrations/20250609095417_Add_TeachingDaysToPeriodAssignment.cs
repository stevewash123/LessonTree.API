using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Add_TeachingDaysToPeriodAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeriodAssignment_UserConfigurations_UserConfigurationId",
                table: "PeriodAssignment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PeriodAssignment",
                table: "PeriodAssignment");

            migrationBuilder.DropIndex(
                name: "IX_PeriodAssignment_UserConfigurationId",
                table: "PeriodAssignment");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PeriodAssignment_CourseId_PositiveOrNull",
                table: "PeriodAssignment");

            migrationBuilder.RenameTable(
                name: "PeriodAssignment",
                newName: "PeriodAssignments");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "UserConfigurations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "UserConfigurations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "UserConfigurations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeachingDays",
                table: "PeriodAssignments",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PeriodAssignments",
                table: "PeriodAssignments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserConfigurations_UserId1",
                table: "UserConfigurations",
                column: "UserId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeriodAssignments_UserConfig_Period_TeachingDays",
                table: "PeriodAssignments",
                columns: new[] { "UserConfigurationId", "Period", "TeachingDays" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_PeriodAssignment_CourseId_Positive",
                table: "PeriodAssignments",
                sql: "CourseId IS NULL OR CourseId > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PeriodAssignment_ExclusiveAssignment",
                table: "PeriodAssignments",
                sql: "(CourseId IS NOT NULL AND SpecialPeriodType IS NULL) OR (CourseId IS NULL AND SpecialPeriodType IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PeriodAssignment_TeachingDays_NotEmpty",
                table: "PeriodAssignments",
                sql: "TeachingDays IS NOT NULL AND LENGTH(TRIM(TeachingDays)) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PeriodAssignment_TeachingDays_ValidDays",
                table: "PeriodAssignments",
                sql: "TeachingDays NOT LIKE '%[^MondayTueswdhFrig,]%' AND \r\n                        (TeachingDays LIKE '%Monday%' OR \r\n                        TeachingDays LIKE '%Tuesday%' OR \r\n                        TeachingDays LIKE '%Wednesday%' OR \r\n                        TeachingDays LIKE '%Thursday%' OR \r\n                        TeachingDays LIKE '%Friday%' OR\r\n                        TeachingDays LIKE '%Saturday%' OR\r\n                        TeachingDays LIKE '%Sunday%')");

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodAssignments_UserConfigurations_UserConfigurationId",
                table: "PeriodAssignments",
                column: "UserConfigurationId",
                principalTable: "UserConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserConfigurations_AspNetUsers_UserId1",
                table: "UserConfigurations",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeriodAssignments_UserConfigurations_UserConfigurationId",
                table: "PeriodAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserConfigurations_AspNetUsers_UserId1",
                table: "UserConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_UserConfigurations_UserId1",
                table: "UserConfigurations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PeriodAssignments",
                table: "PeriodAssignments");

            migrationBuilder.DropIndex(
                name: "IX_PeriodAssignments_UserConfig_Period_TeachingDays",
                table: "PeriodAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PeriodAssignment_CourseId_Positive",
                table: "PeriodAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PeriodAssignment_ExclusiveAssignment",
                table: "PeriodAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PeriodAssignment_TeachingDays_NotEmpty",
                table: "PeriodAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PeriodAssignment_TeachingDays_ValidDays",
                table: "PeriodAssignments");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "UserConfigurations");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "UserConfigurations");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserConfigurations");

            migrationBuilder.DropColumn(
                name: "TeachingDays",
                table: "PeriodAssignments");

            migrationBuilder.RenameTable(
                name: "PeriodAssignments",
                newName: "PeriodAssignment");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PeriodAssignment",
                table: "PeriodAssignment",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodAssignment_UserConfigurationId",
                table: "PeriodAssignment",
                column: "UserConfigurationId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PeriodAssignment_CourseId_PositiveOrNull",
                table: "PeriodAssignment",
                sql: "CourseId IS NULL OR CourseId > 0 OR CourseId < 0");

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodAssignment_UserConfigurations_UserConfigurationId",
                table: "PeriodAssignment",
                column: "UserConfigurationId",
                principalTable: "UserConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
