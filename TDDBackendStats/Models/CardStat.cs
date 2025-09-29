namespace TDDBackendStats.Models
{
    public class GameStat
    {
        public int Id { get; set; }
        public string SteamName { get; set; } = string.Empty;
        public string StartingClass { get; set; } = string.Empty;
        public bool Win { get; set; }
        public int DifficultyLevel { get; set; }
        public int ExtractionsUsed { get; set; }
        public int DeckSize { get; set; }
        public string EnemyThatKilled { get; set; } = string.Empty;
        public List<string> RelicsPicked { get; set; } = new List<string>();
        public List<string> CharmsPicked { get; set; } = new List<string>();
        public List<string> CardsPicked { get; set; } = new List<string>();
        public string TimePlayed { get; set; }
        public int ShopsVisited { get; set; }
        public int EndingScore { get; set; }
    }

    public class CardPickStat
    {
        public int Id { get; set; }
        public string CardName { get; set; } = string.Empty;
        public int TimesPicked { get; set; }
    }
}
