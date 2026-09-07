using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDateBatchAndReplenishmentSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReplenishmentSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplenishmentSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DateBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReplenishmentSessionId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    RequestUnits = table.Column<int>(type: "integer", nullable: false),
                    DateUnits = table.Column<int>(type: "integer", nullable: false),
                    PullFromFreezerDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DateBatches_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DateBatches_ReplenishmentSessions_ReplenishmentSessionId",
                        column: x => x.ReplenishmentSessionId,
                        principalTable: "ReplenishmentSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DateBatches_ProductId",
                table: "DateBatches",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DateBatches_ReplenishmentSessionId",
                table: "DateBatches",
                column: "ReplenishmentSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DateBatches");

            migrationBuilder.DropTable(
                name: "ReplenishmentSessions");
        }
    }
}
