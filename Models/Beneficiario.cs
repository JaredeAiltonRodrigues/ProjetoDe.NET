using System;
using System.ComponentModel.DataAnnotations;

namespace OngDoacao.Models
{
    public class Beneficiario
    {
        [Key]
        public int Id { get; set; }
        public string? Nome { get; set; }

        [RegularExpression("^\\d{3}\\.?\\d{3}\\.?\\d{3}-?\\d{2}$")]
        public string? Cpf { get; set; }

        [Range(1, 20)]
        public int TamanhoFamilia { get; set; }

        [Phone]
        public string? Telefone { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public string StatusFila { get; set; } = "Aguardando"; 
        public DateTime? HorarioRetirada { get; set; }
    }
}