using System.Collections.Generic;

namespace StrongFitApp.Models
{
    public class DiagnosticViewModel
    {
        public bool DatabaseStatus { get; set; }
        public List<TableStatus> TablesStatus { get; set; } = new List<TableStatus>();
        public int UsersCount { get; set; }
        public int RolesCount { get; set; }
        public int PersonalsCount { get; set; }
        public int AlunosCount { get; set; }
        public int ExerciciosCount { get; set; }
        public int TreinosCount { get; set; }
    }
}
