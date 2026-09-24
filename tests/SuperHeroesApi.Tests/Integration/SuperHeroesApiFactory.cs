using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SuperHeroesApi.Infrastructure.Data;

namespace SuperHeroesApi.Tests.Integration
{
  /// <summary>
  /// Fábrica de aplicação de testes de integração. Injeta configuração de JWT e CORS
  /// isolada por instância de teste e substitui o DbContext por um banco InMemory único,
  /// evitando que classes de teste diferentes compartilhem estado.
  /// </summary>
  public class SuperHeroesApiFactory : WebApplicationFactory<Program>
  {
    public const string JwtKey = "chave-de-teste-somente-para-integracao-nunca-usar-em-producao-32+";
    public const string JwtIssuer = "SuperHeroesApi.Tests";
    public const string JwtAudience = "SuperHeroesApi.Tests.Clients";

    /// <summary>
    /// Origens de CORS confiáveis usadas por padrão nos testes de integração.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = ["https://trusted.example.com"];

    // Gerado uma única vez por instância da fábrica. Se o nome fosse gerado dentro do
    // delegate de configuração de opções, cada novo escopo de DI (por exemplo, cada
    // requisição HTTP) receberia um banco InMemory diferente e vazio, já que
    // AddDbContext registra DbContextOptions com ciclo de vida "Scoped" por padrão.
    private readonly string _databaseName = $"SuperHeroesDB-Tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.UseEnvironment("Testing");

      builder.ConfigureAppConfiguration((_, config) =>
      {
        var settings = new Dictionary<string, string?>
        {
          ["Jwt:Key"] = JwtKey,
          ["Jwt:Issuer"] = JwtIssuer,
          ["Jwt:Audience"] = JwtAudience,
        };

        for (var i = 0; i < AllowedOrigins.Length; i++)
        {
          settings[$"Cors:AllowedOrigins:{i}"] = AllowedOrigins[i];
        }

        config.AddInMemoryCollection(settings);
      });

      builder.ConfigureServices(services =>
      {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SuperDbContext>));
        if (descriptor != null)
        {
          services.Remove(descriptor);
        }

        services.AddDbContext<SuperDbContext>(options =>
            options.UseInMemoryDatabase(_databaseName));
      });
    }

    /// <summary>
    /// Gera um token JWT válido assinado com a mesma chave configurada para o host de testes.
    /// </summary>
    /// <param name="roles">Papéis a incluir no token</param>
    /// <param name="expiresIn">Tempo de expiração relativo a agora; padrão de 30 minutos</param>
    public string CreateValidToken(IEnumerable<string>? roles = null, TimeSpan? expiresIn = null)
    {
      var claims = new List<Claim> { new(ClaimTypes.Name, "usuario-de-teste") };
      claims.AddRange((roles ?? []).Select(r => new Claim(ClaimTypes.Role, r)));

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
      var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var token = new JwtSecurityToken(
          issuer: JwtIssuer,
          audience: JwtAudience,
          claims: claims,
          expires: DateTime.UtcNow.Add(expiresIn ?? TimeSpan.FromMinutes(30)),
          signingCredentials: credentials);

      return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Gera um token JWT expirado, útil para validar que a expiração é respeitada.
    /// </summary>
    public string CreateExpiredToken()
    {
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
      var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var token = new JwtSecurityToken(
          issuer: JwtIssuer,
          audience: JwtAudience,
          claims: [new Claim(ClaimTypes.Name, "usuario-de-teste")],
          expires: DateTime.UtcNow.AddMinutes(-5),
          signingCredentials: credentials);

      return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Gera um token assinado com uma chave diferente da configurada no servidor (simula um
    /// token forjado/adulterado).
    /// </summary>
    public string CreateTokenSignedWithWrongKey()
    {
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("outra-chave-diferente-da-configurada-no-servidor-32+"));
      var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var token = new JwtSecurityToken(
          issuer: JwtIssuer,
          audience: JwtAudience,
          claims: [new Claim(ClaimTypes.Name, "usuario-de-teste")],
          expires: DateTime.UtcNow.AddMinutes(30),
          signingCredentials: credentials);

      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}
