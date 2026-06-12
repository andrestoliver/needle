using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Needle.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlbumExternalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "albums",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_albums_external_id",
                table: "albums",
                column: "external_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_albums_external_id",
                table: "albums");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "albums");
        }
    }
}
