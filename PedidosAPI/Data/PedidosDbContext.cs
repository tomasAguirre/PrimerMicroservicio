using Microsoft.EntityFrameworkCore;
namespace PedidosAPI.Data
{
    public class PedidosDbContext : DbContext
    {
        public PedidosDbContext(DbContextOptions options) : base(options)
        {
        }

        protected PedidosDbContext() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PedidosAPI.Entidades.Pedido>()
                .Property(p => p.Estado)
                .HasConversion<String>();
        }

        public DbSet<PedidosAPI.Entidades.Pedido> Pedidos { get; set; }
    }
}
