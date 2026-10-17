using System;
using System.ComponentModel.DataAnnotations;

namespace OngDoacao.Models
{
    public class Alimento
    {
        [Key]
        public int Id { get; set; }
        public string? Nome { get; set; }

        [RegularExpression("^[0-9]+$")]
        public string? CodigoDeBarras { get; set; }
        public DateTime DataVencimento { get; set; }

        [Range(1, 1000000)]
        public int Quantidade { get; set; }
    }
}