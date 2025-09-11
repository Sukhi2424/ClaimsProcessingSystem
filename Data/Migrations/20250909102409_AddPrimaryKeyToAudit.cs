using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimsProcessingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPrimaryKeyToAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrimaryKey",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryKey",
                table: "AuditLogs");
        }
    }
}
