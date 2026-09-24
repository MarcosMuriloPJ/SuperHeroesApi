using Microsoft.EntityFrameworkCore;
using SuperHeroesApi.Application.Services;
using SuperHeroesApi.Infrastructure.Data;
using SuperHeroesApi.Infrastructure.Repositories;

namespace SuperHeroesApi.Tests
{
  public class SuperpowerServiceTests : IDisposable
  {
    private readonly SuperDbContext _context;
    private readonly SuperpowerRepository _superpowerRepo;
    private readonly SuperpowerService _superpowerService;

    public SuperpowerServiceTests()
    {
      var options = new DbContextOptionsBuilder<SuperDbContext>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .Options;

      _context = new SuperDbContext(options);
      _context.Database.EnsureCreated();

      _superpowerRepo = new SuperpowerRepository(_context);
      _superpowerService = new SuperpowerService(_superpowerRepo);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllSeededSuperpowers()
    {
      // Act
      var result = await _superpowerService.GetAllAsync();

      // Assert
      Assert.Equal(10, result.Count());
      Assert.Contains(result, s => s.Name == "Voo");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnDtosWithIdAndName()
    {
      // Act
      var result = await _superpowerService.GetAllAsync();

      // Assert
      var superForca = result.Single(s => s.Name == "Super Força");
      Assert.Equal(1, superForca.Id);
      Assert.Equal("Força física sobre-humana", superForca.Description);
    }

    public void Dispose()
    {
      _context.Dispose();
      GC.SuppressFinalize(this);
    }
  }
}
