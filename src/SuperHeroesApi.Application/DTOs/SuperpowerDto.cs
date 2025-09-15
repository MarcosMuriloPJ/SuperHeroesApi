namespace SuperHeroesApi.Application.DTOs
{
  /// <summary>
  /// DTO para representação de um superpoder
  /// </summary>
  public class SuperpowerDto
  {
    /// <summary>
    /// Identificador único do superpoder
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome do superpoder
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descrição do superpoder
    /// </summary>
    public string? Description { get; set; }
  }
}

