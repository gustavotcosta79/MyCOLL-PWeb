using MyCOLL.Shared;

namespace MyCOLL.API.Repositories
{
    public interface ICategoriaRepository
    {
        Task<IEnumerable<Categoria>> GetCategorias();
        Task<Categoria> GetCategoria(int id);
    }
}
