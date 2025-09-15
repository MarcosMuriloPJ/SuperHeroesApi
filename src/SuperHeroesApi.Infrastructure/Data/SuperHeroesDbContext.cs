using Microsoft.EntityFrameworkCore;
using SuperHeroesApi.Domain.Entities;

namespace SuperHeroesApi.Infrastructure.Data
{
  /// <summary>
  /// Contexto de banco de dados para a aplicação SuperHeroes
  /// </summary>
  public class SuperDbContext(DbContextOptions<SuperDbContext> options) : DbContext(options)
  {
    /// <summary>
    /// Conjunto de entidades Hero no banco de dados
    /// </summary>
    public DbSet<Hero> Heros { get; set; }

    /// <summary>
    /// Conjunto de entidades Superpower no banco de dados
    /// </summary>
    public DbSet<Superpower> Superpowers { get; set; }

    /// <summary>
    /// Conjunto de entidades HeroSuperpower no banco de dados, representando a relação muitos-para-muitos entre Hero e Superpower
    /// </summary>
    public DbSet<HeroSuperpower> HerosSuperpowers { get; set; }

    /// <summary>
    /// Configura o modelo de entidades para o banco de dados
    /// </summary>
    /// <param name="modelBuilder">O construtor de modelo usado para configurar as entidades</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<Hero>(entity =>
      {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
        entity.Property(e => e.HeroName).IsRequired().HasMaxLength(120);
        entity.Property(e => e.Birthdate).IsRequired();
        entity.Property(e => e.Height).IsRequired();
        entity.Property(e => e.Weight).IsRequired();

        entity.HasIndex(e => e.HeroName).IsUnique();
      });

      modelBuilder.Entity<Superpower>(entity =>
      {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
        entity.Property(e => e.Description).HasMaxLength(250);
      });

      modelBuilder.Entity<HeroSuperpower>(entity =>
      {
        entity.HasKey(e => new { e.HeroId, e.SuperpowerId });

        entity.HasOne(e => e.Hero)
                  .WithMany(h => h.HerosSuperpowers)
                  .HasForeignKey(e => e.HeroId)
                  .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Superpower)
                  .WithMany(s => s.HerosSuperpowers)
                  .HasForeignKey(e => e.SuperpowerId)
                  .OnDelete(DeleteBehavior.Cascade);
      });

      modelBuilder.Entity<Superpower>().HasData(
          new Superpower { Id = 1, Name = "Super Força", Description = "Força física sobre-humana" },
          new Superpower { Id = 2, Name = "Voo", Description = "Capacidade de voar" },
          new Superpower { Id = 3, Name = "Invisibilidade", Description = "Capacidade de se tornar invisível" },
          new Superpower { Id = 4, Name = "Telepatia", Description = "Capacidade de ler mentes" },
          new Superpower { Id = 5, Name = "Teletransporte", Description = "Capacidade de se teletransportar" },
          new Superpower { Id = 6, Name = "Super Velocidade", Description = "Velocidade sobre-humana" },
          new Superpower { Id = 7, Name = "Regeneração", Description = "Capacidade de se curar rapidamente" },
          new Superpower { Id = 8, Name = "Controle Mental", Description = "Capacidade de controlar mentes" },
          new Superpower { Id = 9, Name = "Raios Laser", Description = "Capacidade de disparar raios laser dos olhos" },
          new Superpower { Id = 10, Name = "Elasticidade", Description = "Capacidade de esticar o corpo" }
      );
    }
  }
}

