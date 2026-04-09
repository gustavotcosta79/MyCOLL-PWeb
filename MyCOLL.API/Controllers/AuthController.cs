using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyCOLL.Data;
using MyCOLL.Shared.DTOs;
using MyCOLL.Shared.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyCOLL.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
{
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager; 

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _configuration = configuration; 
            _roleManager = roleManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistoDto model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return BadRequest("Utilizador já existe.");

            EstadoConta estadoInicial = (model.Tipo == TipoUtilizador.Fornecedor)
                                ? EstadoConta.Pendente
                                : EstadoConta.Ativo;

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email,
                Nome = model.Nome,
                NIF = model.NIF,
                Morada = model.Morada,
                Tipo = model.Tipo,
                EstadoConta = estadoInicial
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao criar utilizador.");

            string roleName = (model.Tipo == TipoUtilizador.Fornecedor) ? "Fornecedor" : "Cliente";

            // Verificação de segurança: garantir que a role existe na BD antes de atribuir
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }
            else
            {
                // Opcional: Criar a role se não existir ou registar log de erro
                await _roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(roleName));
                await _userManager.AddToRoleAsync(user, roleName);
            }
            
            if (model.Tipo == TipoUtilizador.Fornecedor)
            {
                return Ok(new { Message = "Conta de fornecedor criada! Aguarda aprovação do administrador." });
            }

            return Ok("Registo efetuado com sucesso! Aguarde aprovação.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                if (user.EstadoConta != EstadoConta.Ativo)
                    return Unauthorized("Conta pendente ou suspensa.");

                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim("TipoUtilizador", user.Tipo.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var role in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized("Email ou password incorretos.");
        }

        [HttpGet("perfil")]
        [Authorize]
        public async Task<ActionResult<PerfilDto>> GetPerfil()
        {
            // vemos quem é o utilizador logado?
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null) return NotFound("Utilizador não encontrado.");

            return Ok(new PerfilDto
            {
                Email = user.Email!,
                Nome = user.Nome,
                NIF = user.NIF,
                Morada = user.Morada
            });
        }

        [HttpPut("perfil")]
        [Authorize]
        public async Task <IActionResult> UpdatePerfil([FromBody] PerfilDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null) return NotFound();

            // Atualizar campos
            user.Nome = model.Nome;
            user.NIF = model.NIF;
            user.Morada = model.Morada;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Perfil atualizado com sucesso!" });
            }

            return BadRequest(result.Errors.FirstOrDefault()?.Description);
        }

    }
}