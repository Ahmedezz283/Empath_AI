using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Empath_AI.Migrations
{
    /// <inheritdoc />
    public partial class chat1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_ID = table.Column<int>(type: "int", nullable: false),
                    Bot_ID = table.Column<int>(type: "int", nullable: false),
                    Device_ID = table.Column<int>(type: "int", nullable: false),
                    Conversation_ID = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sender_Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message_Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Created_At = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");
        }
    }
}
