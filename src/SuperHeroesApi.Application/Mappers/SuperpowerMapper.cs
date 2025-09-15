using SuperHeroesApi.Domain.Entities;
using SuperHeroesApi.Application.DTOs;

namespace SuperHeroesApi.Application.Mappers
{
  public static class SuperpowerMapper
  {
    public static SuperpowerDto ToDto(Superpower superpower)
    {
      return new SuperpowerDto
      {
        Id = superpower.Id,
        Name = superpower.Name,
        Description = superpower.Description
      };
    }
  }
}