using Microsoft.EntityFrameworkCore;
using SuperHeroesApi.Domain.Entities;
using SuperHeroesApi.Infrastructure.Data;

namespace SuperHeroesApi.Tests
{
  /// <summary>
  /// Testes específicos para o banco de dados em memória
  /// </summary>
  public class InMemoryDatabaseTests
  {
    /// <summary>
    /// Testa se o banco de dados em memória é criado corretamente
    /// </summary>
    [Fact]
    public void InMemoryDatabase_ShouldBeCreated()
    {
      // Arrange
      var options = new DbContextOptionsBuilder<SuperDbContext>()
          .UseInMemoryDatabase(databaseName: "TestDatabase")
          .Options;

      // Act
      using var context = new SuperDbContext(options);
      context.Database.EnsureCreated();

      // Assert
      Assert.True(context.Database.IsInMemory());
    }

    /// <summary>
    /// Testa se os dados de seed são carregados corretamente
    /// </summary>
    [Fact]
    public void InMemoryDatabase_ShouldLoadSeedData()
    {
      // Arrange
      var options = new DbContextOptionsBuilder<SuperDbContext>()
          .UseInMemoryDatabase(databaseName: "TestSeedDatabase")
          .Options;

      // Act
      using var context = new SuperDbContext(options);
      context.Database.EnsureCreated();

      // Assert
      Assert.True(context.Superpowers.Any());
      Assert.Equal(10, context.Superpowers.Count());
    }

    /// <summary>
    /// Testa se é possível adicionar e recuperar dados do banco em memória
    /// </summary>
    [Fact]
    public async Task InMemoryDatabase_ShouldAddAndRetrieveData()
    {
      // Arrange
      var options = new DbContextOptionsBuilder<SuperDbContext>()
          .UseInMemoryDatabase(databaseName: "TestAddRetrieveDatabase")
          .Options;

      // Act - Add data
      using (var context = new SuperDbContext(options))
      {
        context.Database.EnsureCreated();

        var hero = new Hero
        {
          Name = "Bruce Wayne",
          HeroName = "Batman",
          Birthdate = DateTime.Now.AddYears(-40),
          Height = 1.88,
          Weight = 95.0
        };

        context.Heros.Add(hero);
        await context.SaveChangesAsync();
      }

      // Act - Retrieve data
      using (var context = new SuperDbContext(options))
      {
        var hero = await context.Heros.FirstOrDefaultAsync(h => h.HeroName == "Batman");

        // Assert
        Assert.NotNull(hero);
        Assert.Equal("Bruce Wayne", hero.Name);
      }
    }

    /// <summary>
    /// Testa se o relacionamento entre Hero e Superpower funciona corretamente
    /// </summary>
    [Fact]
    public async Task InMemoryDatabase_ShouldHandleRelationships()
    {
      // Arrange
      var options = new DbContextOptionsBuilder<SuperDbContext>()
          .UseInMemoryDatabase(databaseName: "TestRelationshipsDatabase")
          .Options;

      // Act - Add data with relationships
      using (var context = new SuperDbContext(options))
      {
        context.Database.EnsureCreated();

        var hero = new Hero
        {
          Name = "Diana Prince",
          HeroName = "Wonder Woman",
          Birthdate = DateTime.Now.AddYears(-800),
          Height = 1.80,
          Weight = 74.0
        };

        context.Heros.Add(hero);
        await context.SaveChangesAsync();

        var superpower1 = await context.Superpowers.FindAsync(1); // Super Força
        var superpower2 = await context.Superpowers.FindAsync(2); // Voo

        context.HerosSuperpowers.Add(new HeroSuperpower
        {
          HeroId = hero.Id,
          SuperpowerId = superpower1!.Id
        });

        context.HerosSuperpowers.Add(new HeroSuperpower
        {
          HeroId = hero.Id,
          SuperpowerId = superpower2!.Id
        });

        await context.SaveChangesAsync();
      }

      // Act - Retrieve data with relationships
      using (var context = new SuperDbContext(options))
      {
        var hero = await context.Heros
            .Include(h => h.HerosSuperpowers)
                .ThenInclude(hs => hs.Superpower)
            .FirstOrDefaultAsync(h => h.HeroName == "Wonder Woman");

        // Assert
        Assert.NotNull(hero);
        Assert.Equal(2, hero.HerosSuperpowers.Count);
        Assert.Contains(hero.HerosSuperpowers, hs => hs.Superpower.Name == "Voo");
        Assert.Contains(hero.HerosSuperpowers, hs => hs.Superpower.Name == "Super Força");
      }
    }
  }
}