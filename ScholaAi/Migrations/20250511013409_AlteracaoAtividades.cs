using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScholaAi.Migrations
{
    /// <inheritdoc />
    public partial class AlteracaoAtividades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "TipoAtividade",
                newName: "Nome");

            migrationBuilder.AddColumn<float>(
                name: "Pontuacao",
                table: "Questao",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "IdProfessor",
                table: "Atividade",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pontuacao",
                table: "Questao");

            migrationBuilder.DropColumn(
                name: "IdProfessor",
                table: "Atividade");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "TipoAtividade",
                newName: "Name");
        }
    }
}
