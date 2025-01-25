using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pytanie_Quiz_QuizId",
                table: "Pytanie");

            migrationBuilder.AlterColumn<int>(
                name: "QuizId",
                table: "Pytanie",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pytanie_Quiz_QuizId",
                table: "Pytanie",
                column: "QuizId",
                principalTable: "Quiz",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pytanie_Quiz_QuizId",
                table: "Pytanie");

            migrationBuilder.AlterColumn<int>(
                name: "QuizId",
                table: "Pytanie",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Pytanie_Quiz_QuizId",
                table: "Pytanie",
                column: "QuizId",
                principalTable: "Quiz",
                principalColumn: "Id");
        }
    }
}
