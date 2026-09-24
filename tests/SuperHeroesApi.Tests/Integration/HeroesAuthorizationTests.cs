using System.Net;
using System.Net.Http.Json;
using SuperHeroesApi.Application.DTOs;

namespace SuperHeroesApi.Tests.Integration
{
  /// <summary>
  /// Testes de segurança para autenticação/autorização dos endpoints de escrita.
  /// Cobre a correção do achado F1 (ausência total de autenticação/autorização).
  /// </summary>
  public class HeroesAuthorizationTests : IClassFixture<SuperHeroesApiFactory>
  {
    private readonly SuperHeroesApiFactory _factory;

    public HeroesAuthorizationTests(SuperHeroesApiFactory factory)
    {
      _factory = factory;
    }

    private static CreateHeroDto BuildValidCreateDto(string? heroName = null) => new()
    {
      Name = "Nome Civil " + Guid.NewGuid(),
      HeroName = heroName ?? $"Heroi-{Guid.NewGuid()}",
      Birthdate = DateTime.Now.AddYears(-30),
      Height = 1.80,
      Weight = 80.0,
      SuperpowersIds = [1]
    };

    [Fact]
    public async Task CreateHero_WithoutToken_ShouldReturnUnauthorized()
    {
      var client = _factory.CreateClient();

      var response = await client.PostAsJsonAsync("/api/heroes", BuildValidCreateDto());

      Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateHero_WithoutToken_ShouldReturnUnauthorized()
    {
      var client = _factory.CreateClient();

      var response = await client.PutAsJsonAsync("/api/heroes/1", BuildValidCreateDto());

      Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteHero_WithoutToken_ShouldReturnUnauthorized()
    {
      var client = _factory.CreateClient();

      var response = await client.DeleteAsync("/api/heroes/1");

      Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateHero_WithExpiredToken_ShouldReturnUnauthorized()
    {
      var client = _factory.CreateClient();
      client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_factory.CreateExpiredToken()}");

      var response = await client.PostAsJsonAsync("/api/heroes", BuildValidCreateDto());

      Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateHero_WithTokenSignedByWrongKey_ShouldReturnUnauthorized()
    {
      var client = _factory.CreateClient();
      client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_factory.CreateTokenSignedWithWrongKey()}");

      var response = await client.PostAsJsonAsync("/api/heroes", BuildValidCreateDto());

      Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateHero_WithValidToken_ShouldSucceed()
    {
      var client = _factory.CreateClient();
      client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_factory.CreateValidToken()}");

      var response = await client.PostAsJsonAsync("/api/heroes", BuildValidCreateDto());

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAndDeleteHero_WithValidToken_ShouldSucceed()
    {
      var client = _factory.CreateClient();
      client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_factory.CreateValidToken()}");

      var createResponse = await client.PostAsJsonAsync("/api/heroes", BuildValidCreateDto());
      createResponse.EnsureSuccessStatusCode();
      var created = await createResponse.Content.ReadFromJsonAsync<CreatedHeroEnvelope>();
      Assert.NotNull(created);

      var updateResponse = await client.PutAsJsonAsync($"/api/heroes/{created!.Data.Id}", BuildValidCreateDto());
      Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

      var deleteResponse = await client.DeleteAsync($"/api/heroes/{created.Data.Id}");
      Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task GetAllHeroes_WithoutToken_ShouldStillBeAllowed()
    {
      // Leitura permanece pública nesta correção; garante que não houve regressão nos GETs.
      var client = _factory.CreateClient();

      var response = await client.GetAsync("/api/heroes");

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private class CreatedHeroEnvelope
    {
      public HeroDto Data { get; set; } = null!;
    }
  }
}
