using Microsoft.AspNetCore.Mvc;
using SuperHeroesApi.Application.DTOs;
using SuperHeroesApi.Application.Services;

namespace SuperHeroesApi.WebAPI.Controllers
{
  /// <summary>
  /// Controller para gerenciamento de super-heróis
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class HeroesController(IHeroService heroService) : ControllerBase
  {
    /// <summary>
    /// Serviço de heróis injetado por DI
    /// </summary>
    private readonly IHeroService _heroService = heroService;


    /// <summary>
    /// Obtém todos os super-heróis cadastrados
    /// </summary>
    /// <returns>Lista de super-heróis</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HeroDto>>> GetAllHeroes()
    {
      try
      {
        var heroes = await _heroService.GetAllAsync();

        if (!heroes.Any())
        {
          return Ok(new { message = "Nenhum super-herói encontrado", data = heroes });
        }

        return Ok(heroes);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
      }
    }

    /// <summary>
    /// Obtém um super-herói por ID
    /// </summary>
    /// <param name="id">ID do super-herói</param>
    /// <returns>Super-herói encontrado</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<HeroDto>> GetHeroById(int id)
    {
      try
      {
        if (id <= 0)
        {
          return BadRequest(new { message = "ID inválido" });
        }

        var hero = await _heroService.GetByIdAsync(id);

        if (hero == null)
        {
          return NotFound(new { message = $"Super-herói com ID {id} não encontrado" });
        }

        return Ok(hero);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
      }
    }

    /// <summary>
    /// Cadastra um novo super-herói
    /// </summary>
    /// <param name="createHeroDto">Dados do super-herói a ser criado</param>
    /// <returns>Super-herói criado</returns>
    [HttpPost]
    public async Task<ActionResult<HeroDto>> CreateHero([FromBody] CreateHeroDto createHeroDto)
    {
      try
      {
        if (!ModelState.IsValid)
        {
          return BadRequest(ModelState);
        }

        var hero = await _heroService.CreateAsync(createHeroDto);
        return Ok(new { message = "Super-herói atualizado com sucesso", data = hero });
      }
      catch (InvalidOperationException ex)
      {
        return Conflict(new { message = ex.Message });
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { message = ex.Message });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
      }
    }

    /// <summary>
    /// Atualiza um super-herói existente
    /// </summary>
    /// <param name="id">ID do super-herói</param>
    /// <param name="updateHeroDto">Dados atualizados do super-herói</param>
    /// <returns>Super-herói atualizado</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<HeroDto>> UpdateHero(int id, [FromBody] UpdateHeroDto updateHeroDto)
    {
      try
      {
        if (id <= 0)
        {
          return BadRequest(new { message = "ID inválido" });
        }

        if (!ModelState.IsValid)
        {
          return BadRequest(ModelState);
        }

        var hero = await _heroService.UpdateAsync(id, updateHeroDto);
        return Ok(new { message = "Super-herói atualizado com sucesso", data = hero });
      }
      catch (ArgumentException ex)
      {
        return NotFound(new { message = ex.Message });
      }
      catch (InvalidOperationException ex)
      {
        return Conflict(new { message = ex.Message });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
      }
    }

    /// <summary>
    /// Exclui um super-herói
    /// </summary>
    /// <param name="id">ID do super-herói</param>
    /// <returns>Confirmação de exclusão</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHero(int id)
    {
      try
      {
        if (id <= 0)
        {
          return BadRequest(new { message = "ID inválido" });
        }

        await _heroService.DeleteAsync(id);
        return Ok(new { message = "Super-herói excluído com sucesso" });
      }
      catch (ArgumentException ex)
      {
        return NotFound(new { message = ex.Message });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
      }
    }
  }
}

