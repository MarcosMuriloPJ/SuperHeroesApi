using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SuperHeroesApi.Infrastructure.Data;
using SuperHeroesApi.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Registers infrastructure services
// EnableSensitiveDataLogging só é habilitado em desenvolvimento para evitar vazamento
// de dados sensíveis em logs/exceções em produção.
builder.Services.AddInfrastructure(enableSensitiveDataLogging: builder.Environment.IsDevelopment());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
  {
    Title = "Super Heroes API",
    Version = "v1",
    Description = "API para gerenciamento de super-heróis com operações CRUD completas",
    Contact = new Microsoft.OpenApi.Models.OpenApiContact
    {
      Name = "Marcos Murilo",
      Email = "marcosmurilo.ti@gmail.com"
    },
    License = new Microsoft.OpenApi.Models.OpenApiLicense
    {
      Name = "Uso Demonstrativo",
      Url = new Uri("https://opensource.org/licenses/MIT")
    }
  });

  var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
  var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
  if (File.Exists(xmlPath))
  {
    c.IncludeXmlComments(xmlPath);
  }

  c.AddServer(new Microsoft.OpenApi.Models.OpenApiServer
  {
    Url = "/",
    Description = "Local com DB em memória"
  });

  c.DescribeAllParametersInCamelCase();
  c.EnableAnnotations();
});

// Política de CORS nomeada e restrita a origens explicitamente confiáveis, configuradas em
// "Cors:AllowedOrigins" (appsettings). Sem nenhuma origem configurada, nenhuma origem
// cross-origin é permitida (fail-closed), evitando o risco de uma política AllowAny* aberta.
// A leitura da configuração é adiada para dentro do delegate (avaliado de forma lazy pelo
// sistema de Options, após builder.Build()), garantindo que overrides de configuração
// (ex.: variáveis de ambiente, appsettings de teste) sejam respeitados.
builder.Services.AddCors(options =>
{
  options.AddPolicy("VueAppPolicy", policy =>
  {
    var corsAllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

    if (corsAllowedOrigins.Length > 0)
    {
      policy.WithOrigins(corsAllowedOrigins)
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Content-Type", "Authorization");
    }
    else
    {
      policy.WithOrigins([]);
    }
  });
});

// Autenticação JWT Bearer. A chave de assinatura é obrigatória e deve vir de configuração
// segura (variável de ambiente, user-secrets ou cofre de segredos). A leitura e validação são
// adiadas para dentro do delegate de opções (avaliado de forma lazy pelo sistema de Options,
// após builder.Build()), garantindo que overrides de configuração sejam respeitados; a
// aplicação falha (nunca cai em um default inseguro) assim que a autenticação é utilizada caso
// a chave esteja ausente ou fraca.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
      var jwtKey = builder.Configuration["Jwt:Key"];
      var jwtIssuer = builder.Configuration["Jwt:Issuer"];
      var jwtAudience = builder.Configuration["Jwt:Audience"];

      if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
      {
        throw new InvalidOperationException(
            "Configuração 'Jwt:Key' ausente ou fraca (mínimo de 32 caracteres). Configure um segredo " +
            "forte via variável de ambiente ou gerenciador de segredos antes de iniciar a aplicação.");
      }

      if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
      {
        throw new InvalidOperationException("As configurações 'Jwt:Issuer' e 'Jwt:Audience' são obrigatórias.");
      }

      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(1)
      };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
  var context = scope.ServiceProvider.GetRequiredService<SuperDbContext>();
  context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Super Heroes API v1");
    c.RoutePrefix = string.Empty;
  });
}

app.UseHttpsRedirection();

app.UseCors("VueAppPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

/// <summary>
/// Classe parcial pública que expõe o ponto de entrada da aplicação para uso em testes de
/// integração via WebApplicationFactory&lt;Program&gt;.
/// </summary>
public partial class Program { }

