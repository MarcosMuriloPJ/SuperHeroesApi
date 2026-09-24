using Microsoft.EntityFrameworkCore;
using SuperHeroesApi.Infrastructure.Data;
using SuperHeroesApi.Infrastructure.Repositories;

namespace SuperHeroesApi.Tests
{
  public class SuperpowerRepositoryTests : IDisposable
  {
    private readonly SuperDbContext _context;
    private readonly SuperpowerRepository _repo;

    public SuperpowerRepositoryTests()
    {
      var options = new DbContextOptionsBuilder<SuperDbContext>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .Options;

      _context = new SuperDbContext(options);
      _context.Database.EnsureCreated();

      _repo = new SuperpowerRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSuperpower_WhenExists()
    {
      var result = await _repo.GetByIdAsync(2);

      Assert.NotNull(result);
      Assert.Equal("Voo", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
      var result = await _repo.GetByIdAsync(999);

      Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnOnlyMatchingSuperpowers()
    {
      var result = await _repo.GetByIdsAsync([1, 2, 999]);

      Assert.Equal(2, result.Count());
      Assert.Contains(result, s => s.Name == "Super Força");
      Assert.Contains(result, s => s.Name == "Voo");
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnEmpty_WhenNoIdsMatch()
    {
      var result = await _repo.GetByIdsAsync([9001, 9002]);

      Assert.Empty(result);
    }

    public void Dispose()
    {
      _context.Dispose();
      GC.SuppressFinalize(this);
    }
  }
}
