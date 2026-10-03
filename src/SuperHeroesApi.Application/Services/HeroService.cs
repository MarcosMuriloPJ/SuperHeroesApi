using SuperHeroesApi.Application.DTOs;
using SuperHeroesApi.Application.Mappers;
using SuperHeroesApi.Domain.Interfaces;

namespace SuperHeroesApi.Application.Services
{
  /// <summary>
  /// Implementação dos serviços de aplicação para heróis
  /// </summary>
  public class HeroService(IHeroRepository repo, ISuperpowerRepository superpowerRepo) : IHeroService
  {
    private readonly IHeroRepository _repo = repo;
    private readonly ISuperpowerRepository _superpowerRepo = superpowerRepo;

    /// <summary>
    /// Cria um novo herói
    /// </summary>
    /// <param name="createHeroDto">DTO com os dados para criação do herói</param>
    /// <returns>DTO do herói criado</returns>
    /// <exception cref="InvalidOperationException">Lançada quando já existe um herói com o mesmo nome</exception>
    /// <exception cref="ArgumentException">Lançada quando um ou mais superpoderes informados não existem</exception>
    public async Task<HeroDto> CreateAsync(CreateHeroDto createHeroDto)
    {
      var existingHero = await _repo.GetByHeroNameAsync(createHeroDto.HeroName);
      if (existingHero != null) throw new InvalidOperationException($"Já existe um herói com o nome '{createHeroDto.HeroName}'");

      var superpowers = await _superpowerRepo.GetByIdsAsync(createHeroDto.SuperpowersIds);
      if (superpowers.Count() != createHeroDto.SuperpowersIds.Count) throw new ArgumentException("Um ou mais superpoderes informados não existem");

      var hero = HeroMapper.ToEntity(createHeroDto);

      hero.ValidateBusinessRules();

      var createdHero = await _repo.AddAsync(hero);

      return HeroMapper.ToDto(createdHero);
    }

    /// <summary>
    /// Obtém um herói pelo seu identificador
    /// </summary>
    /// <param name="id">Identificador do herói</param>
    /// <returns>DTO do herói encontrado ou null se não existir</returns>
    public async Task<HeroDto?> GetByIdAsync(int id)
    {
      var hero = await _repo.GetByIdAsync(id);
      return hero != null ? HeroMapper.ToDto(hero) : null;
    }

    /// <summary>
    /// Obtém heróis de forma paginada, com suporte a filtros e ordenação
    /// </summary>
    /// <param name="queryParameters">Parâmetros de paginação, filtro e ordenação</param>
    /// <returns>Página de DTOs de heróis</returns>
    public async Task<PagedResultDto<HeroDto>> GetPagedAsync(HeroQueryParameters queryParameters)
    {
      var (items, totalCount) = await _repo.GetPagedAsync(
          queryParameters.Page,
          queryParameters.PageSize,
          queryParameters.Name,
          queryParameters.HeroName,
          queryParameters.SuperpowerId,
          queryParameters.SortBy.ToString(),
          queryParameters.SortDescending);

      return new PagedResultDto<HeroDto>
      {
        Items = items.Select(HeroMapper.ToDto),
        Page = queryParameters.Page,
        PageSize = queryParameters.PageSize,
        TotalCount = totalCount
      };
    }

    /// <summary>
    /// Atualiza um herói existente
    /// </summary>
    /// <param name="id">Identificador do herói a ser atualizado</param>
    /// <param name="updateHeroDto">DTO com os dados para atualização do herói</param>
    /// <returns>DTO do herói atualizado</returns>
    /// <exception cref="ArgumentException">Lançada quando o herói não é encontrado ou quando um ou mais superpoderes informados não existem</exception>
    /// <exception cref="InvalidOperationException">Lançada quando já existe outro herói com o mesmo nome</exception>
    public async Task<HeroDto> UpdateAsync(int id, UpdateHeroDto updateHeroDto)
    {
      _ = await _repo.GetByIdAsync(id) ?? throw new ArgumentException($"Herói com ID {id} não encontrado");

      if (await _repo.HeroNameExistsAsync(updateHeroDto.HeroName, id)) throw new InvalidOperationException($"Já existe outro herói com o nome '{updateHeroDto.HeroName}'");

      var superpowers = await _superpowerRepo.GetByIdsAsync(updateHeroDto.SuperpowersIds);
      if (superpowers.Count() != updateHeroDto.SuperpowersIds.Count) throw new ArgumentException("Um ou mais superpoderes informados não existem");

      var hero = HeroMapper.ToEntity(id, updateHeroDto);

      hero.ValidateBusinessRules();

      var updatedHero = await _repo.UpdateAsync(hero);
      return HeroMapper.ToDto(updatedHero);
    }

    /// <summary>
    /// Exclui um herói pelo seu identificador
    /// </summary>
    /// <param name="id">Identificador do herói a ser excluído</param>
    /// <exception cref="ArgumentException">Lançada quando o herói não é encontrado</exception>
    public async Task DeleteAsync(int id)
    {
      if (!await _repo.ExistsAsync(id)) throw new ArgumentException($"Herói com ID {id} não encontrado");

      await _repo.DeleteAsync(id);
    }
  }
}

