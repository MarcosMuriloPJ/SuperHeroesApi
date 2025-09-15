using SuperHeroesApi.Domain.Entities;

namespace SuperHeroesApi.Tests
{
  public class HeroTests
  {
    [Fact]
    public void Hero_ValidateBusinessRules_ShouldThrowException_WhenNomeIsEmpty()
    {
      // Arrange
      var hero = new Hero
      {
        Name = "",
        HeroName = "Superman",
        Birthdate = DateTime.Now.AddYears(-30),
        Height = 1.85,
        Weight = 80.0
      };

      // Act & Assert
      Assert.Throws<ArgumentException>(() => hero.ValidateBusinessRules());
    }

    [Fact]
    public void Hero_ValidateBusinessRules_ShouldThrowException_WhenHeroNameIsEmpty()
    {
      // Arrange
      var hero = new Hero
      {
        Name = "Clark Kent",
        HeroName = "",
        Birthdate = DateTime.Now.AddYears(-30),
        Height = 1.85,
        Weight = 80.0
      };

      // Act & Assert
      Assert.Throws<ArgumentException>(() => hero.ValidateBusinessRules());
    }

    [Fact]
    public void Hero_ValidateBusinessRules_ShouldThrowException_WhenBirthdateIsInFuture()
    {
      // Arrange
      var hero = new Hero
      {
        Name = "Clark Kent",
        HeroName = "Superman",
        Birthdate = DateTime.Now.AddYears(1),
        Height = 1.85,
        Weight = 80.0
      };

      // Act & Assert
      Assert.Throws<ArgumentException>(() => hero.ValidateBusinessRules());
    }

    [Fact]
    public void Hero_ValidateBusinessRules_ShouldThrowException_WhenAlturaIsZeroOrNegative()
    {
      // Arrange
      var hero = new Hero
      {
        Name = "Clark Kent",
        HeroName = "Superman",
        Birthdate = DateTime.Now.AddYears(-30),
        Height = 0,
        Weight = 80.0
      };

      // Act & Assert
      Assert.Throws<ArgumentException>(() => hero.ValidateBusinessRules());
    }

    [Fact]
    public void Hero_ValidateBusinessRules_ShouldThrowException_WhenPesoIsZeroOrNegative()
    {
      // Arrange
      var hero = new Hero
      {
        Name = "Clark Kent",
        HeroName = "Superman",
        Birthdate = DateTime.Now.AddYears(-30),
        Height = 1.85,
        Weight = 0
      };

      // Act & Assert
      Assert.Throws<ArgumentException>(() => hero.ValidateBusinessRules());
    }

    [Fact]
    public void Hero_ValidateBusinessRules_ShouldNotThrowException_WhenAllPropertiesAreValid()
    {
      // Arrange
      var hero = new Hero
      {
        Name = "Clark Kent",
        HeroName = "Superman",
        Birthdate = DateTime.Now.AddYears(-30),
        Height = 1.85,
        Weight = 80.0
      };

      // Act & Assert
      var exception = Record.Exception(() => hero.ValidateBusinessRules());
      Assert.Null(exception);
    }

    [Fact]
    public void Hero_UpdateDetails_ShouldUpdateAllProperties()
    {
      // Arrange
      var hero = new Hero();
      var nome = "Bruce Wayne";
      var nomeHeroi = "Batman";
      var dataNascimento = DateTime.Now.AddYears(-35);
      var altura = 1.88;
      var peso = 85.0;

      // Act
      hero.UpdateDetails(nome, nomeHeroi, dataNascimento, altura, peso);

      // Assert
      Assert.Equal(nome, hero.Name);
      Assert.Equal(nomeHeroi, hero.HeroName);
      Assert.Equal(dataNascimento, hero.Birthdate);
      Assert.Equal(altura, hero.Height);
      Assert.Equal(peso, hero.Weight);
    }
  }
}

