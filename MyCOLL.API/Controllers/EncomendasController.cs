using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using MyCOLL.API.Repositories;
using MyCOLL.Shared;
using System.Security.Claims; 

namespace MyCOLL.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class EncomendasController : ControllerBase
    {
        private readonly IEncomendaRepository _repository;

        public EncomendasController(IEncomendaRepository repository)
        {
            _repository = repository;
        }

        // POST: api/Encomendas
        [HttpPost]
        public async Task<ActionResult<Encomenda>> PostEncomenda(Encomenda encomenda)
        {
            // 1. Validar estrutura
            if (encomenda == null || encomenda.Detalhes == null || !encomenda.Detalhes.Any())
            {
                return BadRequest("A encomenda tem de ter produtos.");
            }

            // 2. SEGURANÇA: Obter o ID do utilizador através do Token (Quem está logado?)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 3. Forçar o ID na encomenda (Ignora o que veio no JSON para evitar fraude)
            encomenda.ClienteId = userId;

            // 4. Gravar via Repositório
            var novaEncomenda = await _repository.CreateEncomenda(encomenda);

            return CreatedAtAction(nameof(GetEncomenda), new { id = novaEncomenda.Id }, novaEncomenda);
        }

        // GET: api/Encomendas/MeusPedidos
     
        [HttpGet("MeusPedidos")]
        public async Task<ActionResult<IEnumerable<Encomenda>>> GetHistorico()
        {
            // Descobrir quem está a pedir
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Pedir ao repo as encomendas DESTE user
            var encomendas = await _repository.GetEncomendasPorCliente(userId);
            return Ok(encomendas);
        }

        // GET: api/Encomendas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Encomenda>> GetEncomenda(int id)
        {
            var encomenda = await _repository.GetEncomenda(id);

            if (encomenda == null) return NotFound();

            // SEGURANÇA: Verificar se a encomenda pertence a quem a está a pedir
            // (Impedir que o Cliente A veja a encomenda do Cliente B)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Se não for o dono E não for Admin -> Proibido
            if (encomenda.ClienteId != userId && !User.IsInRole("Admin"))
            {
                return Forbid(); // 403 Forbidden
            }

            return Ok(encomenda);
        }

        [HttpGet("VendasFornecedor")]
        [Authorize(Roles = "Fornecedor,Admin")]
        public async Task <ActionResult<IEnumerable<DetalheEncomenda>>> GetMinhasVendas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var vendas = await _repository.GetVendasPorFornecedor(userId);
            return Ok(vendas);
        }
    }
}