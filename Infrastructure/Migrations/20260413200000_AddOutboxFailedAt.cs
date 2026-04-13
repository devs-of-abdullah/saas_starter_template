using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxFailedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FailedAt",
                table: "OutboxEmails",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEmails_FailedAt",
                table: "OutboxEmails",
                column: "FailedAt",
                filter: "[FailedAt] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxEmails_FailedAt",
                table: "OutboxEmails");

            migrationBuilder.DropColumn(
                name: "FailedAt",
                table: "OutboxEmails");
        }
    }
}
