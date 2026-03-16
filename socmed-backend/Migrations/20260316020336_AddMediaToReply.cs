using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace socmed_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaToReply : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MediaId",
                table: "RantReplies",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                table: "RantReplies",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaId",
                table: "RantReplies");

            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "RantReplies");
        }
    }
}
