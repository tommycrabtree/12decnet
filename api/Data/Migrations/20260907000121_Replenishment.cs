using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Replenishment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ReplenishmentSessions");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "ReplenishmentSessions");

            migrationBuilder.RenameColumn(
                name: "PullFromFreezerDate",
                table: "DateBatches",
                newName: "ExpirationDate");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                table: "DateBatches",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "DonationUnits",
                table: "DateBatches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FiftyPercentOffUnits",
                table: "DateBatches",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "DateBatches");

            migrationBuilder.DropColumn(
                name: "DonationUnits",
                table: "DateBatches");

            migrationBuilder.DropColumn(
                name: "FiftyPercentOffUnits",
                table: "DateBatches");

            migrationBuilder.RenameColumn(
                name: "ExpirationDate",
                table: "DateBatches",
                newName: "PullFromFreezerDate");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ReplenishmentSessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "ReplenishmentSessions",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }
    }
}
