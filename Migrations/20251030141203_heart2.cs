using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Empath_AI.Migrations
{
    /// <inheritdoc />
    public partial class heart2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Device_ID",
                table: "Hearts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Device_ID",
                table: "Hearts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
