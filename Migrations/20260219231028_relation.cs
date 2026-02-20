using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Empath_AI.Migrations
{
    /// <inheritdoc />
    public partial class relation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Conversations_ID",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Conversations_ID",
                table: "Messages",
                column: "Conversations_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Conversations_Conversations_ID",
                table: "Messages",
                column: "Conversations_ID",
                principalTable: "Conversations",
                principalColumn: "Conversations_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Conversations_Conversations_ID",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_Conversations_ID",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Conversations_ID",
                table: "Messages");
        }
    }
}
