using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barber.DataAccess.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class ShopHoursAndDayOffs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SlotMinutes",
                table: "Services",
                type: "integer",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.CreateTable(
                name: "WorkingDayOverrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BarbershopId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkingDayOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkingDayOverrides_Barbershops_BarbershopId",
                        column: x => x.BarbershopId,
                        principalTable: "Barbershops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkingDayOverrides_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkingHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BarbershopId = table.Column<Guid>(type: "uuid", nullable: false),
                    Day = table.Column<int>(type: "integer", nullable: false),
                    Open = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Close = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkingHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkingHours_Barbershops_BarbershopId",
                        column: x => x.BarbershopId,
                        principalTable: "Barbershops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkingDayOverrides_BarbershopId_Date",
                table: "WorkingDayOverrides",
                columns: new[] { "BarbershopId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkingDayOverrides_StaffId_Date",
                table: "WorkingDayOverrides",
                columns: new[] { "StaffId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkingHours_BarbershopId_Day",
                table: "WorkingHours",
                columns: new[] { "BarbershopId", "Day" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkingDayOverrides");

            migrationBuilder.DropTable(
                name: "WorkingHours");

            migrationBuilder.DropColumn(
                name: "SlotMinutes",
                table: "Services");
        }
    }
}
