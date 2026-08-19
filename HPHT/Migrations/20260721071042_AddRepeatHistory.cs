using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HPHT.Migrations
{
    /// <inheritdoc />
    public partial class AddRepeatHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepeatHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueId = table.Column<int>(type: "int", nullable: false),
                    KAID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientCode = table.Column<int>(type: "int", nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepeatNo = table.Column<int>(type: "int", nullable: false),
                    RepeatIssueWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RepeatIssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RepeatReturnWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RepeatReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsReturned = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepeatHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepeatHistories_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepeatHistories_IssueId",
                table: "RepeatHistories",
                column: "IssueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepeatHistories");
        }
    }
}
