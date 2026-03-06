using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Empath_AI.Migrations
{
    /// <inheritdoc />
    public partial class fixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hearts_Devices_DeviceId",
                table: "Hearts");

            migrationBuilder.AddColumn<int>(
                name: "DeviceId",
                table: "GSRRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeviceId",
                table: "Accelerometer",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hearts_UserId",
                table: "Hearts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GSRRecords_DeviceId",
                table: "GSRRecords",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_GSRRecords_UserId",
                table: "GSRRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accelerometer_DeviceId",
                table: "Accelerometer",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Accelerometer_UserId",
                table: "Accelerometer",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accelerometer_Devices_DeviceId",
                table: "Accelerometer",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Accelerometer_Users_UserId",
                table: "Accelerometer",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GSRRecords_Devices_DeviceId",
                table: "GSRRecords",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GSRRecords_Users_UserId",
                table: "GSRRecords",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Hearts_Devices_DeviceId",
                table: "Hearts",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Hearts_Users_UserId",
                table: "Hearts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accelerometer_Devices_DeviceId",
                table: "Accelerometer");

            migrationBuilder.DropForeignKey(
                name: "FK_Accelerometer_Users_UserId",
                table: "Accelerometer");

            migrationBuilder.DropForeignKey(
                name: "FK_GSRRecords_Devices_DeviceId",
                table: "GSRRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_GSRRecords_Users_UserId",
                table: "GSRRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Hearts_Devices_DeviceId",
                table: "Hearts");

            migrationBuilder.DropForeignKey(
                name: "FK_Hearts_Users_UserId",
                table: "Hearts");

            migrationBuilder.DropIndex(
                name: "IX_Hearts_UserId",
                table: "Hearts");

            migrationBuilder.DropIndex(
                name: "IX_GSRRecords_DeviceId",
                table: "GSRRecords");

            migrationBuilder.DropIndex(
                name: "IX_GSRRecords_UserId",
                table: "GSRRecords");

            migrationBuilder.DropIndex(
                name: "IX_Accelerometer_DeviceId",
                table: "Accelerometer");

            migrationBuilder.DropIndex(
                name: "IX_Accelerometer_UserId",
                table: "Accelerometer");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "GSRRecords");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Accelerometer");

            migrationBuilder.AddForeignKey(
                name: "FK_Hearts_Devices_DeviceId",
                table: "Hearts",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id");
        }
    }
}
