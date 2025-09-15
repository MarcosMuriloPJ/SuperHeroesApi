using System.ComponentModel.DataAnnotations;

namespace SuperHeroesApi.Domain.Entities
{
  /// <summary>
  /// Representa um superpoder que pode ser atribuído a um herói
  /// </summary>
  public class Superpower
  {
    /// <summary>
    /// Identificador único do superpoder
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome do superpoder
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada do superpoder
    /// </summary>
    [StringLength(250)]
    public string? Description { get; set; }

    /// <summary>
    /// Coleção de heróis associados a este superpoder
    /// </summary>
    public virtual ICollection<HeroSuperpower> HerosSuperpowers { get; set; } = [];
  }
}

