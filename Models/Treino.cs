using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StrongFitApp.Models
{
    public class Treino
    {
        public int TreinoID { get; set; }

        [Required(ErrorMessage = "O aluno é obrigatório")]
        [Display(Name = "Aluno")]
        public int AlunoID { get; set; }

        [Required(ErrorMessage = "A data é obrigatória")]
        [Display(Name = "Data")]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "A hora é obrigatória")]
        [Display(Name = "Hora")]
        [DataType(DataType.Time)]
        public DateTime Hora { get; set; } = DateTime.Now;

        [Display(Name = "Observações")]
        public string Observacoes { get; set; }

        // Relacionamentos
        public Aluno Aluno { get; set; }
        public List<Exercicio> Exercicios { get; set; } = new List<Exercicio>();
    }
}