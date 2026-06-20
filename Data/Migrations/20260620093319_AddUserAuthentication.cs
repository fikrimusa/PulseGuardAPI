using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulseGuard.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var legacyUserId = new Guid("00000000-0000-0000-0000-000000000001");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "email", "password_hash", "created_at_utc" },
                values: new object[]
                {
                    legacyUserId,
                    "legacy@pulseguard.invalid",
                    "LegacyMonitorOwnerNotForLogin",
                    new DateTime(2026, 6, 20, 0, 0, 0, DateTimeKind.Utc)
                });

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "monitors",
                type: "uuid",
                nullable: false,
                defaultValue: legacyUserId);

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "monitors",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: legacyUserId);

            migrationBuilder.CreateIndex(
                name: "IX_monitors_user_id",
                table: "monitors",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_monitors_users_user_id",
                table: "monitors",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_monitors_users_user_id",
                table: "monitors");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropIndex(
                name: "IX_monitors_user_id",
                table: "monitors");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "monitors");
        }
    }
}
