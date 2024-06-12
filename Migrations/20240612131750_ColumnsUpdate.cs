using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnetrpg.Migrations
{
    /// <inheritdoc />
    public partial class ColumnsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Intelligent",
                table: "Characters",
                newName: "Intelligence");

            migrationBuilder.RenameColumn(
                name: "HitPoints",
                table: "Characters",
                newName: "HP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Intelligence",
                table: "Characters",
                newName: "Intelligent");

            migrationBuilder.RenameColumn(
                name: "HP",
                table: "Characters",
                newName: "HitPoints");
        }
    }
}
