using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Empath_AI.Migrations
{
    /// <inheritdoc />
    public partial class test_messages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Conversations_Conversations_ID",
                table: "Messages");

            migrationBuilder.AlterColumn<int>(
                name: "Conversations_ID",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Conversations_Conversations_ID",
                table: "Messages",
                column: "Conversations_ID",
                principalTable: "Conversations",
                principalColumn: "Conversations_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Conversations_Conversations_ID",
                table: "Messages");

            migrationBuilder.AlterColumn<int>(
                name: "Conversations_ID",
                table: "Messages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Conversations_Conversations_ID",
                table: "Messages",
                column: "Conversations_ID",
                principalTable: "Conversations",
                principalColumn: "Conversations_ID");
        }
    }
}
