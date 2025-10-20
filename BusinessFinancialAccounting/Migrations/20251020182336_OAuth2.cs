using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessFinancialAccounting.Migrations
{
    /// <inheritdoc />
    public partial class OAuth2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OAuthId",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthProvider",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OAuthId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OAuthProvider",
                table: "Users");
        }
    }
}
