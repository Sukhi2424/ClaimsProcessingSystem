using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimsProcessingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovedAmountToClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Claims",
                newName: "RequestedAmount");

            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedAmount",
                table: "Claims",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAmount",
                table: "Claims");

            migrationBuilder.RenameColumn(
                name: "RequestedAmount",
                table: "Claims",
                newName: "Amount");
        }
    }
}
