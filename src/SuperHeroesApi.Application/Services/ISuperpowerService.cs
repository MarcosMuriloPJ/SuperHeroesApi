using SuperHeroesApi.Application.DTOs;

namespace SuperHeroesApi.Application.Services
{
  /// <summary>
  /// Interface que define os serviços de aplicação para superpoderes
  /// </summary>
  public interface ISuperpowerService
  {
    /// <summary>
    /// Obtém todos os superpoderes disponíveis
    /// </summary>
    /// <returns>Uma coleção de DTOs de superpoderes</returns>
    Task<IEnumerable<SuperpowerDto>> GetAllAsync();
  }
}

