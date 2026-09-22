using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barber.DataAccess.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class ShopTaglineAndDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Barbershops",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tagline",
                table: "Barbershops",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Barbershops");

            migrationBuilder.DropColumn(
                name: "Tagline",
                table: "Barbershops");
        }
    }
}
