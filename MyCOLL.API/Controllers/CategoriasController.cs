using Microsoft.AspNetCore.Mvc;
using MyCOLL.API.Repositories;
using MyCOLL.Shared;

namespace MyCOLL.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepository _repository;

        public CategoriasController(ICategoriaRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
        {
            var categorias = await _repository.GetCategorias();
            return Ok(categorias);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Categoria>> GetCategoria(int id)
        {
            var categoria = await _repository.GetCategoria(id);
            if (categoria == null) return NotFound();
            return Ok(categoria);
        }
    }
}
