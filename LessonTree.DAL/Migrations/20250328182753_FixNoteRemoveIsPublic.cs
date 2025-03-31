using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixNoteRemoveIsPublic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Note_AspNetUsers_UserId",
                table: "Note");

            migrationBuilder.DropForeignKey(
                name: "FK_Note_Courses_CourseId",
                table: "Note");

            migrationBuilder.DropForeignKey(
                name: "FK_Note_Lessons_LessonId",
                table: "Note");

            migrationBuilder.DropForeignKey(
                name: "FK_Note_SubTopics_SubTopicId",
                table: "Note");

            migrationBuilder.DropForeignKey(
                name: "FK_Note_Team_TeamId",
                table: "Note");

            migrationBuilder.DropForeignKey(
                name: "FK_Note_Topics_TopicId",
                table: "Note");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Note",
                table: "Note");

            migrationBuilder.RenameTable(
                name: "Note",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "IsPublic",
                table: "Notes",
                newName: "Visibility");

            migrationBuilder.RenameIndex(
                name: "IX_Note_UserId",
                table: "Notes",
                newName: "IX_Notes_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Note_TopicId",
                table: "Notes",
                newName: "IX_Notes_TopicId");

            migrationBuilder.RenameIndex(
                name: "IX_Note_TeamId",
                table: "Notes",
                newName: "IX_Notes_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_Note_SubTopicId",
                table: "Notes",
                newName: "IX_Notes_SubTopicId");

            migrationBuilder.RenameIndex(
                name: "IX_Note_LessonId",
                table: "Notes",
                newName: "IX_Notes_LessonId");

            migrationBuilder.RenameIndex(
                name: "IX_Note_CourseId",
                table: "Notes",
                newName: "IX_Notes_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notes",
                table: "Notes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_AspNetUsers_UserId",
                table: "Notes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Courses_CourseId",
                table: "Notes",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Lessons_LessonId",
                table: "Notes",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_SubTopics_SubTopicId",
                table: "Notes",
                column: "SubTopicId",
                principalTable: "SubTopics",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Team_TeamId",
                table: "Notes",
                column: "TeamId",
                principalTable: "Team",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Topics_TopicId",
                table: "Notes",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_AspNetUsers_UserId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Courses_CourseId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Lessons_LessonId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_SubTopics_SubTopicId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Team_TeamId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Topics_TopicId",
                table: "Notes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notes",
                table: "Notes");

            migrationBuilder.RenameTable(
                name: "Notes",
                newName: "Note");

            migrationBuilder.RenameColumn(
                name: "Visibility",
                table: "Note",
                newName: "IsPublic");

            migrationBuilder.RenameIndex(
                name: "IX_Notes_UserId",
                table: "Note",
                newName: "IX_Note_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Notes_TopicId",
                table: "Note",
                newName: "IX_Note_TopicId");

            migrationBuilder.RenameIndex(
                name: "IX_Notes_TeamId",
                table: "Note",
                newName: "IX_Note_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_Notes_SubTopicId",
                table: "Note",
                newName: "IX_Note_SubTopicId");

            migrationBuilder.RenameIndex(
                name: "IX_Notes_LessonId",
                table: "Note",
                newName: "IX_Note_LessonId");

            migrationBuilder.RenameIndex(
                name: "IX_Notes_CourseId",
                table: "Note",
                newName: "IX_Note_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Note",
                table: "Note",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Note_AspNetUsers_UserId",
                table: "Note",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Note_Courses_CourseId",
                table: "Note",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Note_Lessons_LessonId",
                table: "Note",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Note_SubTopics_SubTopicId",
                table: "Note",
                column: "SubTopicId",
                principalTable: "SubTopics",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Note_Team_TeamId",
                table: "Note",
                column: "TeamId",
                principalTable: "Team",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Note_Topics_TopicId",
                table: "Note",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id");
        }
    }
}
