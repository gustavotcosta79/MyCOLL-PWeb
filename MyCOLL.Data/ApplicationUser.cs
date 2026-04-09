using Microsoft.AspNetCore.Identity;
using MyCOLL.Shared.Enums;

namespace MyCOLL.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string Nome { get; set; } = string.Empty;
        public string? NIF { get; set; }
        public string? Morada { get; set; }
        public DateTime DataNascimento { get; set; }

        // Usa o teu Enum do Shared
        public TipoUtilizador Tipo { get; set; } = TipoUtilizador.Cliente;
        public EstadoConta EstadoConta { get; set; } = EstadoConta.Pendente;
    }


}
///Nota: Não te esqueças de registar este serviço no Program.cs do Public e do Mobile: builder.Services.AddScoped<CarrinhoService>();