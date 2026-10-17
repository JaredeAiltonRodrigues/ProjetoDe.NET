using System;
using System.ComponentModel.DataAnnotations;

namespace OngDoacao.Models
{
    public class EscalaVoluntario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MembroEquipeId { get; set; }
        public MembroEquipe? Membro { get; set; } // Ligação com o funcionário

        [Required]
        public DateTime DataTurno { get; set; }

        [Required]
        public string? Setor { get; set; } // Ex: "Triagem", "Entrega de Cestas", "Cozinha"

        public string? Observacao { get; set; }
    }
}