using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Language_School.Migrations
{
    /// <inheritdoc />
    public partial class CourseChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_AspNetUsers_AdministratorId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_AdministratorId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "AdministratorId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Courses");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "News",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Courses");

            migrationBuilder.UpdateData(
                table: "News",
                keyColumn: "ImageUrl",
                keyValue: null,
                column: "ImageUrl",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "News",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AdministratorId",
                table: "Courses",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Courses",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_AdministratorId",
                table: "Courses",
                column: "AdministratorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_AspNetUsers_AdministratorId",
                table: "Courses",
                column: "AdministratorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
