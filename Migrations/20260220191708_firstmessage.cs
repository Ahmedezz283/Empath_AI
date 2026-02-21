using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Empath_AI.Migrations
{
    /// <inheritdoc />
    public partial class firstmessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstMessage",
                table: "Conversations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstMessage",
                table: "Conversations");
        }
    }
}
