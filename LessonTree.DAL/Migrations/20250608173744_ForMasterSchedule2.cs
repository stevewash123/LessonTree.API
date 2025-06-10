using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ForMasterSchedule2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SectionName",
                table: "PeriodAssignment");

            migrationBuilder.AddColumn<int>(
                name: "SpecialPeriodType",
                table: "PeriodAssignment",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpecialPeriodType",
                table: "PeriodAssignment");

            migrationBuilder.AddColumn<string>(
                name: "SectionName",
                table: "PeriodAssignment",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }
    }
}
