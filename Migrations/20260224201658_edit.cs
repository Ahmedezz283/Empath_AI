using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Empath_AI.Migrations
{
    /// <inheritdoc />
    public partial class edit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Conversation_ID",
                table: "Messages",
                column: "Conversation_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Conversations_Conversation_ID",
                table: "Messages",
                column: "Conversation_ID",
                principalTable: "Conversations",
                principalColumn: "Conversations_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Conversations_Conversation_ID",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_Conversation_ID",
                table: "Messages");

            migrationBuilder.AddColumn<int>(
                name: "Conversations_ID",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Conversations_ID",
                table: "Messages",
                column: "Conversations_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Conversations_Conversations_ID",
                table: "Messages",
                column: "Conversations_ID",
                principalTable: "Conversations",
                principalColumn: "Conversations_ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
