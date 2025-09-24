using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TDDBackendStats.Models;

namespace TDDBackendStats.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // Store all game runs in memory
        private static List<GameStat> _stats = new List<GameStat>();

        // Store global card pick stats (fast lookup with dictionary)
        private static Dictionary<string, CardPickStat> _cardStats = new Dictionary<string, CardPickStat>();


        // -------------------------
        // 1. Record a Full Game Run
        // -------------------------
        [HttpPost]
        public IActionResult PostGameStat(GameStat stat)
        {
            stat.Id = _stats.Count + 1;
            _stats.Add(stat);

            // Update global card pick stats from this run
            foreach (var card in stat.CardsPicked)
            {
                if (_cardStats.ContainsKey(card))
                {
                    _cardStats[card].TimesPicked++;
                }
                else
                {
                    _cardStats[card] = new CardPickStat
                    {
                        CardName = card,
                        TimesPicked = 1
                    };
                }
            }

            return Ok(stat);
        }


        // -------------------------
        // 2. Get All Game Runs
        // -------------------------
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_stats);
        }

        // -------------------------
        // 4. Get Global Card Pick Stats
        // -------------------------
        [HttpGet("card-picks")]
        public IActionResult GetCardPicks()
        {
            return Ok(_cardStats.Values.OrderByDescending(c => c.TimesPicked));
        }

        // -------------------------
        // 5. Get Card Picks by Class (with optional win filter)
        // -------------------------
        [HttpGet("card-picks/by-class")]
        public IActionResult GetCardPicksByClass(string playerClass, bool? onlyWins = null)
        {
            var filteredStats = _stats
                .Where(s => s.StartingClass.Equals(playerClass, StringComparison.OrdinalIgnoreCase));

            if (onlyWins == true)
            {
                filteredStats = filteredStats.Where(s => s.Win);
            }

            var cardStats = filteredStats
                .SelectMany(s => s.CardsPicked)
                .GroupBy(c => c)
                .Select(g => new CardPickStat
                {
                    CardName = g.Key,
                    TimesPicked = g.Count()
                })
                .OrderByDescending(c => c.TimesPicked);

            return Ok(cardStats);
        }

    }
}
