using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SuperHeroesApi.Domain.Entities;
using SuperHeroesApi.Infrastructure.Data;

namespace SuperHeroesApi.Tests
{
  /// <summary>
  /// Testes de infraestrutura com o provedor real SQLite, garantindo que as migrations
  /// aplicam corretamente o esquema (incluindo o seed de superpoderes) e que operações
  /// básicas de CRUD funcionam sobre um banco relacional de fato.
  /// </summary>
  public class SqliteInfrastructureTests : IDisposable
  {
    private readonly SqliteConnection _connection;
    private readonly SuperDbContext _context;

    public SqliteInfrastructureTests()
    {
      // Mantém a conexão SQLite em memória aberta durante todo o teste: o banco ":memory:"
      // deixa de existir assim que a única conexão associada a ele é fechada.
      _connection = new SqliteConnection("Data Source=:memory:");
      _connection.Open();

      var options = new DbContextOptionsBuilder<SuperDbContext>()
          .UseSqlite(_connection)
          .Options;

      _context = new SuperDbContext(options);
      _context.Database.Migrate();
    }

    [Fact]
    public void Database_ShouldBeRelational()
    {
      Assert.True(_context.Database.IsRelational());
    }

    [Fact]
    public async Task Migrate_ShouldSeedSuperpowers()
    {
      var superpowers = await _context.Superpowers.ToListAsync();

      Assert.Equal(10, superpowers.Count);
      Assert.Contains(superpowers, s => s.Name == "Voo");
    }

    [Fact]
    public async Task Migrate_ShouldEnforceUniqueHeroNameIndex()
    {
      _context.Heros.Add(new Hero { Name = "Clark Kent", HeroName = "Superman", Birthdate = DateTime.Now.AddYears(-30), Height = 1.85, Weight = 80 });
      await _context.SaveChangesAsync();

      _context.Heros.Add(new Hero { Name = "Outro Clark", HeroName = "Superman", Birthdate = DateTime.Now.AddYears(-20), Height = 1.80, Weight = 75 });

      await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task CanPersistAndRetrieveHeroWithSuperpowers()
    {
      var hero = new Hero
      {
        Name = "Barry Allen",
        HeroName = "Flash",
        Birthdate = DateTime.Now.AddYears(-28),
        Height = 1.83,
        Weight = 75,
        HerosSuperpowers = [new HeroSuperpower { SuperpowerId = 6 }]
      };

      _context.Heros.Add(hero);
      await _context.SaveChangesAsync();

      var persisted = await _context.Heros
        .Include(h => h.HerosSuperpowers)
          .ThenInclude(hs => hs.Superpower)
        .SingleAsync(h => h.HeroName == "Flash");

      Assert.True(persisted.Id > 0);
      Assert.Single(persisted.HerosSuperpowers);
      Assert.Equal("Super Velocidade", persisted.HerosSuperpowers.Single().Superpower.Name);
    }

    public void Dispose()
    {
      _context.Dispose();
      _connection.Dispose();
      GC.SuppressFinalize(this);
    }
  }
}
