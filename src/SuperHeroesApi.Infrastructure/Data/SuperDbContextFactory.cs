using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SuperHeroesApi.Infrastructure.Data
{
  /// <summary>
  /// Fábrica usada pelas ferramentas de design-time do EF Core (ex.: <c>dotnet ef migrations add</c>)
  /// para criar instâncias de <see cref="SuperDbContext"/> sem depender do host da aplicação.
  /// </summary>
  public class SuperDbContextFactory : IDesignTimeDbContextFactory<SuperDbContext>
  {
    public SuperDbContext CreateDbContext(string[] args)
    {
      var optionsBuilder = new DbContextOptionsBuilder<SuperDbContext>();
      optionsBuilder.UseSqlite("Data Source=superheroes.db");

      return new SuperDbContext(optionsBuilder.Options);
    }
  }
}
