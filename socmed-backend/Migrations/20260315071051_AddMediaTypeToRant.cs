using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace socmed_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaTypeToRant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                table: "Rants",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "Rants");
        }
    }
}
