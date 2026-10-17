using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OngDoacao.Migrations
{
    /// <inheritdoc />
    public partial class AddLoginEquipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PerfilAcesso",
                table: "Equipe",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Senha",
                table: "Equipe",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PerfilAcesso",
                table: "Equipe");

            migrationBuilder.DropColumn(
                name: "Senha",
                table: "Equipe");
        }
    }
}
