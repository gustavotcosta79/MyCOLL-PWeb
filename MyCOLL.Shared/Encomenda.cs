using MyCOLL.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MyCOLL.Shared
{
    public class Encomenda
    {
        public int Id { get; set; }
        public DateTime DataEncomenda { get; set; } = DateTime.Now;
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ValorTotal { get; set; }

        public EstadoEncomenda Estado { get; set; } = EstadoEncomenda.Pendente;

        public String ClienteId { get; set; } = string.Empty;
        
        public ICollection <DetalheEncomenda>? Detalhes { get; set; }


    }
}
