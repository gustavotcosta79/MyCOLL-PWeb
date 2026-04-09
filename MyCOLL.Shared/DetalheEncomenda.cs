using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyCOLL.Shared
{
    public class DetalheEncomenda
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser pelo menos 1")]
        public int Quantidade { get; set; }

        // O preço que estava em vigor na altura da compra (Registo Histórico)
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrecoUnitario { get; set; }

        public int EncomendaId { get; set; }

        public Encomenda? Encomenda { get; set; }
        public int ProdutoId { get; set; }
        public Produto? Produto { get; set; }


    }
}
