using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulseGuard.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMonitorHealthChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "expected_status_code",
                table: "monitors",
                type: "integer",
                nullable: false,
                defaultValue: 200);

            migrationBuilder.AddColumn<int>(
                name: "timeout_seconds",
                table: "monitors",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AlterColumn<int>(
                name: "expected_status_code",
                table: "monitors",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 200);

            migrationBuilder.AlterColumn<int>(
                name: "timeout_seconds",
                table: "monitors",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 10);

            migrationBuilder.CreateTable(
                name: "monitor_checks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    monitor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_success = table.Column<bool>(type: "boolean", nullable: false),
                    status_code = table.Column<int>(type: "integer", nullable: true),
                    response_time_ms = table.Column<long>(type: "bigint", nullable: false),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monitor_checks", x => x.id);
                    table.ForeignKey(
                        name: "FK_monitor_checks_monitors_monitor_id",
                        column: x => x.monitor_id,
                        principalTable: "monitors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_monitor_checks_monitor_id_checked_at",
                table: "monitor_checks",
                columns: new[] { "monitor_id", "checked_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "monitor_checks");

            migrationBuilder.DropColumn(
                name: "expected_status_code",
                table: "monitors");

            migrationBuilder.DropColumn(
                name: "timeout_seconds",
                table: "monitors");
        }
    }
}
