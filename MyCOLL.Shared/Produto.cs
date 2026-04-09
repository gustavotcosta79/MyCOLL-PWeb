using MyCOLL.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MyCOLL.Shared
{
    public class Produto
    {
        public int Id { get; set; }

        [Required]
        public String Nome { get; set; } = string.Empty;
        public String? Descricao { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrecoBase { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MargemLucro { get; set; } // Ex: 0.20 para 20%

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrecoVenda { get; set; } // Valor final guardado
        public int Stock {  get; set; }
        public String? ImagemUrl { get; set; }

        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        public int ModoDisponibilizacaoId { get; set; }
        public ModoDisponibilizacao? ModoDisponibilizacao { get; set; }
        public string? FornecedorId { get; set; } = string.Empty;

        [NotMapped]
        public string? FornecedorNomeAuxiliar { get; set; }

        public EstadoProduto Estado { get; set; } = EstadoProduto.Pendente;

    }
}
