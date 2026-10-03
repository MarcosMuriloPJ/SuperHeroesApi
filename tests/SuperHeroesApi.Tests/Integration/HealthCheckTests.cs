using System.Net;

namespace SuperHeroesApi.Tests.Integration
{
  /// <summary>
  /// Testes do endpoint de health check, usado por orquestradores/monitoramento
  /// (ex.: liveness e readiness probes) para verificar a saúde da aplicação e do banco de dados.
  /// </summary>
  public class HealthCheckTests : IClassFixture<SuperHeroesApiFactory>
  {
    private readonly SuperHeroesApiFactory _factory;

    public HealthCheckTests(SuperHeroesApiFactory factory)
    {
      _factory = factory;
    }

    [Fact]
    public async Task Health_ShouldReturnOkAndHealthyStatus()
    {
      var client = _factory.CreateClient();

      var response = await client.GetAsync("/health");
      var content = await response.Content.ReadAsStringAsync();

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      Assert.Equal("Healthy", content);
    }
  }
}
