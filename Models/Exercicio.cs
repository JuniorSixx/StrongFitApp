using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StrongFitApp.Models
{
    public class Exercicio
    {
        [Key]
        public int ExercicioID { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória")]
        [StringLength(50)]
        public string Categoria { get; set; } = string.Empty;

        [Range(1, 20, ErrorMessage = "O número de séries deve estar entre 1 e 20")]
        public int Series { get; set; } = 3;

        [Range(1, 100, ErrorMessage = "O número de repetições deve estar entre 1 e 100")]
        public int Repeticoes { get; set; } = 12;

        public virtual ICollection<Treino>? Treinos { get; set; }
    }
}
