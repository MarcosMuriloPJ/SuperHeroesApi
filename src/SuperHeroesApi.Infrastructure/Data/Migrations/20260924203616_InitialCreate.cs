using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SuperHeroesApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Heros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    HeroName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Birthdate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Height = table.Column<double>(type: "REAL", nullable: false),
                    Weight = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Heros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Superpowers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Superpowers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HerosSuperpowers",
                columns: table => new
                {
                    HeroId = table.Column<int>(type: "INTEGER", nullable: false),
                    SuperpowerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HerosSuperpowers", x => new { x.HeroId, x.SuperpowerId });
                    table.ForeignKey(
                        name: "FK_HerosSuperpowers_Heros_HeroId",
                        column: x => x.HeroId,
                        principalTable: "Heros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HerosSuperpowers_Superpowers_SuperpowerId",
                        column: x => x.SuperpowerId,
                        principalTable: "Superpowers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Superpowers",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Força física sobre-humana", "Super Força" },
                    { 2, "Capacidade de voar", "Voo" },
                    { 3, "Capacidade de se tornar invisível", "Invisibilidade" },
                    { 4, "Capacidade de ler mentes", "Telepatia" },
                    { 5, "Capacidade de se teletransportar", "Teletransporte" },
                    { 6, "Velocidade sobre-humana", "Super Velocidade" },
                    { 7, "Capacidade de se curar rapidamente", "Regeneração" },
                    { 8, "Capacidade de controlar mentes", "Controle Mental" },
                    { 9, "Capacidade de disparar raios laser dos olhos", "Raios Laser" },
                    { 10, "Capacidade de esticar o corpo", "Elasticidade" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Heros_HeroName",
                table: "Heros",
                column: "HeroName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HerosSuperpowers_SuperpowerId",
                table: "HerosSuperpowers",
                column: "SuperpowerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HerosSuperpowers");

            migrationBuilder.DropTable(
                name: "Heros");

            migrationBuilder.DropTable(
                name: "Superpowers");
        }
    }
}
