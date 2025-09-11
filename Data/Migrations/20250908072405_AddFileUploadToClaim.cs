using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimsProcessingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFileUploadToClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SupportingDocumentPath",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupportingDocumentPath",
                table: "Claims");
        }
    }
}
