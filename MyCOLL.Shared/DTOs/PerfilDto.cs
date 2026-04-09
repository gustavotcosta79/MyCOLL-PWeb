using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCOLL.Shared.DTOs
{
    public class PerfilDto
    {
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Nome { get; set; }
        public string? NIF { get; set; }
        public string? Morada { get; set; }
    }
}
