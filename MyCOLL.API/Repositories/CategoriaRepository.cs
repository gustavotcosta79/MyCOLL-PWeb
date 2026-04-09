using Microsoft.EntityFrameworkCore;
using MyCOLL.Data;
using MyCOLL.Shared;

namespace MyCOLL.API.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoriaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Categoria>> GetCategorias()
        {
            
            return await _context.Categorias.Where(c => c.Ativa)
            .OrderBy(c => c.Nome)
            .Include(c => c.SubCategorias) 
            .OrderBy(c => c.Nome)
            .ToListAsync();
        }



        public async Task<Categoria> GetCategoria(int id)
        {
            return await _context.Categorias.FindAsync(id);
        }
    }
}
