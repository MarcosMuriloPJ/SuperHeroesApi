using SuperHeroesApi.Infrastructure.Data;
using SuperHeroesApi.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Registers infrastructure services
builder.Services.AddInfrastructure();

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

builder.Services.AddCors(options =>
{
  options.AddDefaultPolicy(policy =>
  {
    policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
  });
});

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

app.UseAuthorization();

app.MapControllers();

app.Run();

