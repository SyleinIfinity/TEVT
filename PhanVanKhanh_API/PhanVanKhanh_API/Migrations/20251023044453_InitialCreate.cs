using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhanVanKhanh_API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbNHOM",
                columns: table => new
                {
                    MANHOM = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TENNHOM = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbNHOM", x => x.MANHOM);
                });

            migrationBuilder.CreateTable(
                name: "tbTHIETBI",
                columns: table => new
                {
                    MATHIETBI = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TENTHIETBI = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DONVITINH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SOLUONG = table.Column<int>(type: "int", nullable: true),
                    DONGIA = table.Column<decimal>(type: "numeric(18,0)", nullable: true),
                    HINHANH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MOTA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MANHOM = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbTHIETBI", x => x.MATHIETBI);
                    table.ForeignKey(
                        name: "FK_tbTHIETBI_tbNHOM_MANHOM",
                        column: x => x.MANHOM,
                        principalTable: "tbNHOM",
                        principalColumn: "MANHOM");
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbTHIETBI_MANHOM",
                table: "tbTHIETBI",
                column: "MANHOM");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbTHIETBI");

            migrationBuilder.DropTable(
                name: "tbNHOM");
        }
    }
}
