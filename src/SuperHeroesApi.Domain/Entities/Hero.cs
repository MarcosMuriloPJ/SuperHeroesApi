using System.ComponentModel.DataAnnotations;

namespace SuperHeroesApi.Domain.Entities
{
  /// <summary>
  /// Representa a entidade de um Super-Herói no sistema
  /// </summary>
  public class Hero
  {
    /// <summary>
    /// Identificador único do herói
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome civil do herói
    /// </summary>
    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Nome de super-herói
    /// </summary>
    [Required]
    [StringLength(120)]
    public string HeroName { get; set; } = string.Empty;

    /// <summary>
    /// Data de nascimento do herói
    /// </summary>
    [Required]
    public DateTime Birthdate { get; set; }

    /// <summary>
    /// Altura do herói em metros
    /// </summary>
    [Required]
    public double Height { get; set; }

    /// <summary>
    /// Peso do herói em quilogramas
    /// </summary>
    [Required]
    public double Weight { get; set; }

    /// <summary>
    /// Coleção de superpoderes associados ao herói
    /// </summary>
    public virtual ICollection<HeroSuperpower> HerosSuperpowers { get; set; } = [];

    /// <summary>
    /// Valida as regras de negócio para a entidade Hero
    /// </summary>
    /// <exception cref="ArgumentException">Lançada quando alguma regra de negócio é violada</exception>
    public void ValidateBusinessRules()
    {
      if (string.IsNullOrWhiteSpace(Name))
        throw new ArgumentException("Nome é obrigatório");

      if (string.IsNullOrWhiteSpace(HeroName))
        throw new ArgumentException("Nome do Herói é obrigatório");

      if (Birthdate > DateTime.Now)
        throw new ArgumentException("Data de nascimento não pode ser no futuro");

      if (Height <= 0)
        throw new ArgumentException("Altura deve ser maior que zero");

      if (Weight <= 0)
        throw new ArgumentException("Peso deve ser maior que zero");
    }

    public void UpdateDetails(string name, string heroName, DateTime birthdate, double height, double weight)
    {
      Name = name;
      HeroName = heroName;
      Birthdate = birthdate;
      Height = height;
      Weight = weight;

      ValidateBusinessRules();
    }
  }
}

