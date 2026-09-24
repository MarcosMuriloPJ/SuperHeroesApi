using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SuperHeroesApi.Infrastructure.Data;
using SuperHeroesApi.Infrastructure.Extensions;

namespace SuperHeroesApi.Tests
{
  /// <summary>
  /// Testes de segurança/configuração para o registro de infraestrutura.
  /// Cobre a correção do achado F3 (EnableSensitiveDataLogging habilitado sem gate de ambiente).
  /// </summary>
  public class InfrastructureServiceTests
  {
    [Fact]
    public void AddInfrastructure_ByDefault_ShouldNotEnableSensitiveDataLogging()
    {
      // Arrange
      var services = new ServiceCollection();

      // Act
      services.AddInfrastructure();
      using var provider = services.BuildServiceProvider();
      var options = provider.GetRequiredService<DbContextOptions<SuperDbContext>>();

      // Assert
      var extension = options.FindExtension<CoreOptionsExtension>();
      Assert.False(extension is not null && extension.IsSensitiveDataLoggingEnabled);
    }

    [Fact]
    public void AddInfrastructure_WhenEnableSensitiveDataLoggingIsFalse_ShouldNotEnableIt()
    {
      // Arrange
      var services = new ServiceCollection();

      // Act
      services.AddInfrastructure(enableSensitiveDataLogging: false);
      using var provider = services.BuildServiceProvider();
      var options = provider.GetRequiredService<DbContextOptions<SuperDbContext>>();

      // Assert
      var extension = options.FindExtension<CoreOptionsExtension>();
      Assert.False(extension is not null && extension.IsSensitiveDataLoggingEnabled);
    }

    [Fact]
    public void AddInfrastructure_WhenEnableSensitiveDataLoggingIsTrue_ShouldEnableIt()
    {
      // Arrange - simula builder.Environment.IsDevelopment() == true
      var services = new ServiceCollection();

      // Act
      services.AddInfrastructure(enableSensitiveDataLogging: true);
      using var provider = services.BuildServiceProvider();
      var options = provider.GetRequiredService<DbContextOptions<SuperDbContext>>();

      // Assert
      var extension = options.FindExtension<CoreOptionsExtension>();
      Assert.NotNull(extension);
      Assert.True(extension.IsSensitiveDataLoggingEnabled);
    }

    [Fact]
    public void AddInfrastructure_ShouldStillRegisterAllApplicationServices()
    {
      // Arrange
      var services = new ServiceCollection();

      // Act
      services.AddInfrastructure();
      using var provider = services.BuildServiceProvider();

      // Assert - garante que a correção não quebrou o registro dos demais serviços
      Assert.NotNull(provider.GetService<SuperDbContext>());
      Assert.NotNull(provider.GetService<SuperHeroesApi.Domain.Interfaces.IHeroRepository>());
      Assert.NotNull(provider.GetService<SuperHeroesApi.Domain.Interfaces.ISuperpowerRepository>());
      Assert.NotNull(provider.GetService<SuperHeroesApi.Application.Services.IHeroService>());
      Assert.NotNull(provider.GetService<SuperHeroesApi.Application.Services.ISuperpowerService>());
    }
  }
}
