using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatbotBlazorApp.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contents",
                columns: table => new
                {
                    ContentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contents", x => x.ContentID);
                });

            migrationBuilder.CreateTable(
                name: "SplitContents",
                columns: table => new
                {
                    SplitContentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SplitContent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SplitContents", x => x.SplitContentID);
                    table.ForeignKey(
                        name: "FK_SplitContents_Contents_ContentID",
                        column: x => x.ContentID,
                        principalTable: "Contents",
                        principalColumn: "ContentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Keywords",
                columns: table => new
                {
                    KeywordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Keyword = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KeywordNotToneMarks = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SplitContentID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keywords", x => x.KeywordID);
                    table.ForeignKey(
                        name: "FK_Keywords_SplitContents_SplitContentID",
                        column: x => x.SplitContentID,
                        principalTable: "SplitContents",
                        principalColumn: "SplitContentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Keywords_Keyword_KeywordNotToneMarks",
                table: "Keywords",
                columns: new[] { "Keyword", "KeywordNotToneMarks" });

            migrationBuilder.CreateIndex(
                name: "IX_Keywords_SplitContentID",
                table: "Keywords",
                column: "SplitContentID");

            migrationBuilder.CreateIndex(
                name: "IX_SplitContents_ContentID",
                table: "SplitContents",
                column: "ContentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Keywords");

            migrationBuilder.DropTable(
                name: "SplitContents");

            migrationBuilder.DropTable(
                name: "Contents");
        }
    }
}
