using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hms.BillingApi.Migrations
{
    /// <inheritdoc />
    public partial class FixPaymentMethodName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "InvoiceItems");

            migrationBuilder.RenameColumn(
                name: "PaymentMode",
                table: "Payments",
                newName: "PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "InvoiceItems",
                newName: "Price");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "InvoiceItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "InvoiceItems");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "Payments",
                newName: "PaymentMode");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "InvoiceItems",
                newName: "Amount");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "InvoiceItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "InvoiceItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "InvoiceItems",
                type: "datetime2",
                nullable: true);
        }
    }
}
