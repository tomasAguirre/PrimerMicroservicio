using Microsoft.EntityFrameworkCore;
using ProductosAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ProductosBbContext>(options => options.UseSqlite(
    builder.Configuration.GetConnectionString("Productos")
    ));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductosBbContext>();

    if (! await db.Productos.AnyAsync()) 
    {
        db.Productos.Add(new ProductosAPI.Entidades.Producto { Nombre = "Teclado Mecanico", Precio = 10.5 });
        db.Productos.Add(new ProductosAPI.Entidades.Producto { Nombre = "Mouse Inalambrico", Precio = 20.0 });
        db.Productos.Add(new ProductosAPI.Entidades.Producto { Nombre = "Monitor 27 pulgadas", Precio = 15.75 });
        await db.SaveChangesAsync();
    }
}

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

app.UseHttpsRedirection();


app.MapGet("/productos", async (ProductosBbContext db) =>
    Results.Ok(await db.Productos.AsNoTracking().ToListAsync())
);

app.MapGet("/productos/{id:int}", async (int id, ProductosBbContext db, CancellationToken cancellationToken) =>
{
    var producto = await db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    return producto is null ? Results.NotFound() : Results.Ok(producto);
}
);

app.MapDefaultEndpoints();

app.Run();

