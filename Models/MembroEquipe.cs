using System;
using System.ComponentModel.DataAnnotations;

namespace OngDoacao.Models
{
    public class MembroEquipe
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string? Nome { get; set; }
        
        [Required]
        public string? CargoOuFuncao { get; set; }
        
        public string? Telefone { get; set; }
        
        public string? TipoVinculo { get; set; }
        
        // --- NOVOS CAMPOS PARA O LOGIN ---
        [Required]
        public string? Senha { get; set; } 
        
        [Required]
        public string? PerfilAcesso { get; set; } // Pode ser: "Admin", "Estoque" ou "Financeiro"
        // ---------------------------------

        public DateTime DataEntrada { get; set; } = DateTime.Now;
        public bool Ativo { get; set; } = true;
    }
}