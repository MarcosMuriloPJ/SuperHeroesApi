using Microsoft.EntityFrameworkCore;
using SuperHeroesApi.Domain.Entities;
using SuperHeroesApi.Infrastructure.Data;
using SuperHeroesApi.Infrastructure.Repositories;

namespace SuperHeroesApi.Tests
{
  public class HeroRepositoryTests : IDisposable
  {
    private readonly SuperDbContext _context;
    private readonly HeroRepository _repo;

    public HeroRepositoryTests()
    {
      var options = new DbContextOptionsBuilder<SuperDbContext>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .Options;

      _context = new SuperDbContext(options);
      _context.Database.EnsureCreated();

      _repo = new HeroRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoHeroesExist()
    {
      var result = await _repo.GetAllAsync();

      Assert.Empty(result);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistHeroWithSuperpowers()
    {
      var hero = new Hero
      {
        Name = "Barry Allen",
        HeroName = "Flash",
        Birthdate = DateTime.Now.AddYears(-28),
        Height = 1.83,
        Weight = 75.0,
        HerosSuperpowers = [new HeroSuperpower { SuperpowerId = 6 }]
      };

      var created = await _repo.AddAsync(hero);

      Assert.True(created.Id > 0);
      Assert.Single(created.HerosSuperpowers);
      Assert.Equal("Super Velocidade", created.HerosSuperpowers.Single().Superpower.Name);
    }

    [Fact]
    public async Task GetByHeroNameAsync_ShouldReturnNull_WhenNotFound()
    {
      var result = await _repo.GetByHeroNameAsync("Inexistente");

      Assert.Null(result);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenHeroDoesNotExist()
    {
      var exists = await _repo.ExistsAsync(12345);

      Assert.False(exists);
    }

    [Fact]
    public async Task HeroNameExistsAsync_ShouldIgnoreExcludedId()
    {
      var hero = new Hero
      {
        Name = "Arthur Curry",
        HeroName = "Aquaman",
        Birthdate = DateTime.Now.AddYears(-32),
        Height = 1.9,
        Weight = 100.0
      };
      var created = await _repo.AddAsync(hero);

      var existsIncludingSelf = await _repo.HeroNameExistsAsync("Aquaman");
      var existsExcludingSelf = await _repo.HeroNameExistsAsync("Aquaman", created.Id);

      Assert.True(existsIncludingSelf);
      Assert.False(existsExcludingSelf);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveHero_WhenExists()
    {
      var hero = new Hero
      {
        Name = "Hal Jordan",
        HeroName = "Green Lantern",
        Birthdate = DateTime.Now.AddYears(-33),
        Height = 1.82,
        Weight = 84.0
      };
      var created = await _repo.AddAsync(hero);

      await _repo.DeleteAsync(created.Id);

      Assert.False(await _repo.ExistsAsync(created.Id));
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotThrow_WhenHeroDoesNotExist()
    {
      var exception = await Record.ExceptionAsync(() => _repo.DeleteAsync(99999));

      Assert.Null(exception);
    }

    public void Dispose()
    {
      _context.Dispose();
      GC.SuppressFinalize(this);
    }
  }
}
