using System.Text.Json.Serialization;

namespace PedidosAPI.Entidades
{
    public class Pedido
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaCreacion { get; set; }

        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;
    }

    [JsonConverter(typeof(JsonStringEnumConverter<EstadoPedido>))]
    public enum EstadoPedido
    {
        Pendiente,
        Procesando,
        Rechazado
    }
}
