using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToSpecialDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleEvents_SpecialDays_SpecialDayId",
                table: "ScheduleEvents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PeriodAssignment_TeachingDays_ValidDays",
                table: "PeriodAssignments");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SpecialDays",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PeriodAssignment_TeachingDays_ValidDays",
                table: "PeriodAssignments",
                sql: "TeachingDays NOT LIKE '%[^MondayTueswdhFrig,]%' AND \n                        (TeachingDays LIKE '%Monday%' OR \n                        TeachingDays LIKE '%Tuesday%' OR \n                        TeachingDays LIKE '%Wednesday%' OR \n                        TeachingDays LIKE '%Thursday%' OR \n                        TeachingDays LIKE '%Friday%' OR\n                        TeachingDays LIKE '%Saturday%' OR\n                        TeachingDays LIKE '%Sunday%')");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleEvents_SpecialDays_SpecialDayId",
                table: "ScheduleEvents",
                column: "SpecialDayId",
                principalTable: "SpecialDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleEvents_SpecialDays_SpecialDayId",
                table: "ScheduleEvents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PeriodAssignment_TeachingDays_ValidDays",
                table: "PeriodAssignments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SpecialDays");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PeriodAssignment_TeachingDays_ValidDays",
                table: "PeriodAssignments",
                sql: "TeachingDays NOT LIKE '%[^MondayTueswdhFrig,]%' AND \r\n                        (TeachingDays LIKE '%Monday%' OR \r\n                        TeachingDays LIKE '%Tuesday%' OR \r\n                        TeachingDays LIKE '%Wednesday%' OR \r\n                        TeachingDays LIKE '%Thursday%' OR \r\n                        TeachingDays LIKE '%Friday%' OR\r\n                        TeachingDays LIKE '%Saturday%' OR\r\n                        TeachingDays LIKE '%Sunday%')");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleEvents_SpecialDays_SpecialDayId",
                table: "ScheduleEvents",
                column: "SpecialDayId",
                principalTable: "SpecialDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
