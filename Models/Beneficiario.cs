using System;
using System.ComponentModel.DataAnnotations;

namespace OngDoacao.Models
{
    public class Beneficiario
    {
        [Key]
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Cpf { get; set; }
        public int TamanhoFamilia { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public string StatusFila { get; set; } = "Aguardando"; 
        public DateTime? HorarioRetirada { get; set; }
    }
}