using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Barber.DataAccess.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class UserProfileAndStaffSeniority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Clients_ClientId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Staff_BarbershopId",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Staff");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Bookings",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_ClientId",
                table: "Bookings",
                newName: "IX_Bookings_UserId");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Staff",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SeniorityId",
                table: "Staff",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Seniorities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seniorities", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Seniorities",
                columns: new[] { "Id", "Level", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 0, "Junior" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 1, "Barber" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 2, "Senior" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), 3, "Master" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Staff_BarbershopId_UserId",
                table: "Staff",
                columns: new[] { "BarbershopId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_SeniorityId",
                table: "Staff",
                column: "SeniorityId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_UserId",
                table: "Staff",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Seniorities_Name",
                table: "Seniorities",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_UserId",
                table: "Bookings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_Seniorities_SeniorityId",
                table: "Staff",
                column: "SeniorityId",
                principalTable: "Seniorities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_Users_UserId",
                table: "Staff",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_UserId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Seniorities_SeniorityId",
                table: "Staff");

            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Users_UserId",
                table: "Staff");

            migrationBuilder.DropTable(
                name: "Seniorities");

            migrationBuilder.DropIndex(
                name: "IX_Staff_BarbershopId_UserId",
                table: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_Staff_SeniorityId",
                table: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_Staff_UserId",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SeniorityId",
                table: "Staff");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Bookings",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                newName: "IX_Bookings_ClientId");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Staff",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Staff",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Staff",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Staff_BarbershopId",
                table: "Staff",
                column: "BarbershopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Clients_ClientId",
                table: "Bookings",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
