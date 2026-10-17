using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OngDoacao.Models
{
    public class DashboardViewModel
    {
         public int TotalItensEstoque { get; set; }
        public int CestasDisponiveis { get; set; }
        public int ItensVencendoEmSeteDias { get; set; }
        public int ReservasAtivas { get; set; }
        public int EntregasConcluidas { get; set; }
        public int NotificacoesPendentes { get; set; }
        public List<Beneficiario> Fila { get; set; } = new List<Beneficiario>();

        public List<Beneficiario> BeneficiariosPrevistos { get; set; } = new List<Beneficiario>();
        public List<DistribuicaoDoacao> DistribuicoesRecentes { get; set; } = new List<DistribuicaoDoacao>();
        public List<Alimento> ProximosVencimentos { get; set; } = new List<Alimento>();
        public string? SugestaoIA { get; set; }
        public bool AssistenteDisponivel { get; set; }
    }
}