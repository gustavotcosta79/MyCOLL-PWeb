using MyCOLL.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCOLL.Shared.DTOs
{
    public class RegistoDto
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string NIF { get; set; } = string.Empty;
        public string Morada { get; set; } = string.Empty;

        [Required]
        public TipoUtilizador Tipo { get; set; } = TipoUtilizador.Cliente;
    }
}
