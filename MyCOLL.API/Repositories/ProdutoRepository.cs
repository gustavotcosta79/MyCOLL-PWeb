using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using MyCOLL.Data;
using MyCOLL.Shared;
using MyCOLL.Shared.Enums;

namespace MyCOLL.API.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly ApplicationDbContext _context;

        public ProdutoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Produto>> GetProdutos()
        {
            return await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoDisponibilizacao)
                .Where(p => p.Estado == EstadoProduto.Ativo) // Apenas ativos
                .ToListAsync();
        }

        public async Task<Produto> GetProduto(int id)
        {
            return await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoDisponibilizacao)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Produto>> GetProdutosPorCategoria(int categoriaId)
        {
            return await _context.Produtos
                .Include(p => p.Categoria)
                .Where(p => p.CategoriaId == categoriaId && p.Estado == EstadoProduto.Ativo)
                .ToListAsync();
        }

        //Métodos p o fornecedor (gestão)

        public async Task <IEnumerable<Produto>> GetProdutosPorFornecedorAsync (string fornecedorId)
        {
            return await _context.Produtos
                .Include(p => p.Categoria)
                .Where(p => p.FornecedorId == fornecedorId)
                .ToListAsync();
        }

        public async Task<Produto>AddProdutoAsync (Produto produto)
        {
            _context.Produtos .Add(produto);
            await _context.SaveChangesAsync();
            return produto;
        }

        public async Task <Produto?> UpdateProdutoAsync (Produto produto)
        {
            var existente = await _context.Produtos.FindAsync (produto.Id);
            if (existente == null) return null;

            existente.Nome = produto.Nome;
            existente.Descricao = produto.Descricao;
            existente.PrecoBase = produto.PrecoBase; 
            existente.PrecoVenda = produto.PrecoVenda;
            existente.Stock = produto.Stock;
            existente.CategoriaId = produto.CategoriaId;   
            existente.ImagemUrl = produto.ImagemUrl;
            existente.Estado = produto.Estado;

            await _context.SaveChangesAsync();
            return existente;
        }

        public async Task<bool> DeleteProdutoAsync (int id)
        {
            var produto = await _context.Produtos.FindAsync (id);
            if (produto == null) return false;  

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task <bool> IsDonoDoProdutoAsync (int produtoId, string  userId)
        {
            return await _context.Produtos.AnyAsync(p => p.Id == produtoId && p.FornecedorId == userId);
        }

       
    }
}
