using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // <--- NECESSÁRIO
using MyCOLL.API.Repositories;
using MyCOLL.Shared;
using MyCOLL.Data; // <--- NECESSÁRIO PARA VER O ApplicationUser
using System.Security.Claims;

namespace MyCOLL.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutoRepository _repository;
        // Adicionamos o UserManager para conseguir buscar o nome do fornecedor
        private readonly UserManager<ApplicationUser> _userManager;

        public ProdutosController(IProdutoRepository repository, UserManager<ApplicationUser> userManager)
        {
            _repository = repository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            return Ok(await _repository.GetProdutos());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetProduto(int id)
        {
            var produto = await _repository.GetProduto(id);
            if (produto == null) return NotFound();

            // ==========================================================
            // LÓGICA DO [NotMapped] PARA MOSTRAR O NOME DO FORNECEDOR
            // ==========================================================
            if (!string.IsNullOrEmpty(produto.FornecedorId))
            {
                // Vai buscar o user pelo ID
                var fornecedor = await _userManager.FindByIdAsync(produto.FornecedorId);

                if (fornecedor != null)
                {
                    // Preenche a propriedade auxiliar que criámos no Model
                    produto.FornecedorNomeAuxiliar = fornecedor.Nome;
                }
            }
            // ==========================================================

            return Ok(produto);
        }

        [HttpGet("categoria/{categoriaId}")]
        public async Task<ActionResult<IEnumerable<Produto>>> GetPorCategoria(int categoriaId)
        {
            return Ok(await _repository.GetProdutosPorCategoria(categoriaId));
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImagem(IFormFile ficheiro)
        {
            if (ficheiro == null || ficheiro.Length == 0)
                return BadRequest("Nenhum ficheiro enviado.");

            var nomeFicheiro = $"{Guid.NewGuid()}{Path.GetExtension(ficheiro.FileName)}";
            var caminhoPasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagens");

            if (!Directory.Exists(caminhoPasta))
                Directory.CreateDirectory(caminhoPasta);

            var caminhoCompleto = Path.Combine(caminhoPasta, nomeFicheiro);

            using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
            {
                await ficheiro.CopyToAsync(stream);
            }

            var urlImagem = $"/imagens/{nomeFicheiro}";
            return Ok(new { Url = urlImagem });
        }

        // --- ÁREA DO FORNECEDOR ---

        [HttpGet("MeusProdutos")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Produto>>> GetMeusProdutos()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            return Ok(await _repository.GetProdutosPorFornecedorAsync(userId));
        }

        [HttpPost]
        [Authorize(Roles = "Fornecedor, Admin")]
        public async Task<ActionResult<Produto>> PostProduto(Produto produto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            produto.FornecedorId = userId;
            produto.Estado = Shared.Enums.EstadoProduto.Pendente;

            // Se não definirem preço de venda, assume o base (regra simples)
            if (produto.PrecoVenda == 0) produto.PrecoVenda = produto.PrecoBase;

            var criado = await _repository.AddProdutoAsync(produto);
            return CreatedAtAction(nameof(GetProduto), new { id = criado.Id }, criado);
        }

        [HttpPut("{id}")] // ATENÇÃO: Adicionei "{id}" aqui porque é boa prática no PUT
        [Authorize(Roles = "Fornecedor, Admin")]
        public async Task<IActionResult> PutProduto(int id, Produto produto)
        {
            if (id != produto.Id) return BadRequest();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verificar se é dono ou admin
            var isDono = await _repository.IsDonoDoProdutoAsync(id, userId);
            if (!isDono && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Se for fornecedor a editar, volta a PENDENTE para aprovação
            if (!User.IsInRole("Admin"))
            {
                produto.Estado = Shared.Enums.EstadoProduto.Pendente;
            }

            var atualizado = await _repository.UpdateProdutoAsync(produto);
            if (atualizado == null) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Fornecedor, Admin")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var isDono = await _repository.IsDonoDoProdutoAsync(id, userId);

            if (!isDono && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var apagou = await _repository.DeleteProdutoAsync(id);
            if (!apagou) return NotFound();

            return NoContent();
        }
    }
}