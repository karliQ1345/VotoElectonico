using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using VotoElect.MVC.ApiContracts;
using VotoElect.MVC.Controllers;

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

    public async Task<ApiResponse<IniciarVotacionResponseDto>?> IniciarVotacionAsync(
    IniciarVotacionRequestDto req,
    string token,
    CancellationToken ct = default)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Post, "api/votacion/iniciar");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        msg.Content = JsonContent.Create(req);

        var resp = await Client().SendAsync(msg, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);

        Console.WriteLine("[IniciarVotacion] URL=api/votacion/iniciar");
        Console.WriteLine($"[IniciarVotacion] Status={(int)resp.StatusCode} {resp.StatusCode}");
        Console.WriteLine($"[IniciarVotacion] Body={raw}");

        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
            resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return new ApiResponse<IniciarVotacionResponseDto>
            {
                Ok = false,
                Message = "No autorizado para iniciar votación. Vuelve a iniciar sesión.",
                Data = null
            };
        }

        if (string.IsNullOrWhiteSpace(raw))
        {
            return new ApiResponse<IniciarVotacionResponseDto>
            {
                Ok = false,
                Message = $"Respuesta vacía. HTTP {(int)resp.StatusCode} {resp.StatusCode}",
                Data = null
            };
        }

        if (!resp.IsSuccessStatusCode)
        {
            return new ApiResponse<IniciarVotacionResponseDto>
            {
                Ok = false,
                Message = $"HTTP {(int)resp.StatusCode} {resp.StatusCode}: {raw}",
                Data = null
            };
        }

        return JsonSerializer.Deserialize<ApiResponse<IniciarVotacionResponseDto>>(
            raw,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
    }
    public async Task<ApiResponse<IniciarVotacionResponseDto>?> IniciarVotacionAsync(
    IniciarVotacionRequestDto req,
    CancellationToken ct = default)
    {
        var resp = await Client().PostAsJsonAsync("api/votacion/iniciar", req, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<IniciarVotacionResponseDto>>(cancellationToken: ct);
    }
    public async Task<ApiResponse<BoletaDataDto>?> GetBoletaAsync(string procesoId, string eleccionId, CancellationToken ct = default)
    {
        var url = $"api/votacion/boleta?procesoId={procesoId}&eleccionId={eleccionId}";
        return await Client().GetFromJsonAsync<ApiResponse<BoletaDataDto>>(url, ct);
    }

    public async Task<ApiResponse<BoletaDataDto>?> GetBoletaActivaAsync(string procesoId, CancellationToken ct = default)
    {
        var url = $"api/votacion/boleta-activa?procesoId={procesoId}";
        return await Client().GetFromJsonAsync<ApiResponse<BoletaDataDto>>(url, ct);
    }

    public async Task<ApiResponse<EmitirVotoResponseDto>?> EmitirVotoAsync(EmitirVotoRequestDto req, CancellationToken ct = default)
    {
        var resp = await Client().PostAsJsonAsync("api/votacion/emitir", req, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<EmitirVotoResponseDto>>(cancellationToken: ct);
    }

    // ---- Jefe de Junta 
    public async Task<ApiResponse<JefePanelDto>?> GetJefePanelAsync(
       string procesoId, string token, CancellationToken ct = default)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Get, $"api/juntas/panel/{procesoId}");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await Client().SendAsync(msg, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);

        Console.WriteLine($"[GetJefePanel] URL=api/juntas/panel/{procesoId}");
        Console.WriteLine($"[GetJefePanel] Status={(int)resp.StatusCode} {resp.StatusCode}");
        Console.WriteLine($"[GetJefePanel] Body={raw}");

        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
            resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return new ApiResponse<JefePanelDto>
            {
                Ok = false,
                Message = "No autorizado (token/rol). Vuelve a iniciar sesión.",
                Data = null
            };
        }

        if (string.IsNullOrWhiteSpace(raw))
        {
            return new ApiResponse<JefePanelDto>
            {
                Ok = false,
                Message = $"Respuesta vacía. HTTP {(int)resp.StatusCode} {resp.StatusCode}",
                Data = null
            };
        }

        if (!resp.IsSuccessStatusCode)
        {
            return new ApiResponse<JefePanelDto>
            {
                Ok = false,
                Message = $"HTTP {(int)resp.StatusCode} {resp.StatusCode}: {raw}",
                Data = null
            };
        }

        // Success: deserializa
        return System.Text.Json.JsonSerializer.Deserialize<ApiResponse<JefePanelDto>>(
            raw,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
    }

    //--
    public async Task<ApiResponse<IdResponseDto>?> AdminCrearEleccionAsync(CrearEleccionRequestDto req, string token, CancellationToken ct = default)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Post, "api/elecciones");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        msg.Content = JsonContent.Create(req);

        var resp = await Client().SendAsync(msg, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<IdResponseDto>>(cancellationToken: ct);
    }

    public async Task<ApiResponse<IdResponseDto>?> AdminCrearListaAsync(CrearListaRequestDto req, string token, CancellationToken ct = default)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Post, "api/elecciones/listas");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        msg.Content = JsonContent.Create(req);

        var resp = await Client().SendAsync(msg, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<IdResponseDto>>(cancellationToken: ct);
    }

    public async Task<ApiResponse<IdResponseDto>?> AdminCrearCandidatoAsync(CrearCandidatoRequestDto req, string token, CancellationToken ct = default)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Post, "api/elecciones/candidatos");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        msg.Content = JsonContent.Create(req);

        var resp = await Client().SendAsync(msg, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<IdResponseDto>>(cancellationToken: ct);
    }
    //--
    public async Task<ApiResponse<ProcesoActivoDto>?> GetProcesoActivoAsync(string token, CancellationToken ct = default)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Get, "api/public/procesos/activo");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await Client().SendAsync(msg, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);

        Console.WriteLine("[GetProcesoActivo] URL=api/public/procesos/activo");
        Console.WriteLine($"[GetProcesoActivo] Status={(int)resp.StatusCode} {resp.StatusCode}");
        Console.WriteLine($"[GetProcesoActivo] Body={raw}");

        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
            resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return new ApiResponse<ProcesoActivoDto>
            {
                Ok = false,
                Message = "No autorizado (token/rol). Vuelve a iniciar sesión.",
                Data = null
            };
        }

        if (string.IsNullOrWhiteSpace(raw))
        {
            return new ApiResponse<ProcesoActivoDto>
            {
                Ok = false,
                Message = $"Respuesta vacía. HTTP {(int)resp.StatusCode} {resp.StatusCode}",
                Data = null
            };
        }

        if (!resp.IsSuccessStatusCode)
        {
            return new ApiResponse<ProcesoActivoDto>
            {
                Ok = false,
                Message = $"HTTP {(int)resp.StatusCode} {resp.StatusCode}: {raw}",
                Data = null
            };
        }

        return JsonSerializer.Deserialize<ApiResponse<ProcesoActivoDto>>(
            raw,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
    }
    public async Task<ApiResponse<ProcesoActivoDto>?> GetProcesoActivoPublicAsync(CancellationToken ct = default)
    {
        var resp = await Client().GetAsync("api/public/procesos/activo", ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);

        Console.WriteLine("[GetProcesoActivoPublic] URL=api/public/procesos/activo");
        Console.WriteLine($"[GetProcesoActivoPublic] Status={(int)resp.StatusCode} {resp.StatusCode}");
        Console.WriteLine($"[GetProcesoActivoPublic] Body={raw}");

        if (string.IsNullOrWhiteSpace(raw))
            return new ApiResponse<ProcesoActivoDto> { Ok = false, Message = $"Respuesta vacía. HTTP {(int)resp.StatusCode} {resp.StatusCode}" };

        if (!resp.IsSuccessStatusCode)
            return new ApiResponse<ProcesoActivoDto> { Ok = false, Message = $"HTTP {(int)resp.StatusCode} {resp.StatusCode}: {raw}" };

        return JsonSerializer.Deserialize<ApiResponse<ProcesoActivoDto>>(
            raw,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
    }
    public async Task<ApiResponse<long>?> GetJuntasReportadasPublicAsync(string procesoId, CancellationToken ct = default)
    {
        var url = $"api/public/procesos/{procesoId}/juntas-reportadas";

        var resp = await Client().GetAsync(url, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);

        Console.WriteLine($"[GetJuntasReportadasPublic] URL={url}");
        Console.WriteLine($"[GetJuntasReportadasPublic] Status={(int)resp.StatusCode} {resp.StatusCode}");
        Console.WriteLine($"[GetJuntasReportadasPublic] Body={raw}");

        if (string.IsNullOrWhiteSpace(raw))
            return new ApiResponse<long> { Ok = false, Message = $"Respuesta vacía. HTTP {(int)resp.StatusCode} {resp.StatusCode}" };

        if (!resp.IsSuccessStatusCode)
            return new ApiResponse<long> { Ok = false, Message = $"HTTP {(int)resp.StatusCode} {resp.StatusCode}: {raw}" };

        return JsonSerializer.Deserialize<ApiResponse<long>>(
            raw,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
    }
    public async Task<ApiResponse<ProcesoActivoDto>?> GetUltimoFinalizadoPublicAsync(CancellationToken ct = default)
    {
        var resp = await Client().GetAsync("api/public/procesos/ultimo-finalizado", ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);

        if (string.IsNullOrWhiteSpace(raw))
            return new ApiResponse<ProcesoActivoDto> { Ok = false, Message = "Respuesta vacía del API." };

        return JsonSerializer.Deserialize<ApiResponse<ProcesoActivoDto>>(
            raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
    }

    public async Task<ApiResponse<JefeVerificarVotanteResponseDto>?> JefeVerificarAsync(JefeVerificarVotanteRequestDto req, string token, CancellationToken ct = default)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Post, "api/juntas/verificar");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        msg.Content = JsonContent.Create(req);

        var resp = await Client().SendAsync(msg, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<JefeVerificarVotanteResponseDto>>(cancellationToken: ct);
    }

    public async Task<ApiResponse<FinalizarJuntaResponseDto>?> FinalizarJuntaAsync(
    string procesoId, string token, CancellationToken ct = default)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Post, "api/juntas/finalizar");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        msg.Content = JsonContent.Create(new FinalizarJuntaRequestDto
        {
            ProcesoElectoralId = procesoId
        });

        var resp = await Client().SendAsync(msg, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<FinalizarJuntaResponseDto>>(cancellationToken: ct);
    }

    // ---- Admin
    public async Task<ApiResponse<List<ProcesoResumenDto>>?> AdminListarProcesosAsync(string token, CancellationToken ct = default)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Get, "api/procesos");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await Client().SendAsync(msg, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);

        Console.WriteLine("[AdminListarProcesos] URL=api/procesos");
        Console.WriteLine($"[AdminListarProcesos] Status={(int)resp.StatusCode} {resp.StatusCode}");
        Console.WriteLine($"[AdminListarProcesos] Body={raw}");

        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
            resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return new ApiResponse<List<ProcesoResumenDto>>
            {
                Ok = false,
                Message = "No autorizado (token/rol). Vuelve a iniciar sesión.",
                Data = null
            };
        }

        if (string.IsNullOrWhiteSpace(raw))
        {
            return new ApiResponse<List<ProcesoResumenDto>>
            {
                Ok = false,
                Message = $"Respuesta vacía. HTTP {(int)resp.StatusCode} {resp.StatusCode}",
                Data = null
            };
        }

        if (!resp.IsSuccessStatusCode)
        {
            return new ApiResponse<List<ProcesoResumenDto>>
            {
                Ok = false,
                Message = $"HTTP {(int)resp.StatusCode} {resp.StatusCode}: {raw}",
                Data = null
            };
        }

        return JsonSerializer.Deserialize<ApiResponse<List<ProcesoResumenDto>>>(
            raw,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
    }

public async Task<ApiResponse<IdResponseDto>?> AdminCrearProcesoAsync(CrearProcesoRequestDto req, string token, CancellationToken ct = default)
{
    using var msg = new HttpRequestMessage(HttpMethod.Post, "api/procesos");
    msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    msg.Content = JsonContent.Create(req);

    var resp = await Client().SendAsync(msg, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);

        Console.WriteLine($"[AdminCrearProceso] Status={(int)resp.StatusCode} {resp.StatusCode}");
        Console.WriteLine($"[AdminCrearProceso] Body={raw}");

        if (!resp.IsSuccessStatusCode)
        {
            return new ApiResponse<IdResponseDto>
            {
                Ok = false,
                Message = $"HTTP {(int)resp.StatusCode} {resp.StatusCode}: {raw}",
                Data = null
            };
        }
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
    public async Task<ApiResponse<CargaPadronResponseDto>?> AdminCargarPadronAsync(
    string procesoId, List<PadronExcelRowDto> rows, string token, CancellationToken ct = default)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Post, $"api/padron/{procesoId}/carga");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        msg.Content = JsonContent.Create(rows);
        var resp = await Client().SendAsync(msg, ct);
        return await resp.Content.ReadFromJsonAsync<ApiResponse<CargaPadronResponseDto>>(cancellationToken: ct);
    }
}


