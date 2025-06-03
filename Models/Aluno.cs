using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StrongFitApp.Models
{
    public class Aluno
    {
        [Key]
        public int AlunoID { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data de nascimento é obrigatória")]
        [DataType(DataType.Date)]
        [Display(Name = "Data de Nascimento")]
        public DateTime Data_Nascimento { get; set; }

        [StringLength(20)]
        public string? Telefone { get; set; }

        [StringLength(50)]
        public string? Instagram { get; set; }

        [StringLength(500)]
        public string? Observacoes { get; set; }

        // Relacionamento com Personal
        [Required(ErrorMessage = "O personal é obrigatório")]
        public int PersonalID { get; set; }

        [ForeignKey("PersonalID")]
        public virtual Personal? Personal { get; set; }

        // Relacionamento com Treinos
        public virtual ICollection<Treino>? Treinos { get; set; }
    }
}
