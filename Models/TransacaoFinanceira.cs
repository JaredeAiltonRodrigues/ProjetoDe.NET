using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OngDoacao.Models
{
    public class TransacaoFinanceira
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string? Descricao { get; set; } // Ex: "Doação PIX Anônimo", "Compra de Arroz", "Conta de Luz da ONG"
        
        [Range(0.01, 1000000000)]
        [Required]
        public decimal Valor { get; set; }
        
        [Required, RegularExpression("^(Entrada|Saída)$")]
        public string? Tipo { get; set; } // "Entrada" (Doações) ou "Saída" (Gastos)
        
        public DateTime DataTransacao { get; set; } = DateTime.Now;
        
// --- DADOS PARA OPEN FINANCE E PIX ---
        [RegularExpression("^(PIX|Dinheiro|Transferência)$")]
        public string? MetodoPagamento { get; set; } // "PIX", "Transferência", "Boleto"
        public string? IdentificacaoPagador { get; set; } // Nome ou CPF de quem enviou o PIX
        public string? IdTransacaoBancaria { get; set; } // ID retornado pela API do Banco (Open Finance)
        public bool Conciliado { get; set; } = false; // Define se já foi validado com o saldo do banco
        
        // --- DADOS PARA SEFAZ E RECEITA FEDERAL ---
        public bool ExigeNotaFiscal { get; set; } = false;
        public string? ChaveAcessoNFe { get; set; } // Chave de 44 dígitos da SEFAZ
        public string? StatusSefaz { get; set; } // "Pendente", "Emitida", "Rejeitada"
        
        public string? ComprovanteUrl { get; set; }

    }
}