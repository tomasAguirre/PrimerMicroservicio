using Microsoft.EntityFrameworkCore;

namespace ProductosAPI.Data
{
    public class ProductosBbContext : DbContext
    {
        public ProductosBbContext(DbContextOptions<ProductosBbContext> options) : base(options)
        {
        }

        protected ProductosBbContext() { }

        public DbSet<Entidades.Producto> Productos { get; set; }
    
    }
}
