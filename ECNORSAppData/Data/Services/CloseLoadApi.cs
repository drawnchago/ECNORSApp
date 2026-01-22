using ECNORSAppData.Data.DTO;
using Microsoft.Data.SqlClient;
using System.Net.Http.Json;
using static ECNORSApp.Services.CloseLoadApi;

namespace ECNORSApp.Services;

public sealed class CloseLoadApi : ICloseLoadApi
{
    private readonly HttpClient _http;

    public interface ICloseLoadApi
    {
        Task<DbInfoResp<string>?> GetDbInfoAsync(string station, CancellationToken ct = default);
        Task<IReadOnlyList<DispensaryDto>> GetDispensariosAsync(string station, CancellationToken ct = default);
        Task<IReadOnlyList<BinnacleDto>> GetBinnacleTopAsync(string station,int dispensaryId,CancellationToken ct = default);
        Task<IReadOnlyList<TransactionDto>> GetTransactionsTopAsync(string station, int dispensaryId, CancellationToken ct = default);
        Task<TransactionDto?> GetTransactionBySequenceAsync(string station, long secuencia, CancellationToken ct = default);
        Task CloseManualAsync(string station,int secuenciaBuscar,decimal volumenGross,decimal volumenNetoCt,decimal temperatura,CancellationToken ct = default);
        Task<decimal> GetNetVolAutoAsync(string station,int intDispensario,int intProducto,decimal temperatura,decimal volumenGross,CancellationToken ct = default);
    }
    public CloseLoadApi(HttpClient http) => _http = http;

    public async Task<DbInfoResp<string>?> GetDbInfoAsync(string station,CancellationToken ct = default)
    {
        var url = $"api/station/db-info?station={Uri.EscapeDataString(station)}";
        var res = await _http.GetFromJsonAsync<DbInfoResp<string>>(url, ct);
        return res;
    }
    public async Task<IReadOnlyList<DispensaryDto>> GetDispensariosAsync(string station, CancellationToken ct = default)
    {
        var url = $"api/dispensary?station={Uri.EscapeDataString(station)}";

        using var resp = await _http.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();

        var wrapper = await resp.Content.ReadFromJsonAsync<DbInfoResp<List<DispensaryDto>>>(cancellationToken: ct);

        return (IReadOnlyList<DispensaryDto>?)wrapper?.Data
               ?? Array.Empty<DispensaryDto>();
    }
    public async Task<IReadOnlyList<BinnacleDto>> GetBinnacleTopAsync(string station,int dispensaryId,CancellationToken ct = default)
    {
        var url = $"api/binnacle/top?dispensaryId={dispensaryId}&station={Uri.EscapeDataString(station)}";

        using var resp = await _http.GetAsync(url, ct);

        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound || resp.StatusCode == System.Net.HttpStatusCode.NoContent)
            return Array.Empty<BinnacleDto>();

        resp.EnsureSuccessStatusCode();

        var payload = await resp.Content.ReadFromJsonAsync<DbInfoResp<List<BinnacleDto>>>(cancellationToken: ct);

        if (payload is null || !payload.Success || payload.Data is null)
            return Array.Empty<BinnacleDto>();

        return payload.Data;
    }

    public async Task<IReadOnlyList<TransactionDto>> GetTransactionsTopAsync(string station,int dispensaryId,CancellationToken ct = default)
    {
        var url = $"api/transaction/top?dispensaryId={dispensaryId}&station={Uri.EscapeDataString(station)}";

        using var resp = await _http.GetAsync(url, ct);

        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            return Array.Empty<TransactionDto>();

        resp.EnsureSuccessStatusCode();

        var payload = await resp.Content.ReadFromJsonAsync<DbInfoResp<List<TransactionDto>>>(cancellationToken: ct);

        if (payload is null || !payload.Success || payload.Data is null)
            return Array.Empty<TransactionDto>();

        return payload.Data;
    }

    public Task<TransactionDto?> GetTransactionBySequenceAsync(string station, long secuencia, CancellationToken ct = default)
        => _http.GetFromJsonAsync<TransactionDto>(
            $"api/transaction/by-sequence/{secuencia}?station={Uri.EscapeDataString(station)}", ct);

    public async Task CloseManualAsync(string station, int secuenciaBuscar, decimal volumenGross, decimal volumenNetoCt, decimal temperatura, CancellationToken ct = default)
    {
        var url = $"api/binnacle/close-manual?station={Uri.EscapeDataString(station)}";

        var body = new
        {
            secuenciaBuscar,
            volumenGross,
            volumenNetoCt,
            temperatura
        };

        var resp = await _http.PostAsJsonAsync(url, body, ct);
        resp.EnsureSuccessStatusCode();
    }
    public async Task<decimal> GetNetVolAutoAsync(string station,int intDispensario,int intProducto,decimal temperatura,decimal volumenGross,CancellationToken ct = default)
        => (await (await _http.PostAsJsonAsync(
                "api/binnacle/GetNetVolAuto",
                new { station, intDispensario, intProducto, temperatura, volumenGross },
                ct))
            .Content
            .ReadFromJsonAsync<DbInfoResp<decimal?>>(cancellationToken: ct))
           ?.Data ?? 0m;


    public sealed class DbInfoResp<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
