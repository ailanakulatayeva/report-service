using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Updated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReportId",
                table: "ReportParameter",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ReportParameter_ReportId",
                table: "ReportParameter",
                column: "ReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportParameter_Report_ReportId",
                table: "ReportParameter",
                column: "ReportId",
                principalTable: "Report",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportParameter_Report_ReportId",
                table: "ReportParameter");

            migrationBuilder.DropIndex(
                name: "IX_ReportParameter_ReportId",
                table: "ReportParameter");

            migrationBuilder.DropColumn(
                name: "ReportId",
                table: "ReportParameter");
        }
    }
}
