using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using Microsoft.EntityFrameworkCore;

namespace CricbuzzAppV2.Helpers
{
    public class PlayerStatsService
    {
        private readonly ApplicationDbContext _context;

        public PlayerStatsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task UpdateBattingStatsAsync(BattingInningsScorecard card)
        {
            var innings = await _context.MatchInnings
                .Include(i => i.Match)
                .FirstAsync(i => i.MatchInningsId == card.MatchInningsId);

            var format = MapMatchTypeToFormat(innings.Match.MatchType);

            var stats = await _context.PlayerStats
                .FirstOrDefaultAsync(s =>
                    s.PlayerId == card.PlayerId &&
                    s.Format == format);

            if (stats == null)
            {
                stats = new PlayerStats
                {
                    PlayerId = card.PlayerId,
                    Format = format
                };
                _context.PlayerStats.Add(stats);
            }

            stats.MatchesPlayed ??= 0;
            stats.Innings ??= 0;
            stats.Runs ??= 0;
            stats.BallsFaced ??= 0;

            stats.MatchesPlayed += 1;
            stats.Innings += 1;
            stats.Runs += card.Runs;
            stats.BallsFaced += card.BallsFaced;

            stats.HighestScore = Math.Max(stats.HighestScore ?? 0, card.Runs);

            if (card.Runs >= 100) stats.Hundreds = (stats.Hundreds ?? 0) + 1;
            else if (card.Runs >= 50) stats.Fifties = (stats.Fifties ?? 0) + 1;

            await _context.SaveChangesAsync();
        }


        public async Task UpdateBowlingStatsAsync(BowlingScorecard card)
        {
            var innings = await _context.MatchInnings
                .Include(i => i.Match)
                .FirstAsync(i => i.MatchInningsId == card.MatchInningsId);

            var format = MapMatchTypeToFormat(innings.Match.MatchType);

            var stats = await _context.PlayerStats
                .FirstOrDefaultAsync(s =>
                    s.PlayerId == card.PlayerId &&
                    s.Format == format);

            if (stats == null)
            {
                stats = new PlayerStats
                {
                    PlayerId = card.PlayerId,
                    Format = format
                };
                _context.PlayerStats.Add(stats);
            }

            // Safe init
            stats.BallsBowled ??= 0;
            stats.RunsConceded ??= 0;
            stats.Wickets ??= 0;

            // Overs → balls (important)
            int balls = (int)(card.Overs * 6);

            stats.BallsBowled += balls;
            stats.RunsConceded += card.RunsConceded;
            stats.Wickets += card.Wickets;

            // Best bowling (simple version)
            string currentBest = $"{card.Wickets}/{card.RunsConceded}";
            stats.BestBowling ??= currentBest;

            await _context.SaveChangesAsync();
        }


        private PlayerStats.CricketFormat MapMatchTypeToFormat(string matchType)
        {
            return matchType?.ToUpper() switch
            {
                "TEST" => PlayerStats.CricketFormat.Test,
                "ODI" => PlayerStats.CricketFormat.ODI,
                "T20" => PlayerStats.CricketFormat.T20,
                _ => PlayerStats.CricketFormat.T20
            };
        }

    }
}
