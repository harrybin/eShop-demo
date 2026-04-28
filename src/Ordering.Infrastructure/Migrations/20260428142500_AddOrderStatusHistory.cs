using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using eShop.Ordering.Infrastructure;

#nullable disable

namespace Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(OrderingContext))]
    [Migration("20260428142500_AddOrderStatusHistory")]
    public partial class AddOrderStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create sequence for OrderStatusHistory IDs
            migrationBuilder.CreateSequence<int>(
                name: "orderstatushistoryseq",
                schema: "ordering");

            // Create the order_status_history table
            migrationBuilder.CreateTable(
                name: "order_status_history",
                schema: "ordering",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status_changed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_status_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_status_history_order",
                        column: x => x.order_id,
                        principalSchema: "ordering",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create index for efficient querying
            migrationBuilder.CreateIndex(
                name: "ix_order_status_history_order_date",
                schema: "ordering",
                table: "order_status_history",
                columns: new[] { "order_id", "status_changed_on_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the table and sequence
            migrationBuilder.DropTable(
                name: "order_status_history",
                schema: "ordering");

            migrationBuilder.DropSequence(
                name: "orderstatushistoryseq",
                schema: "ordering");
        }
    }
}
