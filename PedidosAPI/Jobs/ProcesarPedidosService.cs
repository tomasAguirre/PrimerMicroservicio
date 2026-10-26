
using Microsoft.EntityFrameworkCore;
using PedidosAPI.Clientes;
using PedidosAPI.Contratos;
using PedidosAPI.Data;
using PedidosAPI.Entidades;
using Polly.Timeout;

namespace PedidosAPI.Jobs
{
    public class ProcesarPedidosService(IServiceScopeFactory serviceScopeFactory,
                ILogger<ProcesarPedidosService> logger,
                ProductosClient productosClient) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcesarPendientesAsync(stoppingToken);
                }
                catch(OperationCanceledException ex) when (stoppingToken.IsCancellationRequested) 
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error al procesar pedidos pendientes");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task ProcesarPendientesAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PedidosDbContext>();
            var productosCliente = scope.ServiceProvider.GetRequiredService<ProductosClient>();

            var pendientes = await db.Pedidos.Where(p => p.Estado == Entidades.EstadoPedido.Pendiente)
                .OrderBy(p=> p.Id)
                .ToListAsync(cancellationToken);

            foreach (var pedido in pendientes) 
            {
                ProductoResponse? producto = null;

                try
                {
                    producto = await productosClient.ObtenerPorIdAsync(pedido.ProductoId, cancellationToken);
                }
                catch (HttpIOException ex)
                {
                    logger.LogError(ex, "Error al obtener el producto {ProductoId} desde ProductosAPI", pedido.ProductoId);
                    continue;
                }
                catch (TimeoutRejectedException ex)
                {
                    logger.LogError(ex, "Timeout al obtener el producto {ProductoId} desde ProductosAPI", pedido.ProductoId);
                    continue;
                }
                catch (OperationCanceledException ex)
                {

                    logger.LogError(ex, "La operación fue cancelada al obtener el producto {ProductoId} desde ProductosAPI", pedido.ProductoId);
                    continue;
                }


                if (producto is null)
                    pedido.Estado = Entidades.EstadoPedido.Rechazado;
                else
                {
                    pedido.PrecioUnitario = producto.Precio;
                    pedido.Estado = Entidades.EstadoPedido.Procesando;
                    pedido.Total = producto.Precio * pedido.Cantidad;
                }
                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Pedido {pedidoId} : {Estado} ", pedido.Id, pedido.Estado);

            }

        }

    }
}
