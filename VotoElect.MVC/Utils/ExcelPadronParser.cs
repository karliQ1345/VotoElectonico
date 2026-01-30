using VotoElectonico.Models.Enums;
using ClosedXML.Excel;
using VotoElect.MVC.ApiContracts;
namespace VotoElect.MVC.Utils
{
    public static class ExcelPadronParser
    {
        // Solo distinguimos: Jefe de Junta vs Votante
        private static RolTipo ParseRol(string s)
        {
            s = (s ?? "").Trim().ToLowerInvariant();

            // Ejemplos válidos: "Jefe de Junta", "JEFE", "JUNTA", "JefeJunta"
            if (s.Contains("jefe") || s.Contains("junta"))
                return RolTipo.JefeJunta;

            return RolTipo.Votante;
        }

        /// <summary>
        /// Lee el primer worksheet del Excel.
        /// Columnas esperadas (fila 1 headers):
        /// 1 Rol | 2 Cedula | 3 NombreCompleto | 4 Email | 5 Provincia | 6 Canton | 7 Parroquia | 8 Genero | 9 FotoUrl | 10 JuntaCodigo
        /// </summary>
        public static List<PadronExcelRowDto> Leer(Stream stream)
        {
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheet(1);

            var rows = new List<PadronExcelRowDto>();

            var lastRowUsed = ws.LastRowUsed();
            if (lastRowUsed == null) return rows;

            var lastRow = lastRowUsed.RowNumber();

            // Fila 1 = encabezados
            for (int r = 2; r <= lastRow; r++)
            {
                // Cedula es clave: si está vacía, ignoramos la fila
                var cedula = ws.Cell(r, 2).GetString().Trim();
                if (string.IsNullOrWhiteSpace(cedula))
                    continue;

                var rolTxt = ws.Cell(r, 1).GetString();
                var rol = ParseRol(rolTxt);

                // Resto de campos (si vienen vacíos, quedan "")
                var nombre = ws.Cell(r, 3).GetString().Trim();
                var email = ws.Cell(r, 4).GetString().Trim();

                var provincia = ws.Cell(r, 5).GetString().Trim();
                var canton = ws.Cell(r, 6).GetString().Trim();
                var parroquia = ws.Cell(r, 7).GetString().Trim();

                var genero = ws.Cell(r, 8).GetString().Trim();
                var fotoUrl = ws.Cell(r, 9).GetString().Trim();
                var juntaCodigo = ws.Cell(r, 10).GetString().Trim();

                rows.Add(new PadronExcelRowDto
                {
                    Rol = rol,
                    Cedula = cedula,
                    NombreCompleto = nombre,
                    Email = email,
                    Provincia = string.IsNullOrWhiteSpace(provincia) ? null : provincia,
                    Canton = string.IsNullOrWhiteSpace(canton) ? null : canton,
                    Parroquia = string.IsNullOrWhiteSpace(parroquia) ? null : parroquia,
                    Genero = string.IsNullOrWhiteSpace(genero) ? null : genero,
                    FotoUrl = string.IsNullOrWhiteSpace(fotoUrl) ? null : fotoUrl,
                    JuntaCodigo = juntaCodigo
                });
            }

            return rows;
        }
    }
}

