namespace SuperHeroesApi.Application.DTOs
{
  /// <summary>
  /// Campos disponíveis para ordenação da listagem de heróis
  /// </summary>
  public enum HeroSortBy
  {
    /// <summary>
    /// Ordena pelo nome civil do herói
    /// </summary>
    Name,

    /// <summary>
    /// Ordena pelo nome de super-herói
    /// </summary>
    HeroName,

    /// <summary>
    /// Ordena pela data de nascimento
    /// </summary>
    Birthdate,

    /// <summary>
    /// Ordena pela altura
    /// </summary>
    Height,

    /// <summary>
    /// Ordena pelo peso
    /// </summary>
    Weight
  }

  /// <summary>
  /// Parâmetros de consulta para listagem de heróis: paginação, filtros e ordenação
  /// </summary>
  public class HeroQueryParameters
  {
    private const int MaxPageSize = 50;
    private int _pageSize = 10;
    private int _page = 1;

    /// <summary>
    /// Número da página desejada (1-based). Valores menores que 1 são normalizados para 1.
    /// </summary>
    public int Page
    {
      get => _page;
      set => _page = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Quantidade de itens por página. Limitado a <see cref="MaxPageSize"/> para evitar
    /// consultas custosas.
    /// </summary>
    public int PageSize
    {
      get => _pageSize;
      set => _pageSize = value < 1 ? 1 : Math.Min(value, MaxPageSize);
    }

    /// <summary>
    /// Filtra heróis cujo nome civil contenha o texto informado (case-insensitive)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Filtra heróis cujo nome de herói contenha o texto informado (case-insensitive)
    /// </summary>
    public string? HeroName { get; set; }

    /// <summary>
    /// Filtra heróis que possuam o superpoder com o ID informado
    /// </summary>
    public int? SuperpowerId { get; set; }

    /// <summary>
    /// Campo pelo qual a listagem será ordenada. Padrão: Name
    /// </summary>
    public HeroSortBy SortBy { get; set; } = HeroSortBy.Name;

    /// <summary>
    /// Define se a ordenação deve ser decrescente. Padrão: false (crescente)
    /// </summary>
    public bool SortDescending { get; set; }
  }
}
