using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScholaAi.Migrations
{
    /// <inheritdoc />
    public partial class CriarQuestoesEAlternativas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Questao",
                newName: "Texto");

            migrationBuilder.RenameColumn(
                name: "IdAtividade",
                table: "Questao",
                newName: "AtividadeId");

            migrationBuilder.AddColumn<int>(
                name: "IdAluno",
                table: "Atividade",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdTipoAtividade",
                table: "Atividade",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Alternativa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Texto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Correta = table.Column<bool>(type: "bit", nullable: false),
                    QuestaoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alternativa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alternativa_Questao_QuestaoId",
                        column: x => x.QuestaoId,
                        principalTable: "Questao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Questao_AtividadeId",
                table: "Questao",
                column: "AtividadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Alternativa_QuestaoId",
                table: "Alternativa",
                column: "QuestaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questao_Atividade_AtividadeId",
                table: "Questao",
                column: "AtividadeId",
                principalTable: "Atividade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questao_Atividade_AtividadeId",
                table: "Questao");

            migrationBuilder.DropTable(
                name: "Alternativa");

            migrationBuilder.DropIndex(
                name: "IX_Questao_AtividadeId",
                table: "Questao");

            migrationBuilder.DropColumn(
                name: "IdAluno",
                table: "Atividade");

            migrationBuilder.DropColumn(
                name: "IdTipoAtividade",
                table: "Atividade");

            migrationBuilder.RenameColumn(
                name: "Texto",
                table: "Questao",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "AtividadeId",
                table: "Questao",
                newName: "IdAtividade");
        }
    }
}
