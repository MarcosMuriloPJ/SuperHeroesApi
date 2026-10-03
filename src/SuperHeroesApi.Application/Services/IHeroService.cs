using SuperHeroesApi.Application.DTOs;

namespace SuperHeroesApi.Application.Services
{
  /// <summary>
  /// Define operações de serviço para a entidade Hero
  /// </summary>
  public interface IHeroService
  {
    /// <summary>
    /// Cria um novo herói
    /// </summary>
    /// <param name="createHeroDto">DTO com os dados para criação do herói</param>
    /// <returns>DTO do herói criado</returns>
    Task<HeroDto> CreateAsync(CreateHeroDto createHeroDto);

    /// <summary>
    /// Obtém um herói pelo seu identificador
    /// </summary>
    /// <param name="id">Identificador do herói</param>
    /// <returns>DTO do herói encontrado ou null se não existir</returns>
    Task<HeroDto?> GetByIdAsync(int id);

    /// <summary>
    /// Obtém heróis de forma paginada, com suporte a filtros e ordenação
    /// </summary>
    /// <param name="queryParameters">Parâmetros de paginação, filtro e ordenação</param>
    /// <returns>Página de DTOs de heróis</returns>
    Task<PagedResultDto<HeroDto>> GetPagedAsync(HeroQueryParameters queryParameters);

    /// <summary>
    /// Atualiza um herói existente
    /// </summary>
    /// <param name="id">Identificador do herói a ser atualizado</param>
    /// <param name="updateHeroDto">DTO com os dados para atualização do herói</param>
    /// <returns>DTO do herói atualizado</returns>
    Task<HeroDto> UpdateAsync(int id, UpdateHeroDto updateHeroDto);

    /// <summary>
    /// Exclui um herói pelo seu identificador
    /// </summary>
    /// <param name="id">Identificador do herói a ser excluído</param>
    Task DeleteAsync(int id);
  }
}

