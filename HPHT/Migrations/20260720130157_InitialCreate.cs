using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HPHT.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RepeatDate",
                table: "Issues",
                newName: "RepeatReturnDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "RepeatIssueDate",
                table: "Issues",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepeatIssueDate",
                table: "Issues");

            migrationBuilder.RenameColumn(
                name: "RepeatReturnDate",
                table: "Issues",
                newName: "RepeatDate");
        }
    }
}
