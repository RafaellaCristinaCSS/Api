using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScholaAi.Migrations
{
    /// <inheritdoc />
    public partial class DataAtividadeNomeConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Config",
                table: "Config");

            migrationBuilder.RenameTable(
                name: "Config",
                newName: "Configuracoes");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Data",
                table: "AlunoAtividadeMateria",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "Configuracoes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Configuracoes",
                table: "Configuracoes",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Configuracoes",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "Data",
                table: "AlunoAtividadeMateria");

            migrationBuilder.DropColumn(
                name: "Nome",
                table: "Configuracoes");

            migrationBuilder.RenameTable(
                name: "Configuracoes",
                newName: "Config");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Config",
                table: "Config",
                column: "Id");
        }
    }
}
