using Microsoft.EntityFrameworkCore;
namespace PedidosAPI.Data
{
    public class PedidosDbContext : DbContext
    {
        public PedidosDbContext(DbContextOptions options) : base(options)
        {
        }

        protected PedidosDbContext() { }

        public DbSet<PedidosAPI.Entidades.Pedido> Pedidos { get; set; }
    }
}
