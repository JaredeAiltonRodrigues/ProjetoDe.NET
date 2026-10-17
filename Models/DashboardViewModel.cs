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
        public List<Beneficiario> Fila { get; set; } = new List<Beneficiario>();

        public List<Beneficiario> BeneficiariosPrevistos { get; set; } = new List<Beneficiario>();
        public string? SugestaoIA { get; set; }
        public bool AssistenteDisponivel { get; set; }
    }
}