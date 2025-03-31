using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixNoteAddAuthor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Notes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                table: "Notes");
        }
    }
}
