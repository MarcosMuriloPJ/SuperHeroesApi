using SuperHeroesApi.Domain.Entities;
using SuperHeroesApi.Application.DTOs;

namespace SuperHeroesApi.Application.Mappers
{
  public static class HeroMapper
  {
    public static HeroDto ToDto(Hero hero)
    {
      return new HeroDto
      {
        Id = hero.Id,
        Name = hero.Name,
        HeroName = hero.HeroName,
        Birthdate = hero.Birthdate,
        Height = hero.Height,
        Weight = hero.Weight,
        Superpowers = [.. hero.HerosSuperpowers
          .Select(hs => new SuperpowerDto
          {
            Id = hs.Superpower.Id,
            Name = hs.Superpower.Name,
            Description = hs.Superpower.Description
          })]
      };
    }

    public static Hero ToEntity(CreateHeroDto dto)
    {
      return new Hero
      {
        Name = dto.Name,
        HeroName = dto.HeroName,
        Birthdate = dto.Birthdate,
        Height = dto.Height,
        Weight = dto.Weight,
        HerosSuperpowers = [.. dto.SuperpowersIds
          .Select(id => new HeroSuperpower
          {
            SuperpowerId = id
          })]
      };
    }

    public static Hero ToEntity(int id, UpdateHeroDto dto)
    {
      return new Hero
      {
        Id = id,
        Name = dto.Name,
        HeroName = dto.HeroName,
        Birthdate = dto.Birthdate,
        Height = dto.Height,
        Weight = dto.Weight,
        HerosSuperpowers = [.. dto.SuperpowersIds
          .Select(superpowerId => new HeroSuperpower
          {
            HeroId = id,
            SuperpowerId = superpowerId
          })]
      };
    }
  }
}