using System;
using System.ComponentModel.DataAnnotations;

namespace OngDoacao.Models
{
    public class Alimento
    {
        [Key]
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? CodigoDeBarras { get; set; }
        public DateTime DataVencimento { get; set; }
        public int Quantidade { get; set; }
    }
}