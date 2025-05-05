using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kutuphane.Migrations
{
    /// <inheritdoc />
    public partial class KitapEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kitap_Kategoriler_KategoriId",
                table: "Kitap");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Kitap",
                table: "Kitap");

            migrationBuilder.RenameTable(
                name: "Kitap",
                newName: "Kitaplar");

            migrationBuilder.RenameIndex(
                name: "IX_Kitap_KategoriId",
                table: "Kitaplar",
                newName: "IX_Kitaplar_KategoriId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Kitaplar",
                table: "Kitaplar",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Kitaplar_Kategoriler_KategoriId",
                table: "Kitaplar",
                column: "KategoriId",
                principalTable: "Kategoriler",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kitaplar_Kategoriler_KategoriId",
                table: "Kitaplar");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Kitaplar",
                table: "Kitaplar");

            migrationBuilder.RenameTable(
                name: "Kitaplar",
                newName: "Kitap");

            migrationBuilder.RenameIndex(
                name: "IX_Kitaplar_KategoriId",
                table: "Kitap",
                newName: "IX_Kitap_KategoriId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Kitap",
                table: "Kitap",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Kitap_Kategoriler_KategoriId",
                table: "Kitap",
                column: "KategoriId",
                principalTable: "Kategoriler",
                principalColumn: "Id");
        }
    }
}
