namespace SuperHeroesApi.Application.DTOs
{
  /// <summary>
  /// DTO para representação de um herói
  /// </summary>
  public class HeroDto
  {
    /// <summary>
    /// Identificador único do herói
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome civil do herói
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Nome de super-herói
    /// </summary>
    public string HeroName { get; set; } = string.Empty;

    /// <summary>
    /// Data de nascimento do herói
    /// </summary>
    public DateTime Birthdate { get; set; }

    /// <summary>
    /// Altura do herói em metros
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Peso do herói em quilogramas
    /// </summary>
    public double Weight { get; set; }

    /// <summary>
    /// Lista de superpoderes do herói
    /// </summary>
    public List<SuperpowerDto> Superpowers { get; set; } = [];
  }
}

