namespace StrongFitApp.Models
{
    public class TableStatus
    {
        public string Name { get; set; } = string.Empty;
        public bool Exists { get; set; }
        public int RecordsCount { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
