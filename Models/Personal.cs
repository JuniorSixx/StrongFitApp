using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StrongFitApp.Models
{
    public class Personal
    {
        [Key]
        public int PersonalID { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A especialidade é obrigatória")]
        [StringLength(100)]
        public string Especialidade { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Telefone { get; set; }

        [StringLength(50)]
        public string? Instagram { get; set; }

        // Relacionamento com Alunos
        public virtual ICollection<Aluno>? Alunos { get; set; }
    }
}
