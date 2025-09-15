using SuperHeroesApi.Application.DTOs;
using SuperHeroesApi.Application.Mappers;
using SuperHeroesApi.Domain.Interfaces;

namespace SuperHeroesApi.Application.Services
{
  /// <summary>
  /// Implementação dos serviços de aplicação para superpoderes
  /// </summary>
  public class SuperpowerService(ISuperpowerRepository repo) : ISuperpowerService
  {
    private readonly ISuperpowerRepository _repo = repo;

    /// <summary>
    /// Obtém todos os superpoderes disponíveis
    /// </summary>
    /// <returns>Uma coleção de DTOs de superpoderes</returns>
    public async Task<IEnumerable<SuperpowerDto>> GetAllAsync()
    {
      var superpowers = await _repo.GetAllAsync();
      return superpowers.Select(SuperpowerMapper.ToDto);
    }
  }
}

