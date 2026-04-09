using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCOLL.Data.Migrations
{
    /// <inheritdoc />
    public partial class popularBD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "active",
                table: "Produtos");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Produtos",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Encomendas",
                newName: "ClienteId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Categorias",
                newName: "Nome");

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "Produtos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "CategoriaPaiId",
                table: "Categorias",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataNascimento",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "EstadoConta",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Morada",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NIF",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "DataNascimento",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EstadoConta",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Morada",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NIF",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Nome",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "Produtos",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ClienteId",
                table: "Encomendas",
                newName: "ClientId");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "Categorias",
                newName: "Name");

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "Produtos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "CategoriaPaiId",
                table: "Categorias",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
