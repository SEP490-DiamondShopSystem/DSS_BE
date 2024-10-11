using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiamondShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTransactionPaymentOrderShipping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_PaymentMethod_PayMethodId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_PayMethodId",
                table: "Transaction");

            migrationBuilder.AlterColumn<string>(
                name: "PayMethodId",
                table: "Transaction",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "IsManual",
                table: "Transaction",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentType",
                table: "Order",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "ToKm",
                table: "DeliveryFee",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "FromKm",
                table: "DeliveryFee",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "FromLocation",
                table: "DeliveryFee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToLocation",
                table: "DeliveryFee",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_PayMethodId",
                table: "Transaction",
                column: "PayMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_PaymentMethod_PayMethodId",
                table: "Transaction",
                column: "PayMethodId",
                principalTable: "PaymentMethod",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_PaymentMethod_PayMethodId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_PayMethodId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "IsManual",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "FromLocation",
                table: "DeliveryFee");

            migrationBuilder.DropColumn(
                name: "ToLocation",
                table: "DeliveryFee");

            migrationBuilder.AlterColumn<string>(
                name: "PayMethodId",
                table: "Transaction",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ToKm",
                table: "DeliveryFee",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FromKm",
                table: "DeliveryFee",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_PayMethodId",
                table: "Transaction",
                column: "PayMethodId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_PaymentMethod_PayMethodId",
                table: "Transaction",
                column: "PayMethodId",
                principalTable: "PaymentMethod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
