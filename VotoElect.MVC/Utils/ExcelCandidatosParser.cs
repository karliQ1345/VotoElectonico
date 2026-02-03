using ClosedXML.Excel;
using VotoElect.MVC.ApiContracts;

namespace VotoElect.MVC.Utils;

public static class ExcelCandidatosParser
{
    /// <summary>
    /// Columnas esperadas (fila 1 headers):
    /// NombreCompleto | 2 Cargo | 3 FotoUrl | 4 PartidoListaId
    /// </summary>
    public static List<CrearCandidatoRequestDto> Leer(Stream stream)
    {
        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheet(1);

        var rows = new List<CrearCandidatoRequestDto>();
        var lastRowUsed = ws.LastRowUsed();
        if (lastRowUsed == null)
            return rows;

        var lastRow = lastRowUsed.RowNumber();
        for (int r = 2; r <= lastRow; r++)
        {
            var nombre = ws.Cell(r, 1).GetString().Trim();
            var fotoUrl = ws.Cell(r, 3).GetString().Trim();
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(fotoUrl))
                continue;


            var cargo = ws.Cell(r, 2).GetString().Trim();
            var partidoListaId = ws.Cell(r, 4).GetString().Trim();


            rows.Add(new CrearCandidatoRequestDto
            {
                NombreCompleto = nombre,
                Cargo = string.IsNullOrWhiteSpace(cargo) ? null : cargo,
                FotoUrl = fotoUrl,
                PartidoListaId = string.IsNullOrWhiteSpace(partidoListaId) ? null : partidoListaId
            });
        }

        return rows;
    }
}
