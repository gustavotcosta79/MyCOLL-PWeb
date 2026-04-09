using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyCOLL.Shared
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;
        public int Nivel { get; set; } // 1=Principal, 2=País, 3=Tema
        public bool Ativa { get; set; } = true;
        public int ? CategoriaPaiId { get; set; }

        [JsonIgnore] 
        public Categoria? CategoriaPai { get; set; }
        public ICollection<Categoria>? SubCategorias { get; set; }
        [JsonIgnore]
        public ICollection<Produto>? Produtos { get; set; }
    }

}
