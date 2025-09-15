using Microsoft.EntityFrameworkCore;
using SuperHeroesApi.Domain.Entities;
using SuperHeroesApi.Domain.Interfaces;
using SuperHeroesApi.Infrastructure.Data;
using SuperHeroesApi.Infrastructure.Repositories.Extensions;

namespace SuperHeroesApi.Infrastructure.Repositories
{
  /// <summary>
  /// Implementação do repositório para a entidade Hero
  /// </summary>
  public class HeroRepository(SuperDbContext context) : IHeroRepository
  {
    private readonly SuperDbContext _context = context;

    /// <summary>
    /// Obtém um herói pelo seu identificador
    /// </summary>
    /// <param name="id">Identificador do herói</param>
    /// <returns>O herói encontrado ou null se não existir</returns>
    public async Task<Hero?> GetByIdAsync(int id)
    {
      return await _context.Heros
        .Include(h => h.HerosSuperpowers)
          .ThenInclude(hs => hs.Superpower)
        .FirstOrDefaultAsync(h => h.Id == id);
    }

    /// <summary>
    /// Obtém todos os heróis cadastrados
    /// </summary>
    /// <returns>Uma coleção de heróis</returns>
    public async Task<IEnumerable<Hero>> GetAllAsync()
    {
      return await _context.Heros
        .Include(h => h.HerosSuperpowers)
          .ThenInclude(hs => hs.Superpower)
        .ToListAsync();
    }

    /// <summary>
    /// Obtém um herói pelo seu nome de herói
    /// </summary>
    /// <param name="heroName">Nome de herói a ser pesquisado</param>
    /// <returns>O herói encontrado ou null se não existir</returns>
    public async Task<Hero?> GetByHeroNameAsync(string heroName)
    {
      return await _context.Heros
        .Include(h => h.HerosSuperpowers)
          .ThenInclude(hs => hs.Superpower)
        .FirstOrDefaultAsync(h => h.HeroName == heroName);
    }

    /// <summary>
    /// Adiciona um novo herói
    /// </summary>
    /// <param name="hero">Herói a ser adicionado</param>
    /// <returns>O herói adicionado com seu ID gerado</returns>
    public async Task<Hero> AddAsync(Hero hero)
    {
      _context.Heros.Add(hero);
      await _context.SaveChangesAsync();

      return await GetByIdAsync(hero.Id) ?? hero;
    }

    /// <summary>
    /// Atualiza um herói existente
    /// </summary>
    /// <param name="hero">Herói com as informações atualizadas</param>
    /// <returns>O herói atualizado</returns>
    public async Task<Hero> UpdateAsync(Hero hero)
    {
      var existingHero = await _context.Heros
                                    .Include(h => h.HerosSuperpowers)
                                    .FirstOrDefaultAsync(h => h.Id == hero.Id) ?? throw new InvalidOperationException($"Hero with ID {hero.Id} not found for update.");

      _context.Entry(existingHero).CurrentValues.SetValues(hero);

      existingHero.HerosSuperpowers.SyncWith(
          hero.HerosSuperpowers.Select(hs => hs.SuperpowerId),
          hs => hs.SuperpowerId,
          id => new HeroSuperpower { HeroId = existingHero.Id, SuperpowerId = id }
      );

      await _context.SaveChangesAsync();

      return await GetByIdAsync(hero.Id) ?? existingHero;
    }

    /// <summary>
    /// Exclui um herói pelo seu identificador
    /// </summary>
    /// <param name="id">Identificador do herói a ser excluído</param>
    public async Task DeleteAsync(int id)
    {
      var hero = await _context.Heros.FindAsync(id);
      if (hero != null)
      {
        _context.Heros.Remove(hero);
        await _context.SaveChangesAsync();
      }
    }

    /// <summary>
    /// Verifica se existe um herói com o identificador especificado
    /// </summary>
    /// <param name="id">Identificador do herói</param>
    /// <returns>True se o herói existir, False caso contrário</returns>
    public async Task<bool> ExistsAsync(int id)
    {
      return await _context.Heros
        .AsNoTracking()
        .AnyAsync(h => h.Id == id);
    }

    /// <summary>
    /// Verifica se existe um herói com o nome de herói especificado
    /// </summary>
    /// <param name="heroName">Nome de herói a ser verificado</param>
    /// <param name="excludeId">ID opcional de um herói a ser excluído da verificação</param>
    /// <returns>True se o nome de herói já existir, False caso contrário</returns>
    public async Task<bool> HeroNameExistsAsync(string heroName, int? excludeId = null)
    {
      var query = _context.Heros.Where(h => h.HeroName == heroName);

      if (excludeId.HasValue) query = query.Where(h => h.Id != excludeId.Value);

      return await query.AsNoTracking().AnyAsync();
    }
  }
}


