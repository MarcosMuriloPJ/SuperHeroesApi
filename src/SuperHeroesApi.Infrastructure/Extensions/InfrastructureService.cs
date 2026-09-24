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
  /// <param name="services">Coleção de serviços à qual a infraestrutura será adicionada</param>
  /// <param name="enableSensitiveDataLogging">
  /// Habilita o log de dados sensíveis do EF Core (valores de parâmetros em logs/exceções).
  /// Deve ser habilitado apenas em ambiente de desenvolvimento, nunca em produção, pois pode
  /// vazar dados pessoais em logs e telemetria.
  /// </param>
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, bool enableSensitiveDataLogging = false)
  {
    services.AddDbContext<SuperDbContext>(options =>
    {
      options.UseInMemoryDatabase("SuperHeroesDB");

      if (enableSensitiveDataLogging)
      {
        options.EnableSensitiveDataLogging();
      }
    });

    services.AddScoped<IHeroRepository, HeroRepository>();
    services.AddScoped<ISuperpowerRepository, SuperpowerRepository>();

    services.AddScoped<IHeroService, HeroService>();
    services.AddScoped<ISuperpowerService, SuperpowerService>();

    return services;
  }
}
