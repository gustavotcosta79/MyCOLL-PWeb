using Microsoft.EntityFrameworkCore;
using MyCOLL.Data;
using MyCOLL.Shared;

namespace MyCOLL.API.Repositories
{
    public class EncomendaRepository : IEncomendaRepository
    {
        private readonly ApplicationDbContext  _context;

        public EncomendaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Encomenda> CreateEncomenda(Encomenda encomenda)
        {
            // Força a data para o momento atual no servidor (segurança)
            encomenda.DataEncomenda = DateTime.Now;
            encomenda.Estado = MyCOLL.Shared.Enums.EstadoEncomenda.Pendente;
            foreach (var item in encomenda.Detalhes)
            {
                
                var produto = await _context.Produtos.FindAsync(item.ProdutoId);

                if (produto == null)
                {
                    throw new Exception($"Produto com ID {item.ProdutoId} não encontrado.");
                }

                // Verifica se há stock suficiente
                if (produto.Stock < item.Quantidade)
                {
                    throw new Exception($"Stock insuficiente para o produto '{produto.Nome}'. Restam apenas {produto.Stock}.");
                }

                // Abate o stock
                produto.Stock -= item.Quantidade;
            }

            _context.Encomendas.Add(encomenda);
            await _context.SaveChangesAsync(); // Grava a encomenda E as alterações de stock numa só transação
            return encomenda;
        }

        public async Task<Encomenda> GetEncomenda(int id)
        {
            return await _context.Encomendas
                .Include(e => e.Detalhes)
                    .ThenInclude(d => d.Produto) // Para saber o nome do produto na linha
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Encomenda>> GetEncomendasPorCliente(string clienteId)
        {
            return await _context.Encomendas
                .Include(e => e.Detalhes)
                    .ThenInclude(d => d.Produto)
                .Where(e => e.ClienteId == clienteId)
                .OrderByDescending(e => e.DataEncomenda) // Mais recentes primeiro
                .ToListAsync();
        }

        public async Task <IEnumerable<DetalheEncomenda>> GetVendasPorFornecedor (string fornecedorId)
        {
            return await _context.DetalhesEncomenda
                .Include(d => d.Produto)
                .Include (d => d.Encomenda)
                .Where(d => d.Produto.FornecedorId == fornecedorId)
                .OrderByDescending(d => d.Encomenda.DataEncomenda)
                .ToListAsync();
        }
    }
}
