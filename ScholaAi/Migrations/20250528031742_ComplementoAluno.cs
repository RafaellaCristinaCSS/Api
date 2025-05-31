using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScholaAi.Migrations
{
    /// <inheritdoc />
    public partial class ComplementoAluno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstiloAprendizagem",
                table: "Aluno",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneroLiterarioFavorito",
                table: "Aluno",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hobbies",
                table: "Aluno",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HorasEstudo",
                table: "Aluno",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InformacaoAdicional",
                table: "Aluno",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModeloEnsino",
                table: "Aluno",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstiloAprendizagem",
                table: "Aluno");

            migrationBuilder.DropColumn(
                name: "GeneroLiterarioFavorito",
                table: "Aluno");

            migrationBuilder.DropColumn(
                name: "Hobbies",
                table: "Aluno");

            migrationBuilder.DropColumn(
                name: "HorasEstudo",
                table: "Aluno");

            migrationBuilder.DropColumn(
                name: "InformacaoAdicional",
                table: "Aluno");

            migrationBuilder.DropColumn(
                name: "ModeloEnsino",
                table: "Aluno");
        }
    }
}
