using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyCOLL.Shared
{
    public class ModoDisponibilizacao
    {
        public int Id { get; set; }
        // "Venda Unitária", "Listagem", "Pack 10", "Aluguer"
        [Required(ErrorMessage = "O nome do modo é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        // "Item vendido individualmente", "Apenas para exibição na coleção", "Pack promocional"
        [StringLength(200, ErrorMessage = "O detalhe não pode exceder 200 caracteres.")]
        public string? Detalhe { get; set; }

        // Permite "desligar" um modo antigo sem ter de o apagar da BD (o que daria erro nas encomendas antigas)
        public bool Ativo { get; set; } = true;

        // Permite saber: "Quais são todos os produtos que estão marcados como 'Apenas Listagem'?"
        [JsonIgnore] 
        public ICollection<Produto>? Produtos { get; set; }

    }
}
