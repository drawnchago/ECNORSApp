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
        Task<TransactionResp<bool>> UpdateTransactionBySequenceAsync(TransactionUpdateDto dto,CancellationToken ct = default);
        Task<TransactionResp<bool>> CloseForcedAsync(string station, int idSec, CancellationToken ct = default);

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

    public async Task CloseManualAsync(string station,int secuenciaBuscar,decimal volumenGross,decimal volumenNetoCt,decimal temperatura,CancellationToken ct = default)
    {
        var url = "api/binnacle/close-manual";

        var body = new
        {
            station,
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

    public async Task<TransactionResp<bool>> UpdateTransactionBySequenceAsync(TransactionUpdateDto dto,CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/Transaction/UpdateTransactionBySequence", dto,ct);

        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content.ReadAsStringAsync(ct);
            return TransactionResp<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {raw}");
        }

        var result = await response.Content.ReadFromJsonAsync<TransactionResp<bool>>(cancellationToken: ct);
        return result ?? TransactionResp<bool>.Fail("Respuesta vacía del servidor.");
    }
    public async Task<TransactionResp<bool>> CloseForcedAsync(string station, int idSec, CancellationToken ct = default)
    {
        var url = "api/transaction/closeForced";

        var body = new
        {
            station,
            idSec
        };

        var response = await _http.PostAsJsonAsync(url, body, ct);

        // Tu endpoint regresa BadRequest con { Success=false, Message=... }
        // y Ok con { Success=true, Message=... }
        // Entonces NO usamos EnsureSuccessStatusCode, mejor leemos el body.

        if (!response.IsSuccessStatusCode)
        {
            // Intentamos leer respuesta como el mismo contrato
            try
            {
                var err = await response.Content.ReadFromJsonAsync<DbInfoResp<object>>(cancellationToken: ct);
                var msg = err?.Message ?? $"Error HTTP {(int)response.StatusCode}";
                return TransactionResp<bool>.Fail(msg);
            }
            catch
            {
                var raw = await response.Content.ReadAsStringAsync(ct);
                return TransactionResp<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {raw}");
            }
        }

        // Si tu endpoint devuelve TransactionResp<bool> directo, descomenta esto:
        // var result = await response.Content.ReadFromJsonAsync<TransactionResp<bool>>(cancellationToken: ct);
        // return result ?? TransactionResp<bool>.Fail("Respuesta vacía del servidor.");

        // Como tu endpoint devuelve { Success, Message } (anónimo), lo mapeamos:
        var ok = await response.Content.ReadFromJsonAsync<DbInfoResp<object>>(cancellationToken: ct);

        if (ok is null)
            return TransactionResp<bool>.Fail("Respuesta vacía del servidor.");

        if (!ok.Success)
            return TransactionResp<bool>.Fail(ok.Message ?? "Error al ejecutar el cierre forzado.");

        return TransactionResp<bool>.Ok(true, ok.Message ?? "Cierre forzado ejecutado correctamente.");
    }

    public sealed class DbInfoResp<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
