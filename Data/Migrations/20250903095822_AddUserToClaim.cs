using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimsProcessingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubmittingUserId",
                table: "Claims",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Claims_SubmittingUserId",
                table: "Claims",
                column: "SubmittingUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_AspNetUsers_SubmittingUserId",
                table: "Claims",
                column: "SubmittingUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_AspNetUsers_SubmittingUserId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_SubmittingUserId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "SubmittingUserId",
                table: "Claims");
        }
    }
}
