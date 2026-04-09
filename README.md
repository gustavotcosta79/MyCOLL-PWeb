# MyCOLL - Plataforma de Colecionáveis 🪙🏷️

Projeto desenvolvido no âmbito da Unidade Curricular de **Programação Web (2025/2026)** - Tema B.

O **MyCOLL** é uma plataforma distribuída destinada à listagem e venda de artigos de coleção (moedas, selos, pacotes de açúcar, etc.) e respetivos complementos. O sistema é composto por uma interface pública multiplataforma (Web, Android, iOS, macOS, Windows) e um portal de Gestão de Loja (Backoffice) para administração do negócio.

## 🏗️ Estrutura da Solução (Projetos)

A solução foi desenvolvida em C# e .NET, utilizando a seguinte arquitetura:

* **`MyCOLL.API`**: Backend em ASP.NET Core Web API responsável por fornecer os dados e regras de negócio.
* **`MyCOLL.GestaoLoja`**: Aplicação Web (Blazor) de uso exclusivo para administradores e gestores da loja. Permite a gestão de produtos, categorias, encomendas e utilizadores.
* **`MyCOLL.Public`**: Frontend Web público (Blazor) para os clientes explorarem os produtos e fazerem compras.
* **`MyCOLL.Public.Maui`**: Aplicação frontend multiplataforma (.NET MAUI Blazor Hybrid) partilhando a mesma lógica e UI do portal público para dispositivos móveis e desktop.
* **`MyCOLL.RCL` (Razor Class Library)**: Biblioteca de componentes visuais partilhados entre as várias interfaces frontend.
* **`MyCOLL.Shared`**: Biblioteca de classes partilhadas (Modelos, DTOs, Enums como `Produto`, `Encomenda`, `Categoria`) partilhada entre o frontend e a API.
* **`MyCOLL.Data`**: Camada de acesso a dados contendo o `ApplicationDbContext`, modelos do Identity (`ApplicationUser`) e as Migrações do Entity Framework Core.

## 🚀 Tecnologias Utilizadas

* **Linguagem:** C#
* **Frameworks:** .NET 8 / .NET 9
* **Frontend:** Blazor (Web) e .NET MAUI (Mobile/Desktop)
* **Backend:** ASP.NET Core Web API
* **Base de Dados:** Entity Framework Core (SQL Server / SQLite)
* **Autenticação:** ASP.NET Core Identity / JWT Tokens

## ⚙️ Como Configurar e Executar

1. Clona este repositório para a tua máquina local.
2. Abre a solução `MyCOLL_Solution.sln` no Visual Studio 2022.
3. Certifica-te de que tens os workloads de **ASP.NET e desenvolvimento Web** e **.NET Multi-platform App UI (MAUI)** instalados.
4. Define os projetos `MyCOLL.API`, `MyCOLL.GestaoLoja` e o frontend desejado (`MyCOLL.Public` ou `MyCOLL.Public.Maui`) para arrancarem em simultâneo (Startup Projects).
5. **Base de Dados:**
   * Abre a "Package Manager Console" (Consola do Gestor de Pacotes).
   * Define o `MyCOLL.Data` como *Default project*.
   * Executa o comando `Update-Database` para aplicar as migrações e popular a base de dados inicial.
6. Corre a aplicação (F5).

## 👥 Autores

* [Gustavo Costa] - [2023145800]
* [Duarte Santos] - [2022149622]
