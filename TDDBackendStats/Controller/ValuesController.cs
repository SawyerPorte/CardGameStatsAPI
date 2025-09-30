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
            // Get all CardsPicked lists from DB
            var allCardLists = await _context.GameStats
                .Select(s => s.CardsPicked) // List<string>
                .ToListAsync();

            // Flatten into one big list
            var allCards = allCardLists
                .Where(list => list != null && list.Count > 0)
                .SelectMany(list => list) // just flatten the lists
                .Select(c => c.Trim())    // trim spaces if needed
                .ToList();

            if (!allCards.Any())
                return Ok(new { Card = "", Count = 0 });

            // Group by card and count
            var mostCommon = allCards
                .GroupBy(c => c)
                .Select(g => new { Card = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .First();

            return Ok(new { Card = mostCommon.Card, Count = mostCommon.Count });
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

        [HttpGet("popular-hero-power")]
        public async Task<IActionResult> GetPopularHeroPower()
        {
            var heroPowers = await _context.GameStats
                .GroupBy(s => s.HeroPower)
                .Select(g => new { HeroPower = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .FirstOrDefaultAsync();

            return Ok(heroPowers);
        }

        // Player stats by Steam name
        [HttpGet("player-stats/{steamName}")]
        public async Task<IActionResult> GetPlayerStats(string steamName)
        {
            var games = await _context.GameStats
                .Where(s => s.SteamName == steamName)
                .ToListAsync();

            if (!games.Any()) return NotFound();

            var winRate = games.Count(g => g.Win) / (double)games.Count;
            var mostPlayedClass = games.GroupBy(g => g.StartingClass)
                                       .OrderByDescending(g => g.Count())
                                       .First().Key;

            return Ok(new
            {
                SteamName = steamName,
                WinRate = winRate,
                MostPlayedClass = mostPlayedClass
            });
        }
    }
}