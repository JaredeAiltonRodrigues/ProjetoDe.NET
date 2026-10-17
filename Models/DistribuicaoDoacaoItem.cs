using System;

namespace OngDoacao.Models
{
    public class DistribuicaoDoacaoItem
    {
        public int Id { get; set; }
        public int DistribuicaoDoacaoId { get; set; }
        public DistribuicaoDoacao DistribuicaoDoacao { get; set; } = null!;
        public string NomeAlimento { get; set; } = string.Empty;
        public string? CodigoDeBarras { get; set; }
        public DateTime DataVencimento { get; set; }
        public int Quantidade { get; set; }
    }
}