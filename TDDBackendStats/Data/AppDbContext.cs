using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TDDBackendStats.Models;


namespace CardGameStatsAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<GameStat> GameStats { get; set; }
        public DbSet<CardPickStat> CardPickStats { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    // Convert List<string> to JSON for storage
        //    modelBuilder.Entity<GameStat>()
        //        .Property(g => g.RelicsPicked)
        //        .HasConversion(
        //            v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
        //            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null)
        //        );

        //    modelBuilder.Entity<GameStat>()
        //        .Property(g => g.CharmsPicked)
        //        .HasConversion(
        //            v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
        //            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null)
        //        );

        //    modelBuilder.Entity<GameStat>()
        //        .Property(g => g.CardsPicked)
        //        .HasConversion(
        //            v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
        //            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null)
        //        );
        //}
    }

    //public class GameStat
    //{
    //    public int Id { get; set; }
    //    public string SteamName { get; set; } = string.Empty;
    //    public string StartingClass { get; set; } = string.Empty;
    //    public bool Win { get; set; }
    //    public int LayerReached { get; set; }
    //    public int ExtractionsUsed { get; set; }
    //    public int DeckSize { get; set; }
    //    public string EnemyThatKilled { get; set; } = string.Empty;
    //    public List<string> RelicsPicked { get; set; } = new List<string>();
    //    public List<string> CharmsPicked { get; set; } = new List<string>();
    //    public List<string> CardsPicked { get; set; } = new List<string>();
    //}

    //public class CardPickStat
    //{
    //    public int Id { get; set; } // EF Core requires a primary key
    //    public string CardName { get; set; } = string.Empty;
    //    public int TimesPicked { get; set; }
    //}
}
