using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialDayIdToScheduleEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpecialDayId",
                table: "ScheduleEvents",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleEvents_SpecialDayId",
                table: "ScheduleEvents",
                column: "SpecialDayId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleEvents_SpecialDays_SpecialDayId",
                table: "ScheduleEvents",
                column: "SpecialDayId",
                principalTable: "SpecialDays",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleEvents_SpecialDays_SpecialDayId",
                table: "ScheduleEvents");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleEvents_SpecialDayId",
                table: "ScheduleEvents");

            migrationBuilder.DropColumn(
                name: "SpecialDayId",
                table: "ScheduleEvents");
        }
    }
}
