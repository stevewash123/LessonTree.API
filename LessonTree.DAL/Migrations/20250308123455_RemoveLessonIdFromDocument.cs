using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLessonIdFromDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Lessons_LessonId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_LessonId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "LessonId",
                table: "Documents");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SubTopics",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Assessment",
                table: "Lessons",
                type: "TEXT",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClassTime",
                table: "Lessons",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDateTaught",
                table: "Lessons",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "Lessons",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Materials",
                table: "Lessons",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Methods",
                table: "Lessons",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Objective",
                table: "Lessons",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SpecialNeeds",
                table: "Lessons",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LessonDocuments",
                columns: table => new
                {
                    LessonId = table.Column<int>(type: "INTEGER", nullable: false),
                    DocumentId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonDocuments", x => new { x.LessonId, x.DocumentId });
                    table.ForeignKey(
                        name: "FK_LessonDocuments_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonDocuments_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Standards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    TopicId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Standards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Standards_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonStandards",
                columns: table => new
                {
                    LessonId = table.Column<int>(type: "INTEGER", nullable: false),
                    StandardId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonStandards", x => new { x.LessonId, x.StandardId });
                    table.ForeignKey(
                        name: "FK_LessonStandards_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonStandards_Standards_StandardId",
                        column: x => x.StandardId,
                        principalTable: "Standards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonDocuments_DocumentId",
                table: "LessonDocuments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonStandards_StandardId",
                table: "LessonStandards",
                column: "StandardId");

            migrationBuilder.CreateIndex(
                name: "IX_Standards_TopicId",
                table: "Standards",
                column: "TopicId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonDocuments");

            migrationBuilder.DropTable(
                name: "LessonStandards");

            migrationBuilder.DropTable(
                name: "Standards");

            migrationBuilder.DropColumn(
                name: "Assessment",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "ClassTime",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "LastDateTaught",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Materials",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Methods",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Objective",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "SpecialNeeds",
                table: "Lessons");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SubTopics",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LessonId",
                table: "Documents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_LessonId",
                table: "Documents",
                column: "LessonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Lessons_LessonId",
                table: "Documents",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
