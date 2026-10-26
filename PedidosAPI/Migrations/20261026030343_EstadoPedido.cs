using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PedidosAPI.Migrations
{
    /// <inheritdoc />
    public partial class EstadoPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "Pedidos",
                type: "INTEGER",
                nullable: false,
                defaultValue: "Procesado"); //se agrega procesado para que los registors que ya existen
                                            //en la base de datos se les coloque el valor de procesado
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Pedidos");
        }
    }
}
