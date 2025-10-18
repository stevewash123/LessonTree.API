using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixSpecialDayForeignKeyConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleEvents_SpecialDays_SpecialDayId",
                table: "ScheduleEvents");

            // Recreate the foreign key constraint with SET NULL instead of CASCADE DELETE
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
            // Drop the SET NULL foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleEvents_SpecialDays_SpecialDayId",
                table: "ScheduleEvents");

            // Restore the original CASCADE DELETE foreign key constraint
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