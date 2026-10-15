using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Report",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ssrs_report_path = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_on = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    deleted_on = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    status_code = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Report", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ReportParameter",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    display_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_required = table.Column<bool>(type: "bit", nullable: false),
                    default_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_on = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_on = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    deleted_on = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    status_code = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportParameter", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Report_id_deleted_on",
                table: "Report",
                columns: new[] { "id", "deleted_on" },
                unique: true,
                filter: "[deleted_on] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReportParameter_id_deleted_on",
                table: "ReportParameter",
                columns: new[] { "id", "deleted_on" },
                unique: true,
                filter: "[deleted_on] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Report");

            migrationBuilder.DropTable(
                name: "ReportParameter");
        }
    }
}
