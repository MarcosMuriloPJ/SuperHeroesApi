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
    public async Task<(IEnumerable<Hero> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? name,
        string? heroName,
        int? superpowerId,
        string sortBy,
        bool sortDescending)
    {
      var query = _context.Heros
        .Include(h => h.HerosSuperpowers)
          .ThenInclude(hs => hs.Superpower)
        .AsQueryable();

      if (!string.IsNullOrWhiteSpace(name)) query = query.Where(h => h.Name.Contains(name));

      if (!string.IsNullOrWhiteSpace(heroName)) query = query.Where(h => h.HeroName.Contains(heroName));

      if (superpowerId.HasValue) query = query.Where(h => h.HerosSuperpowers.Any(hs => hs.SuperpowerId == superpowerId.Value));

      query = ApplySorting(query, sortBy, sortDescending);

      var totalCount = await query.CountAsync();

      var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

      return (items, totalCount);
    }

    /// <summary>
    /// Aplica ordenação dinâmica sobre a consulta de heróis de acordo com o campo informado
    /// </summary>
    private static IQueryable<Hero> ApplySorting(IQueryable<Hero> query, string sortBy, bool sortDescending)
    {
      return sortBy switch
      {
        nameof(Hero.HeroName) => sortDescending ? query.OrderByDescending(h => h.HeroName) : query.OrderBy(h => h.HeroName),
        nameof(Hero.Birthdate) => sortDescending ? query.OrderByDescending(h => h.Birthdate) : query.OrderBy(h => h.Birthdate),
        nameof(Hero.Height) => sortDescending ? query.OrderByDescending(h => h.Height) : query.OrderBy(h => h.Height),
        nameof(Hero.Weight) => sortDescending ? query.OrderByDescending(h => h.Weight) : query.OrderBy(h => h.Weight),
        _ => sortDescending ? query.OrderByDescending(h => h.Name) : query.OrderBy(h => h.Name),
      };
    }
  }
}


