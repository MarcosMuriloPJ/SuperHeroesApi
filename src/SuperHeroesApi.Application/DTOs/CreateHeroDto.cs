using System.ComponentModel.DataAnnotations;

namespace SuperHeroesApi.Application.DTOs
{
  /// <summary>
  /// DTO para criação de um novo herói
  /// </summary>
  public class CreateHeroDto
  {
    /// <summary>
    /// Nome civil do herói
    /// </summary>
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(120, ErrorMessage = "Nome deve ter no máximo 120 caracteres")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Nome de super-herói
    /// </summary>
    [Required(ErrorMessage = "Nome do Herói é obrigatório")]
    [StringLength(120, ErrorMessage = "Nome do Herói deve ter no máximo 120 caracteres")]
    public string HeroName { get; set; } = string.Empty;

    /// <summary>
    /// Data de nascimento do herói
    /// </summary>
    [Required(ErrorMessage = "Data de nascimento é obrigatória")]
    public DateTime Birthdate { get; set; }

    /// <summary>
    /// Altura do herói em metros
    /// </summary>
    [Required(ErrorMessage = "Altura é obrigatória")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Altura deve ser maior que zero")]
    public double Height { get; set; }

    /// <summary>
    /// Peso do herói em quilogramas
    /// </summary>
    [Required(ErrorMessage = "Peso é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Peso deve ser maior que zero")]
    public double Weight { get; set; }

    /// <summary>
    /// Lista de IDs dos superpoderes do herói
    /// </summary>
    [Required(ErrorMessage = "Pelo menos um superpoder deve ser selecionado")]
    [MinLength(1, ErrorMessage = "Pelo menos um superpoder deve ser selecionado")]
    public List<int> SuperpowersIds { get; set; } = [];
  }
}

