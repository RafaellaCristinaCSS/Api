using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScholaAi.Migrations
{
    /// <inheritdoc />
    public partial class NomeDaNovaMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlunoId",
                table: "AlunoAtividadeMateria",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AtividadeId",
                table: "AlunoAtividadeMateria",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MateriaId",
                table: "AlunoAtividadeMateria",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AlunoAtividadeMateria_AlunoId",
                table: "AlunoAtividadeMateria",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_AlunoAtividadeMateria_AtividadeId",
                table: "AlunoAtividadeMateria",
                column: "AtividadeId");

            migrationBuilder.CreateIndex(
                name: "IX_AlunoAtividadeMateria_MateriaId",
                table: "AlunoAtividadeMateria",
                column: "MateriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlunoAtividadeMateria_Aluno_AlunoId",
                table: "AlunoAtividadeMateria",
                column: "AlunoId",
                principalTable: "Aluno",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AlunoAtividadeMateria_Atividade_AtividadeId",
                table: "AlunoAtividadeMateria",
                column: "AtividadeId",
                principalTable: "Atividade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AlunoAtividadeMateria_Materia_MateriaId",
                table: "AlunoAtividadeMateria",
                column: "MateriaId",
                principalTable: "Materia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlunoAtividadeMateria_Aluno_AlunoId",
                table: "AlunoAtividadeMateria");

            migrationBuilder.DropForeignKey(
                name: "FK_AlunoAtividadeMateria_Atividade_AtividadeId",
                table: "AlunoAtividadeMateria");

            migrationBuilder.DropForeignKey(
                name: "FK_AlunoAtividadeMateria_Materia_MateriaId",
                table: "AlunoAtividadeMateria");

            migrationBuilder.DropIndex(
                name: "IX_AlunoAtividadeMateria_AlunoId",
                table: "AlunoAtividadeMateria");

            migrationBuilder.DropIndex(
                name: "IX_AlunoAtividadeMateria_AtividadeId",
                table: "AlunoAtividadeMateria");

            migrationBuilder.DropIndex(
                name: "IX_AlunoAtividadeMateria_MateriaId",
                table: "AlunoAtividadeMateria");

            migrationBuilder.DropColumn(
                name: "AlunoId",
                table: "AlunoAtividadeMateria");

            migrationBuilder.DropColumn(
                name: "AtividadeId",
                table: "AlunoAtividadeMateria");

            migrationBuilder.DropColumn(
                name: "MateriaId",
                table: "AlunoAtividadeMateria");
        }
    }
}
