using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrongFitApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSeriesRepeticoesToExercicios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Verificar se a coluna Series já existe e adicioná-la se não existir
            migrationBuilder.Sql(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Exercicios]') AND name = 'Series')
        BEGIN
            ALTER TABLE [dbo].[Exercicios]
            ADD [Series] INT NOT NULL DEFAULT 3
        END
    ");

            // Verificar se a coluna Repeticoes já existe e adicioná-la se não existir
            migrationBuilder.Sql(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Exercicios]') AND name = 'Repeticoes')
        BEGIN
            ALTER TABLE [dbo].[Exercicios]
            ADD [Repeticoes] INT NOT NULL DEFAULT 12
        END
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "TreinoExercicio");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Exercicios");

            migrationBuilder.DropTable(
                name: "Treinos");

            migrationBuilder.DropTable(
                name: "Alunos");

            migrationBuilder.DropTable(
                name: "Personals");
        }
    }
}
