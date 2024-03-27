using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcalibConsultBot.Migrations.ConsultPostgresDb
{
    /// <inheritdoc />
    public partial class LessonCounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LessonCount",
                table: "Lessons",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LessonCount",
                table: "Lessons");
        }
    }
}
