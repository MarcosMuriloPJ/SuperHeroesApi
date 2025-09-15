using Microsoft.AspNetCore.Mvc;
using SuperHeroesApi.Application.DTOs;
using SuperHeroesApi.Application.Services;

namespace SuperHeroesApi.WebAPI.Controllers
{
  /// <summary>
  /// Controller para gerenciamento de superpoderes
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class SuperpowersController(ISuperpowerService superpowerService) : ControllerBase
  {
    /// <summary>
    /// Serviço de superpoderes injetado por DI
    /// </summary>
    private readonly ISuperpowerService _superpowerService = superpowerService;

    /// <summary>
    /// Obtém todos os superpoderes disponíveis
    /// </summary>
    /// <returns>Lista de superpoderes</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SuperpowerDto>>> GetAllSuperpowers()
    {
      try
      {
        var superpowers = await _superpowerService.GetAllAsync();
        return Ok(superpowers);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
      }
    }
  }
}

