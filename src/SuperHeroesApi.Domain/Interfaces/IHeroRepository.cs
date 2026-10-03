using SuperHeroesApi.Domain.Entities;

namespace SuperHeroesApi.Domain.Interfaces
{
  /// <summary>
  /// Interface que define as operações de repositório para a entidade Hero
  /// </summary>
  public interface IHeroRepository
  {
    /// <summary>
    /// Obtém um herói pelo seu identificador
    /// </summary>
    /// <param name="id">Identificador do herói</param>
    /// <returns>O herói encontrado ou null se não existir</returns>
    Task<Hero?> GetByIdAsync(int id);

    /// <summary>
    /// Obtém todos os heróis cadastrados
    /// </summary>
    /// <returns>Uma coleção de heróis</returns>
    Task<IEnumerable<Hero>> GetAllAsync();

    /// <summary>
    /// Obtém um herói pelo seu nome de herói
    /// </summary>
    /// <param name="nomeHeroi">Nome de herói a ser pesquisado</param>
    /// <returns>O herói encontrado ou null se não existir</returns>
    Task<Hero?> GetByHeroNameAsync(string nomeHeroi);

    /// <summary>
    /// Adiciona um novo herói
    /// </summary>
    /// <param name="hero">Herói a ser adicionado</param>
    /// <returns>O herói adicionado com seu ID gerado</returns>
    Task<Hero> AddAsync(Hero hero);

    /// <summary>
    /// Atualiza um herói existente
    /// </summary>
    /// <param name="hero">Herói com as informações atualizadas</param>
    /// <returns>O herói atualizado</returns>
    Task<Hero> UpdateAsync(Hero hero);

    /// <summary>
    /// Exclui um herói pelo seu identificador
    /// </summary>
    /// <param name="id">Identificador do herói a ser excluído</param>
    Task DeleteAsync(int id);

    /// <summary>
    /// Verifica se existe um herói com o identificador especificado
    /// </summary>
    /// <param name="id">Identificador do herói</param>
    /// <returns>True se o herói existir, False caso contrário</returns>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Verifica se existe um herói com o nome de herói especificado
    /// </summary>
    /// <param name="nomeHeroi">Nome de herói a ser verificado</param>
    /// <param name="excludeId">ID opcional a ser excluído da verificação (útil em atualizações)</param>
    /// <returns>True se o nome de herói já existir, False caso contrário</returns>
    Task<bool> HeroNameExistsAsync(string nomeHeroi, int? excludeId = null);

    /// <summary>
    /// Obtém heróis de forma paginada, com filtros e ordenação opcionais
    /// </summary>
    /// <param name="page">Número da página (1-based)</param>
    /// <param name="pageSize">Quantidade de itens por página</param>
    /// <param name="name">Filtro opcional por nome civil (contém, case-insensitive)</param>
    /// <param name="heroName">Filtro opcional por nome de herói (contém, case-insensitive)</param>
    /// <param name="superpowerId">Filtro opcional pelo ID de um superpoder que o herói deve possuir</param>
    /// <param name="sortBy">Campo de ordenação: "Name", "HeroName", "Birthdate", "Height" ou "Weight"</param>
    /// <param name="sortDescending">Define se a ordenação é decrescente</param>
    /// <returns>Tupla com os itens da página e a contagem total de itens (sem paginação)</returns>
    Task<(IEnumerable<Hero> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? name,
        string? heroName,
        int? superpowerId,
        string sortBy,
        bool sortDescending);
  }
}

