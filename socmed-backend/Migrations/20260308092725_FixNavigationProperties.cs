using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace socmed_backend.Migrations
{
    /// <inheritdoc />
    public partial class FixNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    Bio = table.Column<string>(type: "TEXT", nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rants_UserId",
                table: "Rants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RantReRants_UserId",
                table: "RantReRants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RantLikes_UserId",
                table: "RantLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RantBookmarks_UserId",
                table: "RantBookmarks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RantBookmarks_Users_UserId",
                table: "RantBookmarks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RantLikes_Users_UserId",
                table: "RantLikes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RantReRants_Users_UserId",
                table: "RantReRants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rants_Users_UserId",
                table: "Rants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RantBookmarks_Users_UserId",
                table: "RantBookmarks");

            migrationBuilder.DropForeignKey(
                name: "FK_RantLikes_Users_UserId",
                table: "RantLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_RantReRants_Users_UserId",
                table: "RantReRants");

            migrationBuilder.DropForeignKey(
                name: "FK_Rants_Users_UserId",
                table: "Rants");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Rants_UserId",
                table: "Rants");

            migrationBuilder.DropIndex(
                name: "IX_RantReRants_UserId",
                table: "RantReRants");

            migrationBuilder.DropIndex(
                name: "IX_RantLikes_UserId",
                table: "RantLikes");

            migrationBuilder.DropIndex(
                name: "IX_RantBookmarks_UserId",
                table: "RantBookmarks");
        }
    }
}
