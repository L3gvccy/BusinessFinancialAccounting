using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessFinancialAccounting.Migrations
{
    /// <inheritdoc />
    public partial class PaymentMethodForReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Reciepts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Reciepts");
        }
    }
}
