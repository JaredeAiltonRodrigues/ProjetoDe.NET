using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OngDoacao.Migrations
{
    /// <inheritdoc />
    public partial class AtualizaFinanceiroSefaz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChaveAcessoNFe",
                table: "Financeiro",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Conciliado",
                table: "Financeiro",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExigeNotaFiscal",
                table: "Financeiro",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IdTransacaoBancaria",
                table: "Financeiro",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentificacaoPagador",
                table: "Financeiro",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetodoPagamento",
                table: "Financeiro",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusSefaz",
                table: "Financeiro",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChaveAcessoNFe",
                table: "Financeiro");

            migrationBuilder.DropColumn(
                name: "Conciliado",
                table: "Financeiro");

            migrationBuilder.DropColumn(
                name: "ExigeNotaFiscal",
                table: "Financeiro");

            migrationBuilder.DropColumn(
                name: "IdTransacaoBancaria",
                table: "Financeiro");

            migrationBuilder.DropColumn(
                name: "IdentificacaoPagador",
                table: "Financeiro");

            migrationBuilder.DropColumn(
                name: "MetodoPagamento",
                table: "Financeiro");

            migrationBuilder.DropColumn(
                name: "StatusSefaz",
                table: "Financeiro");
        }
    }
}
