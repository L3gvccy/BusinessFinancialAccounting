using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessFinancialAccounting.Migrations
{
    /// <inheritdoc />
    public partial class FinancialOperationAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CashBalanceIncrease = table.Column<int>(type: "int", nullable: false),
                    CashBalanceDecrease = table.Column<int>(type: "int", nullable: false),
                    CardBalanceIncrease = table.Column<int>(type: "int", nullable: false),
                    CardBalanceDecrease = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialOperations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOperations_UserId",
                table: "FinancialOperations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialOperations");
        }
    }
}
