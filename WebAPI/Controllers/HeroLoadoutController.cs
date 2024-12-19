using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [ApiController]
    public class HeroLoadoutController : Controller
    {
        private readonly HeroLoadoutService _heroLoadoutService;

        public HeroLoadoutController(HeroLoadoutService heroLoadoutService)
        {
            _heroLoadoutService = heroLoadoutService;
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] HeroLoadoutDTO heroLoadoutDTO)
        {
            

            if (heroLoadoutDTO == null)
            {
                return BadRequest("Hero loadout cannot be null.");
            }
            try
            {
                var createdHeroLoadout = await _heroLoadoutService.CreateHeroLoadoutAsync(heroLoadoutDTO);
                return Ok("Loadout Created Successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while creating the hero loadout.",
                    Error = ex.Message
                });
            }
        }

        [Authorize(Roles = "Administrator,user")]
        [HttpGet]
        public async Task<ActionResult<List<HeroLoadout>>> GetAll()
        {
            var heroLoadouts = await _heroLoadoutService.GetAllHeroLoadoutsAsync();
            if (heroLoadouts == null || !heroLoadouts.Any())
            {
                return NotFound("No Hero Loadouts Registered!");
            }
            return Ok(heroLoadouts);
        }

        [HttpGet("commander/{commander}")]
        public async Task<ActionResult<List<HeroLoadout>>> GetByCommander(string commander)
        {
            if (string.IsNullOrWhiteSpace(commander) || commander.Length > 20 || !Regex.IsMatch(commander, "^[A-Za-z ]+$"))
            {
                return BadRequest("Commander name can only contain letters, spaces, and must not exceed 20 characters.");
            }
            var loadouts = await _heroLoadoutService.GetHeroLoadoutsByCommanderAsync(commander);
            if (loadouts == null || !loadouts.Any())
            {
                return NotFound($"No loadouts found for Commander: {commander}");
            }
            return Ok(loadouts);
        }

        [Authorize(Roles = "Administrator,user")]
        [HttpGet("{id}")]
        public async Task<ActionResult<HeroLoadout>> GetById(int id)
        {
            var heroLoadout = await _heroLoadoutService.GetHeroLoadoutByIdAsync(id);
            if (heroLoadout == null)
            {
                return NotFound($"Hero loadout with ID {id} not found.");
            }
            return Ok(heroLoadout);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] HeroLoadout heroLoadout)
        {
            if (id != heroLoadout.ID)
            {
                return BadRequest("Hero loadout ID mismatch.");
            }
            var updatedHeroLoadout = await _heroLoadoutService.UpdateHeroLoadoutAsync(heroLoadout);
            if (updatedHeroLoadout == null)
            {
                return NotFound($"Hero loadout with ID {id} not found.");
            }
            return Ok("Update Successful!");
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _heroLoadoutService.DeleteHeroLoadoutAsync(id);
            if (!deleted)
            {
                return NotFound($"Hero loadout with ID {id} not found.");
            }
            return Ok("Hero Loadout Deleted!"); 
        }
    }
}

