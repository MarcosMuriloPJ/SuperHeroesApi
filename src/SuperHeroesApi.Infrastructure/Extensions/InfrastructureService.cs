using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SuperHeroesApi.Application.Services;
using SuperHeroesApi.Domain.Interfaces;
using SuperHeroesApi.Infrastructure.Data;
using SuperHeroesApi.Infrastructure.Repositories;

namespace SuperHeroesApi.Infrastructure.Extensions;

public static class InfrastructureService
{
  /// <summary>
  /// Adds the infrastructure services to the service collection.
  /// This includes setting up the database context, repositories and services.
  /// </summary>
  public static IServiceCollection AddInfrastructure(this IServiceCollection services)
  {
    services.AddDbContext<SuperDbContext>(options =>
        options.UseInMemoryDatabase("SuperHeroesDB")
              .EnableSensitiveDataLogging());

    services.AddScoped<IHeroRepository, HeroRepository>();
    services.AddScoped<ISuperpowerRepository, SuperpowerRepository>();

    services.AddScoped<IHeroService, HeroService>();
    services.AddScoped<ISuperpowerService, SuperpowerService>();

    return services;
  }
}
