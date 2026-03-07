using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EnglishForKids.Infrastructure.Migrations
{
    public partial class AddPetFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "VirtualPets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Energy",
                table: "VirtualPets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Experience",
                table: "VirtualPets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFed",
                table: "VirtualPets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPlayed",
                table: "VirtualPets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PetName",
                table: "VirtualPets",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "VirtualPets");

            migrationBuilder.DropColumn(
                name: "Energy",
                table: "VirtualPets");

            migrationBuilder.DropColumn(
                name: "Experience",
                table: "VirtualPets");

            migrationBuilder.DropColumn(
                name: "LastFed",
                table: "VirtualPets");

            migrationBuilder.DropColumn(
                name: "LastPlayed",
                table: "VirtualPets");

            migrationBuilder.DropColumn(
                name: "PetName",
                table: "VirtualPets");
        }
    }
}
