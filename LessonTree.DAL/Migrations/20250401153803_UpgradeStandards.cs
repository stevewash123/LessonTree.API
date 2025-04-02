using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpgradeStandards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Standards_Topics_TopicId",
                table: "Standards");

            migrationBuilder.AlterColumn<int>(
                name: "TopicId",
                table: "Standards",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "Standards",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "Standards",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Standards_CourseId",
                table: "Standards",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Standards_DistrictId",
                table: "Standards",
                column: "DistrictId");

            migrationBuilder.AddForeignKey(
                name: "FK_Standards_Courses_CourseId",
                table: "Standards",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Standards_Districts_DistrictId",
                table: "Standards",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Standards_Topics_TopicId",
                table: "Standards",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Standards_Courses_CourseId",
                table: "Standards");

            migrationBuilder.DropForeignKey(
                name: "FK_Standards_Districts_DistrictId",
                table: "Standards");

            migrationBuilder.DropForeignKey(
                name: "FK_Standards_Topics_TopicId",
                table: "Standards");

            migrationBuilder.DropIndex(
                name: "IX_Standards_CourseId",
                table: "Standards");

            migrationBuilder.DropIndex(
                name: "IX_Standards_DistrictId",
                table: "Standards");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Standards");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Standards");

            migrationBuilder.AlterColumn<int>(
                name: "TopicId",
                table: "Standards",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Standards_Topics_TopicId",
                table: "Standards",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
