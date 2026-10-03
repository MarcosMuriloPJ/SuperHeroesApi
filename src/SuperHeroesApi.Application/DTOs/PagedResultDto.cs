namespace SuperHeroesApi.Application.DTOs
{
  /// <summary>
  /// Representa um resultado paginado genérico
  /// </summary>
  /// <typeparam name="T">Tipo dos itens contidos na página</typeparam>
  public class PagedResultDto<T>
  {
    /// <summary>
    /// Itens da página atual
    /// </summary>
    public IEnumerable<T> Items { get; set; } = [];

    /// <summary>
    /// Número da página atual (1-based)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Quantidade de itens por página
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Quantidade total de itens (sem considerar a paginação)
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Quantidade total de páginas disponíveis
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
  }
}
