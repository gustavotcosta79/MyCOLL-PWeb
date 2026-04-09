using MyCOLL.Shared;

namespace MyCOLL.API.Repositories
{
    public interface IProdutoRepository
    {
        Task<IEnumerable<Produto>> GetProdutos();
        Task<Produto> GetProduto(int id);
        Task<IEnumerable<Produto>> GetProdutosPorCategoria(int categoriaId);

        //métodos para o fornecedor/admin
        Task<IEnumerable<Produto>> GetProdutosPorFornecedorAsync(string fornecedorId);
        Task<Produto> AddProdutoAsync(Produto produto);
        Task <Produto?> UpdateProdutoAsync (Produto produto);
        Task<bool> DeleteProdutoAsync(int id);
        Task<bool> IsDonoDoProdutoAsync(int produtoId, string userId);

    }   
}
