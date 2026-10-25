using Microsoft.EntityFrameworkCore;
using PedidosAPI.Clientes;
using PedidosAPI.Contratos;
using PedidosAPI.Data;
using PedidosAPI.Entidades;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<PedidosDbContext>(option =>
    option.UseSqlite(builder.Configuration.GetConnectionString("Pedidos"))  
                );

builder.Services.AddHttpClient<ProductosClient>(cliente =>
{
    var baseUrl = builder.Configuration["Productos:BaseUrl"]
        ?? throw new InvalidOperationException("No se ha configurado la URL base de ProductosAPI");

    cliente.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/pedidos", async (PedidosDbContext db) =>
    Results.Ok(await db.Pedidos.AsNoTracking().ToListAsync())
);

app.MapGet("/pedidos/{id:int}", async (
    int id, PedidosDbContext db, CancellationToken cancellationToken) =>
{ 
    var pedido = await db.Pedidos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    return pedido is null ? Results.NotFound() : Results.Ok(pedido);
});

app.MapPost("/pedidos", async (
       CrearPedidoRequest request,
       ProductosClient productosClient,
       PedidosDbContext db,
       CancellationToken cancellationToken) =>
       {
           if (request.ProductoId == 0 || request.Cantidad == 0)
           {
               return Results.BadRequest("Producto y Cantidad deben de ser validos");
           }

           var producto = await productosClient.ObtenerPorIdAsync(request.ProductoId, cancellationToken);

           if (producto is null)
               return Results.BadRequest("El producto indicado No existe");

           var pedido = new Pedido
           {
               ProductoId = request.ProductoId,
               Cantidad = request.Cantidad,
               PrecioUnitario = producto.Precio,
               Total = producto.Precio * request.Cantidad,
               FechaCreacion = DateTime.UtcNow
           };
           db.Pedidos.Add(pedido);
           await db.SaveChangesAsync(cancellationToken);

           return Results.Created($"/pedidos/{pedido.Id}", pedido);
       }
   );

app.Run();


