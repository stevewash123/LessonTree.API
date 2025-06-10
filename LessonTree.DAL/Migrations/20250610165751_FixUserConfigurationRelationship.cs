using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LessonTree.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixUserConfigurationRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserConfigurations_AspNetUsers_UserId1",
                table: "UserConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_UserConfigurations_UserId1",
                table: "UserConfigurations");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserConfigurations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "UserConfigurations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserConfigurations_UserId1",
                table: "UserConfigurations",
                column: "UserId1",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserConfigurations_AspNetUsers_UserId1",
                table: "UserConfigurations",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
