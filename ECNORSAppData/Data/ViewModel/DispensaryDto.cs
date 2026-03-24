namespace ECNORSAppData.Data.DTO
{
    public class DispensaryDto
    {
        public int DispensaryId { get; set; }
        public int IslaId { get; set; }
        public int? Secuencia { get; set; }
        public DateTime? FechaSecuencia { get; set; }
        public bool Generado { get; set; }
        public bool Sirviendo { get; set; }
        public bool Despacho { get; set; }
        public int? Estatus { get; set; }

        public string StationName { get; set; } = string.Empty; 
        public string DisplayName => $"Dispensario # {DispensaryId}";
    }
}