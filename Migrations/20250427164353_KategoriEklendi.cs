using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kutuphane.Migrations
{
    /// <inheritdoc />
    public partial class KategoriEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kategoriler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KategoriAdi = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategoriler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kitap",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KitapAdi = table.Column<string>(type: "TEXT", nullable: false),
                    Yazar = table.Column<string>(type: "TEXT", nullable: false),
                    Yayinevi = table.Column<string>(type: "TEXT", nullable: false),
                    ISBN = table.Column<string>(type: "TEXT", nullable: false),
                    KategoriId = table.Column<int>(type: "INTEGER", nullable: true),
                    OduncVerildiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    EklenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kitap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kitap_Kategoriler_KategoriId",
                        column: x => x.KategoriId,
                        principalTable: "Kategoriler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kitap_KategoriId",
                table: "Kitap",
                column: "KategoriId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kitap");

            migrationBuilder.DropTable(
                name: "Kategoriler");
        }
    }
}
