using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kutuphane.Migrations
{
    /// <inheritdoc />
    public partial class OduncKitapModelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ogrenciler_Siniflar_SinifId",
                table: "Ogrenciler");

            migrationBuilder.AlterColumn<int>(
                name: "SinifId",
                table: "Ogrenciler",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Ogrenciler_Siniflar_SinifId",
                table: "Ogrenciler",
                column: "SinifId",
                principalTable: "Siniflar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ogrenciler_Siniflar_SinifId",
                table: "Ogrenciler");

            migrationBuilder.AlterColumn<int>(
                name: "SinifId",
                table: "Ogrenciler",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Ogrenciler_Siniflar_SinifId",
                table: "Ogrenciler",
                column: "SinifId",
                principalTable: "Siniflar",
                principalColumn: "Id");
        }
    }
}
