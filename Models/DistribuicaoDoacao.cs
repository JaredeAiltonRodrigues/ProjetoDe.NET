using System;
using System.Collections.Generic;

namespace OngDoacao.Models
{
    public class DistribuicaoDoacao
    {
        public int Id { get; set; }
        public int BeneficiarioId { get; set; }
        public Beneficiario Beneficiario { get; set; } = null!;
        public DateTime DataReserva { get; set; } = DateTime.Now;
        public DateTime HorarioRetirada { get; set; }
        public DateTime? DataEntrega { get; set; }
        public string Status { get; set; } = "Reservada";
        public string StatusNotificacao { get; set; } = "Pendente";
        public DateTime? DataNotificacao { get; set; }
        public string? DetalheNotificacao { get; set; }
        public List<DistribuicaoDoacaoItem> Itens { get; set; } = new();
    }
}