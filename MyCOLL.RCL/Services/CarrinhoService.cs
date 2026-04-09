using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyCOLL.Shared;

using Blazored.LocalStorage;

namespace MyCOLL.RCL.Services
{
    public class CarrinhoItem
    {
        public Produto Produto { get; set; } = new();
        public int Quantidade { get; set; }
    }

    public class CarrinhoService
    {
        private readonly ILocalStorageService _localStorage;
        private List<CarrinhoItem> _itens = new();
        
        // Evento para avisar o Menu que o carrinho mudou
        public event Action? OnChange;

        public CarrinhoService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        // Carregar do disco ao iniciar
        public async Task Inicializar()
        {
            var guardado = await _localStorage.GetItemAsync<List<CarrinhoItem>>("carrinho");
            if (guardado != null) _itens = guardado;
            NotifyStateChanged();
        }

        public async Task Adicionar(Produto produto, int qtd = 1)
        {
            var itemExistente = _itens.FirstOrDefault(i => i.Produto.Id == produto.Id);

            if (itemExistente != null)
            {
                itemExistente.Quantidade += qtd;
            }
            else
            {
                _itens.Add(new CarrinhoItem { Produto = produto, Quantidade = qtd });
            }

            await Guardar();
        }

        public async Task Remover(Produto produto)
        {
            var item = _itens.FirstOrDefault(i => i.Produto.Id == produto.Id);
            if (item != null)
            {
                _itens.Remove(item);
                await Guardar();
            }
        }

        public async Task Limpar()
        {
            _itens.Clear();
            await Guardar();
        }

        public List<CarrinhoItem> ObterItens() => _itens;
        
        public int ContarItens() => _itens.Sum(i => i.Quantidade);
        
        public decimal Total() => _itens.Sum(i => i.Produto.PrecoVenda * i.Quantidade);

        private async Task Guardar()
        {
            await _localStorage.SetItemAsync("carrinho", _itens);
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}