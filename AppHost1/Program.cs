var builder = DistributedApplication.CreateBuilder(args);

var productos = builder.AddProject<Projects.ProductosAPI>("productos");

//with reference, para indicar que pedidos depende de productos 
builder.AddProject<Projects.PedidosAPI>("pedidos").WithReference(productos);

builder.Build().Run();
