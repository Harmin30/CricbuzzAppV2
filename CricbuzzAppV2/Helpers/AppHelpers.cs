using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;

namespace CricbuzzAppV2.Helpers
{
    public static class AppHelper
    {
        #region Dropdowns
        public static SelectList PlayerSelectList(ApplicationDbContext context, int? selectedId = null)
        {
            var players = context.Players.OrderBy(p => p.FullName).ToList();
            return new SelectList(players, "PlayerId", "FullName", selectedId);
        }

        public static SelectList TeamSelectList(ApplicationDbContext context, int? selectedId = null)
        {
            var teams = context.Teams.OrderBy(t => t.TeamName).ToList();
            return new SelectList(teams, "TeamId", "TeamName", selectedId);
        }

        public static SelectList MatchSelectList(ApplicationDbContext context, int? selectedId = null)
        {
            var matches = context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .ToList();
            return new SelectList(matches, "MatchId", "DisplayName", selectedId);
        }

        public static SelectList CricketFormatSelectList(PlayerStats.CricketFormat? selectedFormat = null)
        {
            return new SelectList(Enum.GetValues(typeof(PlayerStats.CricketFormat)), selectedFormat);
        }

        public static SelectList HowOutSelectList(string selectedValue = null)
        {
            var options = new List<string>
            {
                "Bowled", "Caught", "LBW", "Run Out", "Stumped", "Hit Wicket", "Retired Hurt", "Not Out"
            };
            return new SelectList(options, selectedValue);
        }
        #endregion

        #region TempData
        public static void SetSuccess(Controller controller, string message) => controller.TempData["Success"] = message;
        public static void SetError(Controller controller, string message) => controller.TempData["Error"] = message;
        #endregion

        #region Include Helpers
        public static IQueryable<Player> IncludeTeam(IQueryable<Player> query) => query.Include(p => p.Team);
        public static IQueryable<PlayerStats> IncludePlayer(IQueryable<PlayerStats> query) => query.Include(p => p.Player);
        public static IQueryable<Scorecard> IncludeMatchPlayerTeams(IQueryable<Scorecard> query) =>
            query.Include(s => s.Match).ThenInclude(m => m.TeamA)
                 .Include(s => s.Match).ThenInclude(m => m.TeamB)
                 .Include(s => s.Player);
        #endregion

        #region Generic CRUD
        public static async Task<bool> DeleteEntity<TEntity>(DbContext context, int id) where TEntity : class
        {
            var entity = await context.Set<TEntity>().FindAsync(id);
            if (entity == null) return false;
            context.Set<TEntity>().Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public static async Task<TEntity> FindOrNotFound<TEntity>(DbContext context, int id) where TEntity : class
        {
            return await context.Set<TEntity>().FindAsync(id);
        }
        #endregion

        #region PlayerStats Updates
        /// <summary>
        /// Updates player stats for Create/Edit/Delete
        /// Pass oldScorecard for edits, null for create.
        /// For deletion, pass a Scorecard with negative values.
        /// </summary>
        public static async Task UpdatePlayerStats(ApplicationDbContext context, Scorecard scorecard, Scorecard oldScorecard = null)
        {
            if (scorecard == null || scorecard.PlayerId == 0 || scorecard.MatchId == 0)
                return;

            var match = await context.Matches.FindAsync(scorecard.MatchId);
            if (match == null) return;

            if (!Enum.TryParse<PlayerStats.CricketFormat>(match.MatchType, out var format))
                return;

            var stats = await context.PlayerStats
                .FirstOrDefaultAsync(ps => ps.PlayerId == scorecard.PlayerId && ps.Format == format);

            if (stats == null)
            {
                stats = new PlayerStats
                {
                    PlayerId = scorecard.PlayerId,
                    Format = format,
                    MatchesPlayed = 0,
                    Innings = 0,
                    Runs = 0,
                    BallsFaced = 0,
                    BallsBowled = 0,
                    RunsConceded = 0,
                    Wickets = 0
                };
                context.PlayerStats.Add(stats);
            }

            // Calculate deltas
            int deltaRuns = (scorecard.RunsScored ?? 0) - (oldScorecard?.RunsScored ?? 0);
            int deltaBalls = (scorecard.BallsFaced ?? 0) - (oldScorecard?.BallsFaced ?? 0);
            int deltaWickets = (scorecard.WicketsTaken ?? 0) - (oldScorecard?.WicketsTaken ?? 0);
            int deltaRunsConceded = (scorecard.RunsConceded ?? 0) - (oldScorecard?.RunsConceded ?? 0);
            int deltaBallsBowled = (int)((scorecard.OversBowled ?? 0) * 6 - (oldScorecard?.OversBowled ?? 0) * 6);


            // Update batting stats
            if (scorecard.RunsScored.HasValue || scorecard.BallsFaced.HasValue)
            {
                if (oldScorecard == null)
                {
                    stats.MatchesPlayed = (stats.MatchesPlayed ?? 0) + 1;
                    stats.Innings = (stats.Innings ?? 0) + 1;

                    int runs = scorecard.RunsScored ?? 0;

                    // Update milestones
                    if (runs >= 200) stats.DoubleHundreds = (stats.DoubleHundreds ?? 0) + 1;
                    else if (runs >= 100) stats.Hundreds = (stats.Hundreds ?? 0) + 1;
                    else if (runs >= 50) stats.Fifties = (stats.Fifties ?? 0) + 1;
                }
                else
                {
                    // For edits, adjust milestones
                    int oldRuns = oldScorecard.RunsScored ?? 0;
                    int newRuns = scorecard.RunsScored ?? 0;

                    // Remove old milestone
                    if (oldRuns >= 200) stats.DoubleHundreds = (stats.DoubleHundreds ?? 0) - 1;
                    else if (oldRuns >= 100) stats.Hundreds = (stats.Hundreds ?? 0) - 1;
                    else if (oldRuns >= 50) stats.Fifties = (stats.Fifties ?? 0) - 1;

                    // Add new milestone
                    if (newRuns >= 200) stats.DoubleHundreds = (stats.DoubleHundreds ?? 0) + 1;
                    else if (newRuns >= 100) stats.Hundreds = (stats.Hundreds ?? 0) + 1;
                    else if (newRuns >= 50) stats.Fifties = (stats.Fifties ?? 0) + 1;
                }

                stats.Runs = (stats.Runs ?? 0) + deltaRuns;
                stats.BallsFaced = (stats.BallsFaced ?? 0) + deltaBalls;

                // Update HighestScore
                if ((scorecard.RunsScored ?? 0) > (stats.HighestScore ?? 0))
                {
                    stats.HighestScore = scorecard.RunsScored;
                }


                // Update bowling stats
                if (scorecard.WicketsTaken.HasValue || scorecard.RunsConceded.HasValue || scorecard.OversBowled.HasValue)
                {
                    stats.BallsBowled = (stats.BallsBowled ?? 0) + deltaBallsBowled;
                    stats.RunsConceded = (stats.RunsConceded ?? 0) + deltaRunsConceded;
                    stats.Wickets = (stats.Wickets ?? 0) + deltaWickets;
                }

                await context.SaveChangesAsync();
            }
            #endregion
        }
    }
}
