using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace enquetix.Migrations
{
    /// <inheritdoc />
    public partial class CamposNaoObrigatorios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PollVotes_PollId",
                table: "PollVotes");

            migrationBuilder.CreateIndex(
                name: "IX_PollVotes_PollId_UserId",
                table: "PollVotes",
                columns: new[] { "PollId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PollVotes_PollId_UserId",
                table: "PollVotes");

            migrationBuilder.CreateIndex(
                name: "IX_PollVotes_PollId",
                table: "PollVotes",
                column: "PollId");
        }
    }
}
