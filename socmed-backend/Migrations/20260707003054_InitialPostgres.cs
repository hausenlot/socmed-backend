using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace socmed_backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    ProfileMediaId = table.Column<string>(type: "text", nullable: true),
                    BannerMediaId = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Follows",
                columns: table => new
                {
                    FollowerId = table.Column<string>(type: "text", nullable: false),
                    FollowingId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Follows", x => new { x.FollowerId, x.FollowingId });
                    table.ForeignKey(
                        name: "FK_Follows_Users_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Follows_Users_FollowingId",
                        column: x => x.FollowingId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    RelatedEntityId = table.Column<int>(type: "integer", nullable: true),
                    SourceUsername = table.Column<string>(type: "text", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    MediaId = table.Column<string>(type: "text", nullable: true),
                    MediaType = table.Column<string>(type: "text", nullable: true),
                    QuoteRantId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rants_Rants_QuoteRantId",
                        column: x => x.QuoteRantId,
                        principalTable: "Rants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RantBookmarks",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RantId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RantBookmarks", x => new { x.RantId, x.UserId });
                    table.ForeignKey(
                        name: "FK_RantBookmarks_Rants_RantId",
                        column: x => x.RantId,
                        principalTable: "Rants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RantBookmarks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RantLikes",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RantId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RantLikes", x => new { x.RantId, x.UserId });
                    table.ForeignKey(
                        name: "FK_RantLikes_Rants_RantId",
                        column: x => x.RantId,
                        principalTable: "Rants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RantLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RantReplies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<string>(type: "text", nullable: false),
                    RantId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ParentReplyId = table.Column<int>(type: "integer", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    MediaId = table.Column<string>(type: "text", nullable: true),
                    MediaType = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RantReplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RantReplies_RantReplies_ParentReplyId",
                        column: x => x.ParentReplyId,
                        principalTable: "RantReplies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RantReplies_Rants_RantId",
                        column: x => x.RantId,
                        principalTable: "Rants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RantReplies_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RantReRants",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RantId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RantReRants", x => new { x.RantId, x.UserId });
                    table.ForeignKey(
                        name: "FK_RantReRants_Rants_RantId",
                        column: x => x.RantId,
                        principalTable: "Rants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RantReRants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReplyLikes",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ReplyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplyLikes", x => new { x.ReplyId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ReplyLikes_RantReplies_ReplyId",
                        column: x => x.ReplyId,
                        principalTable: "RantReplies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReplyLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowingId",
                table: "Follows",
                column: "FollowingId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RantBookmarks_UserId",
                table: "RantBookmarks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RantLikes_UserId",
                table: "RantLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RantReplies_ParentReplyId",
                table: "RantReplies",
                column: "ParentReplyId");

            migrationBuilder.CreateIndex(
                name: "IX_RantReplies_PublicId",
                table: "RantReplies",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RantReplies_RantId",
                table: "RantReplies",
                column: "RantId");

            migrationBuilder.CreateIndex(
                name: "IX_RantReplies_UserId",
                table: "RantReplies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RantReRants_UserId",
                table: "RantReRants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rants_PublicId",
                table: "Rants",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rants_QuoteRantId",
                table: "Rants",
                column: "QuoteRantId");

            migrationBuilder.CreateIndex(
                name: "IX_Rants_UserId",
                table: "Rants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplyLikes_UserId",
                table: "ReplyLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Follows");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "RantBookmarks");

            migrationBuilder.DropTable(
                name: "RantLikes");

            migrationBuilder.DropTable(
                name: "RantReRants");

            migrationBuilder.DropTable(
                name: "ReplyLikes");

            migrationBuilder.DropTable(
                name: "RantReplies");

            migrationBuilder.DropTable(
                name: "Rants");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
