using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using PedidosAPI.Clientes;
using PedidosAPI.Contratos;
using PedidosAPI.Data;
using PedidosAPI.Entidades;
using Polly.Timeout;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(); //service defaults de aspire

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
}).AddStandardResilienceHandler(option =>   //Resilencia 
{
    option.Retry.MaxRetryAttempts = 1;
    option.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
    option.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
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
       ILogger<Program> logger,
       CancellationToken cancellationToken) =>
       {
           if (request.ProductoId == 0 || request.Cantidad == 0)
           {
               return Results.BadRequest("Producto y Cantidad deben de ser validos");
           }

           ProductoResponse? producto;

           try
           {
               producto = await productosClient.ObtenerPorIdAsync(request.ProductoId, cancellationToken);
           }
           catch (HttpIOException ex)
           {
               logger.LogError(ex, "Error al obtener el producto {ProductoId} desde ProductosAPI", request.ProductoId);
               return Results.Problem(
                        title: "Servicio de productos no disponible",
                        statusCode: StatusCodes.Status503ServiceUnavailable
                   );
           }
           catch (TimeoutRejectedException ex)
           {
               logger.LogError(ex, "Timeout al obtener el producto {ProductoId} desde ProductosAPI", request.ProductoId);
               return Results.Problem(
                           title: "Servicio de productos no disponible",
                           detail: "La consulta Tardo demasiado",
                           statusCode: StatusCodes.Status503ServiceUnavailable
                    );
           }
           catch (OperationCanceledException ex)
           {

               logger.LogError(ex, "La operación fue cancelada al obtener el producto {ProductoId} desde ProductosAPI", request.ProductoId);
               return Results.Problem(
                           title: "Servicio de productos no disponible",
                           detail: "La operación fue cancelada",
                           statusCode: StatusCodes.Status503ServiceUnavailable
                     );
           }


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

           logger.LogInformation("Pedido {PedidoId} Creado ", pedido.Id);

           return Results.Created($"/pedidos/{pedido.Id}", pedido);
       }
   );

app.MapDefaultEndpoints();

app.Run();


