using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegularPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RegularPaymentId",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RegularPayment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentType = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InterestRate = table.Column<decimal>(type: "numeric", nullable: true),
                    MonthlyPayment = table.Column<decimal>(type: "numeric", nullable: true),
                    RepaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegularPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegularPayment_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_RegularPaymentId",
                table: "Transactions",
                column: "RegularPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_RegularPayment_UserId",
                table: "RegularPayment",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_RegularPayment_RegularPaymentId",
                table: "Transactions",
                column: "RegularPaymentId",
                principalTable: "RegularPayment",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_RegularPayment_RegularPaymentId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "RegularPayment");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_RegularPaymentId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RegularPaymentId",
                table: "Transactions");
        }
    }
}
