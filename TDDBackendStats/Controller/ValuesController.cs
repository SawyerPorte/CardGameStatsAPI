using CardGameStatsAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TDDBackendStats.Models;

namespace TDDBackendStats.Controller
{
    [ApiController]
    [Route("api/[controller]")]

    public class ValuesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ValuesController(AppDbContext context)
        {
            _context = context;
        }

        // -------------------------
        // 1. Record a Full Game Run
        // -------------------------
        [HttpPost]
        public async Task<IActionResult> PostGameStat([FromBody] GameStat stat)
        {
            if (!ModelState.IsValid)
            {
                // This will return a 400 with details about what didn’t bind correctly
                return BadRequest(new { errors = ModelState });
            }


            _context.GameStats.Add(stat);
            await _context.SaveChangesAsync();
            return Ok(stat);
           
        }

        // -------------------------
        // 2. Get All Game Runs
        // -------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var stats = await _context.GameStats.ToListAsync();
            return Ok(stats);
        }

        // -------------------------
        // 4. Get Global Card Pick Stats
        // -------------------------
        [HttpGet("card-picks")]
        public async Task<IActionResult> GetCardPicks()
        {
            var allCards = await _context.GameStats
                .SelectMany(s => s.CardsPicked)
                .ToListAsync();

            var cardStats = allCards
                .GroupBy(c => c)
                .Select(g => new CardPickStat
                {
                    CardName = g.Key,
                    TimesPicked = g.Count()
                })
                .OrderByDescending(c => c.TimesPicked)
                .ToList();

            return Ok(cardStats);
        }

        //// -------------------------
        //// 5. Get Card Picks by Class (with optional win filter)
        //// -------------------------
        [HttpGet("card-picks/by-class")]
        public async Task<IActionResult> GetCardPicksByClass(string playerClass, bool? onlyWins = null)
        {
            var filteredStats = _context.GameStats
                .Where(s => s.StartingClass.Equals(playerClass, StringComparison.OrdinalIgnoreCase));

            if (onlyWins == true)
            {
                filteredStats = filteredStats.Where(s => s.Win);
            }

            var cardStats = await filteredStats
                .SelectMany(s => s.CardsPicked)
                .GroupBy(c => c)
                .Select(g => new CardPickStat
                {
                    CardName = g.Key,
                    TimesPicked = g.Count()
                })
                .OrderByDescending(c => c.TimesPicked)
                .ToListAsync();

            return Ok(cardStats);
        }
    }
}