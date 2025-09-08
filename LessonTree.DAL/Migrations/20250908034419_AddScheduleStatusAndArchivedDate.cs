using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleStatusAndArchivedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleEvents_SpecialDays_SpecialDayId",
                table: "ScheduleEvents");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "ScheduleConfigurations",
                newName: "Status");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleConfigurations_UserId_IsActive",
                table: "ScheduleConfigurations",
                newName: "IX_ScheduleConfigurations_UserId_Status");

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedDate",
                table: "ScheduleConfigurations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleEvents_SpecialDays_SpecialDayId",
                table: "ScheduleEvents",
                column: "SpecialDayId",
                principalTable: "SpecialDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleEvents_SpecialDays_SpecialDayId",
                table: "ScheduleEvents");

            migrationBuilder.DropColumn(
                name: "ArchivedDate",
                table: "ScheduleConfigurations");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "ScheduleConfigurations",
                newName: "IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleConfigurations_UserId_Status",
                table: "ScheduleConfigurations",
                newName: "IX_ScheduleConfigurations_UserId_IsActive");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleEvents_SpecialDays_SpecialDayId",
                table: "ScheduleEvents",
                column: "SpecialDayId",
                principalTable: "SpecialDays",
                principalColumn: "Id");
        }
    }
}
