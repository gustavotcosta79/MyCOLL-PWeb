using MyCOLL.Shared;

namespace MyCOLL.API.Repositories
{
    public interface IEncomendaRepository
    {
        // Criar uma nova encomenda (Checkout)
        Task<Encomenda> CreateEncomenda(Encomenda encomenda);

        // Obter histórico de um cliente específico
        Task<IEnumerable<Encomenda>> GetEncomendasPorCliente(string clienteId);

        // Obter detalhes de uma encomenda
        Task<Encomenda> GetEncomenda(int id);


        Task<IEnumerable<DetalheEncomenda>> GetVendasPorFornecedor(string fornecedorId); 
    }
}
