using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace socmed_backend.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserImageColumnsToMediaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfileImageUrl",
                table: "Users",
                newName: "ProfileMediaId");

            migrationBuilder.RenameColumn(
                name: "BannerImageUrl",
                table: "Users",
                newName: "BannerMediaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfileMediaId",
                table: "Users",
                newName: "ProfileImageUrl");

            migrationBuilder.RenameColumn(
                name: "BannerMediaId",
                table: "Users",
                newName: "BannerImageUrl");
        }
    }
}
