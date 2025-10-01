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



        [HttpGet("global-stats")]
        public async Task<IActionResult> GetGlobalStatsDetailed()
        {
            var allStats = await _context.GameStats.ToListAsync();

            if (!allStats.Any())
                return Ok(new { }); // no data

            // Helper to group and count a list of strings
            List<object> GroupAndCount(IEnumerable<string> items, int topN = 3)
            {
                return items
                .Where(s => !string.IsNullOrEmpty(s))
                .GroupBy(x => x)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(topN)
                .ToList<object>();
            }

            // Cards
            var allCards = allStats.SelectMany(s => s.CardsPicked).ToList();
            var cardStats = GroupAndCount(allCards);

            // Relics
            var allRelics = allStats.SelectMany(s => s.RelicsPicked).ToList();
            var relicStats = GroupAndCount(allRelics);

            // Charms
            var allCharms = allStats.SelectMany(s => s.CharmsPicked).ToList();
            var charmStats = GroupAndCount(allCharms);

            // Hero Powers
            var allHeroPowers = allStats.Select(s => s.HeroPower).Where(h => !string.IsNullOrEmpty(h)).ToList();
            var heroPowerStats = GroupAndCount(allHeroPowers);

            // Enemies
            // Enemies (split on commas)
            var allEnemies = allStats
                .SelectMany(s => s.EnemyThatKilled?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
                .Select(e => e.Trim()) // remove extra spaces
                .Where(e => !string.IsNullOrEmpty(e))
                .ToList();

            var enemyStats = GroupAndCount(allEnemies);

            // Classes
            var allClasses = allStats.Select(s => s.StartingClass).Where(c => !string.IsNullOrEmpty(c)).ToList();
            var classStats = GroupAndCount(allClasses);

            // Wins by Class
            var winsByClass = allStats
                .GroupBy(s => s.StartingClass)
                .ToDictionary(g => g.Key, g => g.Count(s => s.Win));

            // Average Deck Size
            var avgDeckSize = allStats.Any() ? allStats.Average(s => s.DeckSize) : 0;

            // Average Run Time (assuming TimePlayed is in seconds as string)
            var timeSpans = allStats
            .Select(s =>
            {
                bool success = TimeSpan.TryParse(s.TimePlayed, out var ts);
                return success ? ts : (TimeSpan?)null;
            })
            .Where(ts => ts.HasValue)
            .Select(ts => ts.Value)
            .ToList();

            // Calculate average TimeSpan
            TimeSpan avgRunTime = TimeSpan.Zero;
            if (timeSpans.Any())
            {
                long totalTicks = timeSpans.Sum(ts => ts.Ticks);
                avgRunTime = new TimeSpan(totalTicks / timeSpans.Count);
            }

            // Format as string (HH:mm:ss)
            string avgRunTimeStr = avgRunTime.ToString(@"hh\:mm\:ss");

            // Highest Score
            var highestScoreEntry = allStats
                .OrderByDescending(s => s.EndingScore)
                .Select(s => new { s.SteamName, s.EndingScore })
                .FirstOrDefault();


            // Wins / Attempts by Class
            var winRateByClass = allStats
                .GroupBy(s => s.StartingClass)
                .Select(g => new
                {
                    Class = g.Key,
                    Wins = g.Count(s => s.Win),
                    Attempts = g.Count(),
                    WinRate = g.Count() > 0 ? (double)g.Count(s => s.Win) / g.Count() * 100 : 0
                })
                .ToList();

            // Wins / Attempts by Difficulty
            var winRateByDifficulty = allStats
                .GroupBy(s => s.DifficultyLevel) // assuming you have DifficultyLevel in your GameStats model
                .Select(g => new
                {
                    Difficulty = g.Key,
                    Wins = g.Count(s => s.Win),
                    Attempts = g.Count(),
                    WinRate = g.Count() > 0 ? (double)g.Count(s => s.Win) / g.Count() * 100 : 0
                })
                .OrderBy(x => x.Difficulty)
                .ToList();

            return Ok(new
            {
                cards = cardStats,
                relics = relicStats,
                charms = charmStats,
                heroPowers = heroPowerStats,
                enemies = enemyStats,
                classes = classStats,
                topClass = classStats.FirstOrDefault() ?? new { Name = "NEED DATA", Count = 0 },
                winsByClass = winsByClass,
                avgDeckSize = avgDeckSize,
                mostDeadlyEnemy = enemyStats.FirstOrDefault() ?? new { Name = "NEED DATA", Count = 0 },
                topRelic = relicStats.FirstOrDefault() ?? new { Name = "NEED DATA", Count = 0 },
                topCharm = charmStats.FirstOrDefault() ?? new { Name = "NEED DATA", Count = 0 },
                topCard = cardStats.FirstOrDefault() ?? new { Name = "NEED DATA", Count = 0 },
                topHeroPower = heroPowerStats.FirstOrDefault() ?? new { Name = "NEED DATA", Count = 0 },
                avgRunTime = avgRunTime,
                highestScore = highestScoreEntry ?? new { SteamName = "NEED DATA", EndingScore = 0 },
                winRateByClass = winRateByClass,
                winRateByDifficulty = winRateByDifficulty
            });
        }
    }
}