using System.ComponentModel.DataAnnotations;

namespace SuperHeroesApi.Domain.Entities
{
  /// <summary>
  /// Entidade de associação que representa o relacionamento muitos-para-muitos entre Hero e Superpower
  /// </summary>
  public class HeroSuperpower
  {
    /// <summary>
    /// Identificador do herói na relação
    /// </summary>
    [Required]
    public int HeroId { get; set; }

    /// <summary>
    /// Identificador do superpoder na relação
    /// </summary>
    [Required]
    public int SuperpowerId { get; set; }

    /// <summary>
    /// Referência para a entidade Hero
    /// </summary>
    public virtual Hero Hero { get; set; } = null!;

    /// <summary>
    /// Referência para a entidade Superpower
    /// </summary>
    public virtual Superpower Superpower { get; set; } = null!;
  }
}

