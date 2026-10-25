using PedidosAPI.Contratos;
using System.Net;
using System.Runtime.InteropServices;

namespace PedidosAPI.Clientes
{
    public class ProductosClient(HttpClient httpClient)
    {
        public async Task<ProductoResponse?> ObtenerPorIdAsync(int ProductoId, CancellationToken cancellationToken = default)
        {
            using var response = await httpClient.GetAsync(
                $"/productos/{ProductoId}", cancellationToken
                );

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ProductoResponse>(cancellationToken);
        }
    }
}
