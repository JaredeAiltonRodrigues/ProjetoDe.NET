using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OngDoacao.Migrations
{
    /// <inheritdoc />
    public partial class HistoricoDistribuicoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Distribuicoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BeneficiarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataReserva = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HorarioRetirada = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataEntrega = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    StatusNotificacao = table.Column<string>(type: "TEXT", nullable: false),
                    DataNotificacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DetalheNotificacao = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distribuicoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Distribuicoes_Beneficiarios_BeneficiarioId",
                        column: x => x.BeneficiarioId,
                        principalTable: "Beneficiarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DistribuicaoDoacaoItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DistribuicaoDoacaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    NomeAlimento = table.Column<string>(type: "TEXT", nullable: false),
                    CodigoDeBarras = table.Column<string>(type: "TEXT", nullable: true),
                    DataVencimento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistribuicaoDoacaoItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DistribuicaoDoacaoItem_Distribuicoes_DistribuicaoDoacaoId",
                        column: x => x.DistribuicaoDoacaoId,
                        principalTable: "Distribuicoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DistribuicaoDoacaoItem_DistribuicaoDoacaoId",
                table: "DistribuicaoDoacaoItem",
                column: "DistribuicaoDoacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Distribuicoes_BeneficiarioId",
                table: "Distribuicoes",
                column: "BeneficiarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Distribuicoes_Status_HorarioRetirada",
                table: "Distribuicoes",
                columns: new[] { "Status", "HorarioRetirada" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DistribuicaoDoacaoItem");

            migrationBuilder.DropTable(
                name: "Distribuicoes");
        }
    }
}
