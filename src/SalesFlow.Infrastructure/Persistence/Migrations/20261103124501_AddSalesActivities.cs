using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Deals_TenantId_Id",
                table: "Deals",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.CreateTable(
                name: "SalesActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    DealId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesActivities_Customers_TenantId_CustomerId",
                        columns: x => new { x.TenantId, x.CustomerId },
                        principalTable: "Customers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesActivities_Deals_TenantId_DealId",
                        columns: x => new { x.TenantId, x.DealId },
                        principalTable: "Deals",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesActivities_TenantMembers_TenantId_CreatedByMemberId",
                        columns: x => new { x.TenantId, x.CreatedByMemberId },
                        principalTable: "TenantMembers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesActivities_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesActivities_TenantId",
                table: "SalesActivities",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesActivities_TenantId_CreatedByMemberId",
                table: "SalesActivities",
                columns: new[] { "TenantId", "CreatedByMemberId" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesActivities_TenantId_CustomerId_OccurredAtUtc",
                table: "SalesActivities",
                columns: new[] { "TenantId", "CustomerId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesActivities_TenantId_DealId",
                table: "SalesActivities",
                columns: new[] { "TenantId", "DealId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesActivities");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Deals_TenantId_Id",
                table: "Deals");
        }
    }
}
