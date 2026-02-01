using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WPFBudgetPlanerare.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMonthOfYearTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Month",
                table: "Transactions",
                newName: "MonthOfYear");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Transactions",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MonthOfYear",
                table: "Transactions",
                newName: "Month");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Transactions",
                newName: "Date");
        }
    }
}
