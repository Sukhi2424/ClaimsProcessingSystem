using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimsProcessingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDateProcessedToClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateProcessed",
                table: "Claims",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateProcessed",
                table: "Claims");
        }
    }
}
