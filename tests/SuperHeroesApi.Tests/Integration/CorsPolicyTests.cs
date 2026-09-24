using System.Net.Http;

namespace SuperHeroesApi.Tests.Integration
{
  /// <summary>
  /// Testes de segurança/configuração para a política de CORS.
  /// Cobre a correção do achado F2 (CORS AllowAny* com nome de política divergente).
  /// </summary>
  public class CorsPolicyTests : IClassFixture<SuperHeroesApiFactory>
  {
    private readonly SuperHeroesApiFactory _factory;

    public CorsPolicyTests(SuperHeroesApiFactory factory)
    {
      _factory = factory;
    }

    private static HttpRequestMessage BuildPreflightRequest(string origin)
    {
      var request = new HttpRequestMessage(HttpMethod.Options, "/api/heroes");
      request.Headers.Add("Origin", origin);
      request.Headers.Add("Access-Control-Request-Method", "POST");
      return request;
    }

    [Fact]
    public async Task Preflight_ShouldAllow_TrustedConfiguredOrigin()
    {
      var client = _factory.CreateClient();

      var response = await client.SendAsync(BuildPreflightRequest("https://trusted.example.com"));

      Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
      Assert.Equal("https://trusted.example.com", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public async Task Preflight_ShouldBlock_UntrustedOrigin()
    {
      var client = _factory.CreateClient();

      var response = await client.SendAsync(BuildPreflightRequest("https://evil.example.com"));

      Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Cors_ShouldNeverReturnWildcardOrigin()
    {
      // Regressão: a política anterior usava AllowAnyOrigin(), que nunca deve voltar.
      var client = _factory.CreateClient();

      var response = await client.SendAsync(BuildPreflightRequest("https://trusted.example.com"));

      if (response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values))
      {
        Assert.DoesNotContain("*", values);
      }
    }

    [Fact]
    public async Task Cors_WithNoAllowedOriginsConfigured_ShouldBlockAllCrossOriginRequests()
    {
      var factory = new SuperHeroesApiFactory { AllowedOrigins = [] };
      var client = factory.CreateClient();

      var response = await client.SendAsync(BuildPreflightRequest("https://anything.example.com"));

      Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }
  }
}
