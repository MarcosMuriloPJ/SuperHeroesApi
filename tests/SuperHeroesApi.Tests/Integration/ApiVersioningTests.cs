using System.Net;
using System.Net.Http.Json;

namespace SuperHeroesApi.Tests.Integration
{
  /// <summary>
  /// Testes de versionamento de API. Garante que apenas rotas com o segmento de versão
  /// (ex.: "/api/v1/heroes") são atendidas, e que as demais funcionalidades continuam operando
  /// normalmente sob a rota versionada.
  /// </summary>
  public class ApiVersioningTests : IClassFixture<SuperHeroesApiFactory>
  {
    private readonly SuperHeroesApiFactory _factory;

    public ApiVersioningTests(SuperHeroesApiFactory factory)
    {
      _factory = factory;
    }

    [Fact]
    public async Task GetHeroes_WithVersionedRoute_ShouldSucceed()
    {
      var client = _factory.CreateClient();

      var response = await client.GetAsync("/api/v1/heroes");

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHeroes_WithoutVersionSegment_ShouldReturnNotFound()
    {
      var client = _factory.CreateClient();

      var response = await client.GetAsync("/api/heroes");

      Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetHeroes_WithUnsupportedVersion_ShouldReturnNotFound()
    {
      var client = _factory.CreateClient();

      var response = await client.GetAsync("/api/v2/heroes");

      Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetHeroes_ShouldReportSupportedApiVersionsHeader()
    {
      var client = _factory.CreateClient();

      var response = await client.GetAsync("/api/v1/heroes");

      Assert.True(response.Headers.Contains("api-supported-versions"));
    }

    [Fact]
    public async Task GetSuperpowers_WithVersionedRoute_ShouldReturnSeededSuperpowers()
    {
      var client = _factory.CreateClient();

      var response = await client.GetAsync("/api/v1/superpowers");
      response.EnsureSuccessStatusCode();

      var superpowers = await response.Content.ReadFromJsonAsync<List<object>>();
      Assert.NotNull(superpowers);
      Assert.NotEmpty(superpowers);
    }
  }
}
