using SuperHeroesApi.Domain.Entities;

namespace SuperHeroesApi.Domain.Interfaces
{
  /// <summary>
  /// Interface que define as operações de repositório para a entidade Superpower
  /// </summary>
  public interface ISuperpowerRepository
  {
    /// <summary>
    /// Obtém todos os superpoderes cadastrados
    /// </summary>
    /// <returns>Uma coleção de superpoderes</returns>
    Task<IEnumerable<Superpower>> GetAllAsync();

    /// <summary>
    /// Obtém um superpoder pelo seu identificador
    /// </summary>
    /// <param name="id">Identificador do superpoder</param>
    /// <returns>O superpoder encontrado ou null se não existir</returns>
    Task<Superpower?> GetByIdAsync(int id);

    /// <summary>
    /// Obtém múltiplos superpoderes pelos seus identificadores
    /// </summary>
    /// <param name="ids">Coleção de identificadores de superpoderes</param>
    /// <returns>Coleção de superpoderes encontrados</returns>
    Task<IEnumerable<Superpower>> GetByIdsAsync(IEnumerable<int> ids);
  }
}

