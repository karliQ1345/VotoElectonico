using System.Net.Http.Headers;
using System.Net.Http.Json;
using VotoElect.MVC.ApiContracts;

namespace VotoElect.MVC.Services;

public class ApiService
{
    private readonly IHttpClientFactory _http;
    public ApiService(IHttpClientFactory http) => _http = http;

    private HttpClient Client() => _http.CreateClient("Api");

    public async Task<ApiResponse<LoginResponseDto>?> LoginAsync(string cedula, CancellationToken ct = default)
    {
        var req = new LoginRequestDto { Cedula = cedula };
        var resp = await Client().PostAsJsonAsync("api/auth/login", req, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>(cancellationToken: ct);
    }

    public async Task<ApiResponse<TwoFactorVerifyResponseDto>?> VerifyOtpAsync(string twoFactorSessionId, string codigo, CancellationToken ct = default)
    {
        var req = new TwoFactorVerifyRequestDto { TwoFactorSessionId = twoFactorSessionId, Codigo = codigo };
        var resp = await Client().PostAsJsonAsync("api/twofactor/verify", req, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<TwoFactorVerifyResponseDto>>(cancellationToken: ct);
    }

    public async Task<ApiResponse<IniciarVotacionResponseDto>?> IniciarVotacionAsync(IniciarVotacionRequestDto req, CancellationToken ct = default)
    {
        var resp = await Client().PostAsJsonAsync("api/votacion/iniciar", req, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<IniciarVotacionResponseDto>>(cancellationToken: ct);
    }

    public async Task<ApiResponse<BoletaDataDto>?> GetBoletaAsync(string procesoId, string eleccionId, CancellationToken ct = default)
    {
        var url = $"api/votacion/boleta?procesoId={procesoId}&eleccionId={eleccionId}";
        return await Client().GetFromJsonAsync<ApiResponse<BoletaDataDto>>(url, ct);
    }

    public async Task<ApiResponse<EmitirVotoResponseDto>?> EmitirVotoAsync(EmitirVotoRequestDto req, CancellationToken ct = default)
    {
        var resp = await Client().PostAsJsonAsync("api/votacion/emitir", req, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<EmitirVotoResponseDto>>(cancellationToken: ct);
    }

    // ---- Jefe de Junta 
    public async Task<ApiResponse<JefePanelDto>?> GetJefePanelAsync(string procesoId, string token, CancellationToken ct = default)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Get, $"api/juntas/panel/{procesoId}");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await Client().SendAsync(msg, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<JefePanelDto>>(cancellationToken: ct);
    }

    public async Task<ApiResponse<JefeVerificarVotanteResponseDto>?> JefeVerificarAsync(JefeVerificarVotanteRequestDto req, string token, CancellationToken ct = default)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Post, "api/juntas/verificar");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        msg.Content = JsonContent.Create(req);

        var resp = await Client().SendAsync(msg, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<JefeVerificarVotanteResponseDto>>(cancellationToken: ct);
    }

    // ---- Admin
    public async Task<ApiResponse<List<ProcesoResumenDto>>?> AdminListarProcesosAsync(string token, CancellationToken ct = default)
{
    using var msg = new HttpRequestMessage(HttpMethod.Get, "api/procesos");
    msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var resp = await Client().SendAsync(msg, ct);
    return await resp.Content.ReadFromJsonAsync<ApiResponse<List<ProcesoResumenDto>>>(cancellationToken: ct);
}

public async Task<ApiResponse<IdResponseDto>?> AdminCrearProcesoAsync(CrearProcesoRequestDto req, string token, CancellationToken ct = default)
{
    using var msg = new HttpRequestMessage(HttpMethod.Post, "api/procesos");
    msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    msg.Content = JsonContent.Create(req);

    var resp = await Client().SendAsync(msg, ct);
    return await resp.Content.ReadFromJsonAsync<ApiResponse<IdResponseDto>>(cancellationToken: ct);
}

public async Task<ApiResponse<string>?> AdminActivarProcesoAsync(string procesoId, string token, CancellationToken ct = default)
{
    using var msg = new HttpRequestMessage(HttpMethod.Post, $"api/procesos/{procesoId}/activar");
    msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var resp = await Client().SendAsync(msg, ct);
    return await resp.Content.ReadFromJsonAsync<ApiResponse<string>>(cancellationToken: ct);
}

public async Task<ApiResponse<string>?> AdminFinalizarProcesoAsync(string procesoId, string token, CancellationToken ct = default)
{
    using var msg = new HttpRequestMessage(HttpMethod.Post, $"api/procesos/{procesoId}/finalizar");
    msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var resp = await Client().SendAsync(msg, ct);
    return await resp.Content.ReadFromJsonAsync<ApiResponse<string>>(cancellationToken: ct);
}

public async Task<ApiResponse<ReporteResponseDto>?> GetReporteAsync(ReporteFiltroDto filtro, CancellationToken ct = default)
{
    var resp = await Client().PostAsJsonAsync("api/reportes", filtro, ct);
    return await resp.Content.ReadFromJsonAsync<ApiResponse<ReporteResponseDto>>(cancellationToken: ct);
}

}


