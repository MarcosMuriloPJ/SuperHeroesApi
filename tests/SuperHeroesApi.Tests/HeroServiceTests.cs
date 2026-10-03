using Microsoft.EntityFrameworkCore;
using SuperHeroesApi.Application.DTOs;
using SuperHeroesApi.Application.Services;
using SuperHeroesApi.Domain.Entities;
using SuperHeroesApi.Infrastructure.Data;
using SuperHeroesApi.Infrastructure.Repositories;

namespace SuperHeroesApi.Tests
{
  public class HeroServiceTests : IDisposable
  {
    private readonly SuperDbContext _context;
    private readonly HeroRepository _heroRepo;
    private readonly SuperpowerRepository _superpowerRepo;
    private readonly HeroService _heroService;

    public HeroServiceTests()
    {
      var options = new DbContextOptionsBuilder<SuperDbContext>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .Options;

      _context = new SuperDbContext(options);
      _context.Database.EnsureCreated();

      _heroRepo = new HeroRepository(_context);
      _superpowerRepo = new SuperpowerRepository(_context);
      _heroService = new HeroService(_heroRepo, _superpowerRepo);
    }

    [Fact]
    public async Task CreateHeroAsync_ShouldCreateHero_WhenValidData()
    {
      // Arrange
      var createHeroDto = new CreateHeroDto
      {
        Name = "Clark Kent",
        HeroName = "Superman",
        Birthdate = DateTime.Now.AddYears(-30),
        Height = 1.85,
        Weight = 80.0,
        SuperpowersIds = [1, 2]
      };

      // Act
      var result = await _heroService.CreateAsync(createHeroDto);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(createHeroDto.Name, result.Name);
      Assert.Equal(createHeroDto.HeroName, result.HeroName);
      Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task CreateHeroAsync_ShouldThrowException_WhenHeroNameAlreadyExists()
    {
      // Arrange
      var existingHero = new Hero
      {
        Name = "Clark Kent",
        HeroName = "Superman",
        Birthdate = DateTime.Now.AddYears(-30),
        Height = 1.85,
        Weight = 80.0
      };
      await _heroRepo.AddAsync(existingHero);

      var createHeroDto = new CreateHeroDto
      {
        Name = "Kal-El",
        HeroName = "Superman",
        Birthdate = DateTime.Now.AddYears(-25),
        Height = 1.90,
        Weight = 85.0,
        SuperpowersIds = [1]
      };

      // Act & Assert
      await Assert.ThrowsAsync<InvalidOperationException>(() => _heroService.CreateAsync(createHeroDto));
    }

    [Fact]
    public async Task CreateHeroAsync_ShouldThrowException_WhenSuperpowerDoesNotExist()
    {
      // Arrange
      var createHeroDto = new CreateHeroDto
      {
        Name = "Clark Kent",
        HeroName = "Superman",
        Birthdate = DateTime.Now.AddYears(-30),
        Height = 1.85,
        Weight = 80.0,
        SuperpowersIds = [999]
      };

      // Act & Assert
      await Assert.ThrowsAsync<ArgumentException>(() => _heroService.CreateAsync(createHeroDto));
    }

    [Fact]
    public async Task GetHeroByIdAsync_ShouldReturnHero_WhenHeroExists()
    {
      // Arrange
      var hero = new Hero
      {
        Name = "Bruce Wayne",
        HeroName = "Batman",
        Birthdate = DateTime.Now.AddYears(-35),
        Height = 1.88,
        Weight = 85.0
      };
      var createdHero = await _heroRepo.AddAsync(hero);

      // Act
      var result = await _heroService.GetByIdAsync(createdHero.Id);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(createdHero.Id, result.Id);
      Assert.Equal(hero.Name, result.Name);
      Assert.Equal(hero.HeroName, result.HeroName);
    }

    [Fact]
    public async Task GetHeroByIdAsync_ShouldReturnNull_WhenHeroDoesNotExist()
    {
      // Act
      var result = await _heroService.GetByIdAsync(999);

      // Assert
      Assert.Null(result);
    }

    [Fact]
    public async Task UpdateHeroAsync_ShouldUpdateHero_WhenValidData()
    {
      // Arrange
      var hero = new Hero
      {
        Name = "Peter Parker",
        HeroName = "Spider-Man",
        Birthdate = DateTime.Now.AddYears(-25),
        Height = 1.75,
        Weight = 70.0
      };
      var createdHero = await _heroRepo.AddAsync(hero);

      var updateHeroDto = new UpdateHeroDto
      {
        Name = "Peter Benjamin Parker",
        HeroName = "Amazing Spider-Man",
        Birthdate = hero.Birthdate,
        Height = 1.76,
        Weight = 72.0,
        SuperpowersIds = [1]
      };

      // Act
      var result = await _heroService.UpdateAsync(createdHero.Id, updateHeroDto);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(updateHeroDto.Name, result.Name);
      Assert.Equal(updateHeroDto.HeroName, result.HeroName);
      Assert.Equal(updateHeroDto.Height, result.Height);
      Assert.Equal(updateHeroDto.Weight, result.Weight);
    }

    [Fact]
    public async Task UpdateHeroAsync_ShouldThrowException_WhenHeroDoesNotExist()
    {
      // Arrange
      var updateHeroDto = new UpdateHeroDto
      {
        Name = "Non Existent",
        HeroName = "Ghost",
        Birthdate = DateTime.Now.AddYears(-30),
        Height = 1.80,
        Weight = 75.0,
        SuperpowersIds = [1]
      };

      // Act & Assert
      await Assert.ThrowsAsync<ArgumentException>(() => _heroService.UpdateAsync(999, updateHeroDto));
    }

    [Fact]
    public async Task DeleteHeroAsync_ShouldDeleteHero_WhenHeroExists()
    {
      // Arrange
      var hero = new Hero
      {
        Name = "Tony Stark",
        HeroName = "Iron Man",
        Birthdate = DateTime.Now.AddYears(-40),
        Height = 1.85,
        Weight = 80.0
      };
      var createdHero = await _heroRepo.AddAsync(hero);

      // Act
      await _heroService.DeleteAsync(createdHero.Id);

      // Assert
      var deletedHero = await _heroRepo.GetByIdAsync(createdHero.Id);
      Assert.Null(deletedHero);
    }

    [Fact]
    public async Task DeleteHeroAsync_ShouldThrowException_WhenHeroDoesNotExist()
    {
      // Act & Assert
      await Assert.ThrowsAsync<ArgumentException>(() => _heroService.DeleteAsync(999));
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnEmptyPage_WhenNoHeroesExist()
    {
      var result = await _heroService.GetPagedAsync(new HeroQueryParameters());

      Assert.Empty(result.Items);
      Assert.Equal(0, result.TotalCount);
      Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedAndSortedResults()
    {
      await _heroRepo.AddAsync(new Hero { Name = "Diana Prince", HeroName = "Wonder Woman", Birthdate = DateTime.Now.AddYears(-29), Height = 1.80, Weight = 74 });
      await _heroRepo.AddAsync(new Hero { Name = "Arthur Curry", HeroName = "Aquaman", Birthdate = DateTime.Now.AddYears(-32), Height = 1.90, Weight = 100 });
      await _heroRepo.AddAsync(new Hero { Name = "Bruce Wayne", HeroName = "Batman", Birthdate = DateTime.Now.AddYears(-35), Height = 1.88, Weight = 85 });

      var queryParameters = new HeroQueryParameters { Page = 1, PageSize = 2, SortBy = HeroSortBy.HeroName };

      var result = await _heroService.GetPagedAsync(queryParameters);

      Assert.Equal(3, result.TotalCount);
      Assert.Equal(2, result.Items.Count());
      Assert.Equal(2, result.TotalPages);
      Assert.Equal("Aquaman", result.Items.First().HeroName);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByName()
    {
      await _heroRepo.AddAsync(new Hero { Name = "Clark Kent", HeroName = "Superman", Birthdate = DateTime.Now.AddYears(-30), Height = 1.85, Weight = 80 });
      await _heroRepo.AddAsync(new Hero { Name = "Bruce Wayne", HeroName = "Batman", Birthdate = DateTime.Now.AddYears(-35), Height = 1.88, Weight = 85 });

      var result = await _heroService.GetPagedAsync(new HeroQueryParameters { Name = "Clark" });

      Assert.Equal(1, result.TotalCount);
      Assert.Equal("Superman", result.Items.Single().HeroName);
    }

    public void Dispose()
    {
      _context.Dispose();
      GC.SuppressFinalize(this);
    }
  }
}

