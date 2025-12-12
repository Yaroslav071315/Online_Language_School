using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Language_School.Migrations
{
    /// <inheritdoc />
    public partial class lessonUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduledDate",
                table: "Lessons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledDate",
                table: "Lessons",
                type: "datetime(6)",
                nullable: true);
        }
    }
}
