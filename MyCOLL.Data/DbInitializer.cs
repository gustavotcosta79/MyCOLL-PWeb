using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MyCOLL.Shared;
using MyCOLL.Shared.Enums;
using System.Text.RegularExpressions;

namespace MyCOLL.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider, string webRootPath)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Garantir que a BD existe
            context.Database.EnsureDeleted(); // <--- ADICIONAR ISTO PARA FAZER RESET TOTAL A CADA ARRANQUE (APENAS PARA DESENVOLVIMENTO!)
            context.Database.EnsureCreated();

            // Verifica se já existem produtos para não duplicar sempre que corres
            bool seedBusiness = !context.Produtos.Any();

            // =================================================================
            // 2. CRIAR ROLES (Perfis)
            // =================================================================
            string[] roles = { "Admin", "Funcionario", "Cliente", "Fornecedor" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // =================================================================
            // 3. CRIAR UTILIZADORES
            // =================================================================
            
            // Administrador
            var adminUser = await EnsureUser(userManager, "admin@mycoll.pt", "Administrador Principal", TipoUtilizador.Admin, "Admin");

            // Cliente
            var clienteUser = await EnsureUser(userManager, "cliente@mycoll.pt", "João Cliente", TipoUtilizador.Cliente, "Cliente");

            // Fornecedores
            var forn1User = await EnsureUser(userManager, "fornecedor1@mycoll.pt", "Numismática Antiga Lda", TipoUtilizador.Fornecedor, "Fornecedor");
            var forn2User = await EnsureUser(userManager, "fornecedor2@mycoll.pt", "Mundo dos Selos", TipoUtilizador.Fornecedor, "Fornecedor");

            // =================================================================
            // 4. DADOS DE NEGÓCIO (Categorias e Produtos)
            // =================================================================

            // Declarar categorias fora do if para usar depois
            Categoria catNumismatica = null, catFilatelia = null, catOutros = null;
            Categoria catPortugal = null, catEuro = null, catTematicos = null;
            Categoria catMonarquia = null, catRepublica = null;

            if (!context.Categorias.Any())
            {
                // -- Modos de Disponibilização --
                var modos = new[]
                {
                    new ModoDisponibilizacao { Nome = "Venda", Detalhe = "Compra imediata", Ativo = true },
                    new ModoDisponibilizacao { Nome = "Listagem", Detalhe = "Exibição (Museu)", Ativo = true },
                    new ModoDisponibilizacao { Nome = "Leilão", Detalhe = "Licitação base", Ativo = true }
                };
                context.ModosDisponibilizacao.AddRange(modos);
                await context.SaveChangesAsync();

                // -- Categorias --
                catNumismatica = new Categoria { Nome = "Numismática (Moedas)", Nivel = 1, Ativa = true };
                catFilatelia = new Categoria { Nome = "Filatelia (Selos)", Nivel = 1, Ativa = true };
                catOutros = new Categoria { Nome = "Outros Colecionáveis", Nivel = 1, Ativa = true };
                context.Categorias.AddRange(catNumismatica, catFilatelia, catOutros);
                await context.SaveChangesAsync();

                catPortugal = new Categoria { Nome = "Portugal", Nivel = 2, Ativa = true, CategoriaPaiId = catNumismatica.Id };
                catEuro = new Categoria { Nome = "União Europeia", Nivel = 2, Ativa = true, CategoriaPaiId = catNumismatica.Id };
                catTematicos = new Categoria { Nome = "Temáticos / Natureza", Nivel = 2, Ativa = true, CategoriaPaiId = catFilatelia.Id };
                context.Categorias.AddRange(catPortugal, catEuro, catTematicos);
                await context.SaveChangesAsync();

                catMonarquia = new Categoria { Nome = "Monarquia", Nivel = 3, Ativa = true, CategoriaPaiId = catPortugal.Id };
                catRepublica = new Categoria { Nome = "República", Nivel = 3, Ativa = true, CategoriaPaiId = catPortugal.Id };
                context.Categorias.AddRange(catMonarquia, catRepublica);
                await context.SaveChangesAsync();
            }
            else
            {
                // Se já existem, vamos buscá-las para usar na criação de produtos
                catNumismatica = await context.Categorias.FirstOrDefaultAsync(c => c.Nome.Contains("Numismática"));
                catFilatelia = await context.Categorias.FirstOrDefaultAsync(c => c.Nome.Contains("Filatelia"));
                catOutros = await context.Categorias.FirstOrDefaultAsync(c => c.Nome.Contains("Outros"));
                
                // Carregar subcategorias (exemplo rápido)
                catPortugal = await context.Categorias.FirstOrDefaultAsync(c => c.Nome == "Portugal");
                catEuro = await context.Categorias.FirstOrDefaultAsync(c => c.Nome == "União Europeia");
                catTematicos = await context.Categorias.FirstOrDefaultAsync(c => c.Nome.Contains("Temáticos"));
                catMonarquia = await context.Categorias.FirstOrDefaultAsync(c => c.Nome == "Monarquia");
                catRepublica = await context.Categorias.FirstOrDefaultAsync(c => c.Nome == "República");
            }

            // =================================================================
            // 5. POPULAR PRODUTOS COM IMAGENS REAIS
            // =================================================================
            if (seedBusiness)
            {
                // webRootPath vem como: ...\MyCOLL.GestaoLoja\wwwroot
                // Nós queremos ir para: ...\MyCOLL.API\wwwroot\imagens

                string imageFolder = "";

                // Sobe dois níveis (sai do wwwroot, sai do GestaoLoja, chega à Solução)
                var solutionRoot = Directory.GetParent(webRootPath)?.Parent?.FullName;

                if (solutionRoot != null)
                {
                    // Constrói o caminho direto para a API
                    imageFolder = Path.Combine(solutionRoot, "MyCOLL.API", "wwwroot", "imagens");
                    Console.WriteLine($"[SEED] Caminho forçado para API: {imageFolder}");
                }

                // Verifica se existe, senão tenta "images"
                if (!Directory.Exists(imageFolder) && solutionRoot != null)
                {
                    imageFolder = Path.Combine(solutionRoot, "MyCOLL.API", "wwwroot", "images");
                }

                List<string> imageFiles = new List<string>();
                if (Directory.Exists(imageFolder))
                {
                    imageFiles = Directory.GetFiles(imageFolder)
                                          .Where(f => Regex.IsMatch(f, @"\.(jpg|jpeg|png|webp)$", RegexOptions.IgnoreCase))
                                          .ToList();
                }

                bool hasImages = imageFiles.Count > 0;
                var random = new Random();
                var listaProdutos = new List<Produto>();
                var listaFornecedores = new[] { forn1User, forn2User };
                var modosDb = context.ModosDisponibilizacao.ToList();

                for (int i = 0; i < 42; i++)
                {
                    string imagePath = "";
                    string fileName = "";
                    Categoria categoriaAlvo = catOutros;

                    if (hasImages)
                    {
                        string fullPath = imageFiles[random.Next(imageFiles.Count)];
                        fileName = Path.GetFileName(fullPath).ToLower();

                        // IMPORTANTE: O caminho na BD é sempre relativo à wwwroot da API (/imagens/...)
                        imagePath = $"/imagens/{fileName}";

                        if (fileName.Contains("moeda"))
                        {
                            int r = random.Next(3);
                            if (r == 0) categoriaAlvo = catEuro;
                            else if (r == 1) categoriaAlvo = catMonarquia;
                            else categoriaAlvo = catRepublica;
                        }
                        else if (fileName.Contains("selo")) categoriaAlvo = catTematicos;
                        else if (fileName.Contains("outros")) categoriaAlvo = catOutros;
                    }
                    else
                    {
                        imagePath = "https://placehold.co/400x300?text=Sem+Imagem";
                        fileName = $"Produto {i}";
                        categoriaAlvo = catNumismatica;
                    }

                    string nomeCriativo = GerarNomeCriativo(fileName);
                    decimal precoBase = random.Next(5, 500);
                    decimal margem = random.Next(10, 60);

                    listaProdutos.Add(new Produto
                    {
                        Nome = nomeCriativo,
                        Descricao = $"Item genuíno de coleção. {nomeCriativo}.",
                        PrecoBase = precoBase,
                        MargemLucro = margem,
                        PrecoVenda = Math.Round(precoBase * (1 + margem / 100m), 2),
                        Stock = random.Next(1, 10),
                        CategoriaId = categoriaAlvo.Id,
                        ModoDisponibilizacaoId = modosDb[random.Next(modosDb.Count)].Id,
                        Estado = EstadoProduto.Ativo,
                        FornecedorId = listaFornecedores[random.Next(listaFornecedores.Length)].Id,
                        ImagemUrl = imagePath
                    });
                }

                context.Produtos.AddRange(listaProdutos);
                await context.SaveChangesAsync();

                // Encomenda Exemplo
                if (listaProdutos.Any())
                {
                    var p = listaProdutos.First();
                    var enc = new Encomenda { ClienteId = clienteUser.Id, DataEncomenda = DateTime.Now.AddDays(-5), Estado = EstadoEncomenda.Enviada, ValorTotal = p.PrecoVenda };
                    context.Encomendas.Add(enc);
                    await context.SaveChangesAsync();
                    context.DetalhesEncomenda.Add(new DetalheEncomenda { EncomendaId = enc.Id, ProdutoId = p.Id, Quantidade = 1, PrecoUnitario = p.PrecoVenda });
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task<ApplicationUser> EnsureUser(UserManager<ApplicationUser> mgr, string email, string nome, TipoUtilizador tipo, string role)
        {
            var user = await mgr.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser { UserName = email, Email = email, Nome = nome, EmailConfirmed = true, Tipo = tipo, EstadoConta = EstadoConta.Ativo, NIF = "999999990", Morada = "Rua Gerada" };
                await mgr.CreateAsync(user, "Pass123!");
                await mgr.AddToRoleAsync(user, role);
            }
            return user;
        }

        private static string GerarNomeCriativo(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName).Replace("moeda-", "").Replace("selo-", "").Replace("outros-", "").Replace("-", " ").Replace("_", " ");
            var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string cleanName = string.Join(" ", words.Select(w => char.ToUpper(w[0]) + (w.Length > 1 ? w.Substring(1) : "")));
            string[] sufixos = { "Raro", "Antigo", "Certificado", "Séc. XIX", "Comemorativo", "Limitado" };
            var rnd = new Random();
            return cleanName.Length < 3 ? $"Item Coleção {rnd.Next(100)}" : $"{cleanName} - {sufixos[rnd.Next(sufixos.Length)]}";
        }
    }
}