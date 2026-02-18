using ECNORSApp.Data.DTO;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ECNORSApp.Services;

public sealed class HandbookApi
{
    private readonly HttpClient _http;

    public HandbookApi(HttpClient http)
    {
        _http = http;
    }

    // LISTAR
    public async Task<IReadOnlyList<HandbookDto>> GetAsync(CancellationToken ct = default)
    {
        using var resp = await _http.GetAsync("/api/Handbook", ct);

        if (resp.StatusCode == HttpStatusCode.NoContent)
            return [];

        if (!resp.IsSuccessStatusCode)
        {
            var raw = await resp.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Error HTTP {(int)resp.StatusCode} ({resp.ReasonPhrase}): {raw}"
            );
        }

        var payload = await resp.Content.ReadFromJsonAsync<Response<List<HandbookDto>>>(cancellationToken: ct);
        return payload?.data ?? [];
    }

    // SUBIR
    public async Task<HandbookUploadResponseDto> UploadAsync(Stream stream, string fileName, string contentType, string? folder, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();

        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        form.Add(fileContent, "file", fileName);
        form.Add(new StringContent(folder ?? ""), "Folder");

        var url = $"/api/handbook/upload?folder={folder}";

        using var resp = await _http.PostAsync(url, form, ct);
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<HandbookUploadResponseDto>(cancellationToken: ct) ?? new HandbookUploadResponseDto();
    }

    // DESCARGAR
    public async Task<byte[]> DownloadAsync(long id, CancellationToken ct = default)
    {
        using var resp = await _http.GetAsync($"api/handbook/{id}/download", ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsByteArrayAsync(ct);
    }

    // VER (devuelve URI para abrir en navegador/visor)
    public Uri GetViewPdfUri(long id)
    {
        if (_http.BaseAddress is null)
            throw new InvalidOperationException("HttpClient.BaseAddress no está configurado");

        // Quita /swagger si existe y regresa al root del host
        var baseUri = _http.BaseAddress;
        var root = new Uri(baseUri.GetLeftPart(UriPartial.Authority)); // https://host.com

        return new Uri(root, $"api/handbook/{id}/view");
    }
    public async Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        try
        {
            using var resp = await _http.DeleteAsync($"api/handbook/{id}", ct);
            var result = await resp.Content.ReadFromJsonAsync<Response<bool>>(cancellationToken: ct);

            if (result is not null)
                return result;

            return new Response<bool>
            {
                succes = false,
                message = "No se recibió respuesta válida del servidor"
            };
        }
        catch (Exception ex)
        {
            return new Response<bool>
            {
                succes = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }

}
