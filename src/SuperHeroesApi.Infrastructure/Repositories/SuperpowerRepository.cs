using Microsoft.EntityFrameworkCore;
using SuperHeroesApi.Domain.Entities;
using SuperHeroesApi.Domain.Interfaces;
using SuperHeroesApi.Infrastructure.Data;

namespace SuperHeroesApi.Infrastructure.Repositories
{
  /// <summary>
  /// Implementação do repositório para a entidade Superpower
  /// </summary>
  public class SuperpowerRepository(SuperDbContext context) : ISuperpowerRepository
  {
    private readonly SuperDbContext _context = context;

    /// <summary>
    /// Obtém todos os superpoderes cadastrados
    /// </summary>
    /// <returns>Uma coleção de superpoderes</returns>
    public async Task<IEnumerable<Superpower>> GetAllAsync()
    {
      return await _context.Superpowers
        .AsNoTracking()
        .ToListAsync();
    }

    /// <summary>
    /// Obtém um superpoder pelo seu identificador
    /// </summary>
    /// <param name="id">Identificador do superpoder</param>
    /// <returns>O superpoder encontrado ou null se não existir</returns>
    public async Task<Superpower?> GetByIdAsync(int id)
    {
      return await _context.Superpowers
        .AsNoTracking()
        .SingleOrDefaultAsync(s => s.Id == id);
    }

    /// <summary>
    /// Obtém múltiplos superpoderes pelos seus identificadores
    /// </summary>
    /// <param name="ids">Coleção de identificadores de superpoderes</param>
    /// <returns>Coleção de superpoderes encontrados</returns>
    public async Task<IEnumerable<Superpower>> GetByIdsAsync(IEnumerable<int> ids)
    {
      return await _context.Superpowers
        .AsNoTracking()
        .Where(s => ids.Contains(s.Id))
        .ToListAsync();
    }
  }
}

