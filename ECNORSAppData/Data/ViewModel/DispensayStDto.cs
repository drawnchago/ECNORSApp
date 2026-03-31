using ECNORSAppData.Data.DTO;

namespace ECNORSAppData.Data.Config;

public static class DispensaryStatusMap
{
    public record DispensaryStatusInfo(int Id, string Code, string Color, string Description);

    public static readonly Dictionary<int, DispensaryStatusInfo> Items = new()
    {
        { 0, new(0, "NCfg", "#CCCCCC", "No Configurado") },
        { 1, new(1, "F/L",  "#BF1616", "Fuera de Línea") },
        { 2, new(2, "INA",  "#FFFFFF", "Inactivo") },
        { 3, new(3, "LLA",  "#383EFF", "Llamando") },
        { 4, new(4, "RES",  "#3F48CC", "Reservado") },
        { 5, new(5, "AUT",  "#22B14C", "Autorizado") },
        { 6, new(6, "SRV",  "#B5E61D", "Sirviendo") },
        { 7, new(7, "SUS",  "#7F0E0E", "Suspendido") },
        { 8, new(8, "PAG",  "#757575", "Pagable") },
        { 9, new(9, "COB",  "#0000FF", "Cobrar") }
    };
    public static DispensaryStatusInfo Get(int? status) => Items.TryGetValue(status ?? -1, out var item) ? item : new(-1, "N/A", "#999999", "No definido");
}