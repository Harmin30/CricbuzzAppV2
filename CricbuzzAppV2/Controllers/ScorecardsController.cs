using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CricbuzzAppV2.Helpers;


namespace CricbuzzAppV2.Controllers
{
    public class ScorecardsController : Controller
    {
        private readonly ApplicationDbContext _context;


        public ScorecardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Scorecards
        public async Task<IActionResult> Index()
        {
            var scorecards = await _context.Scorecards
                .Include(s => s.Match)
                    .ThenInclude(m => m.TeamA)
                .Include(s => s.Match)
                    .ThenInclude(m => m.TeamB)
                .Include(s => s.Player)
                .ToListAsync();

            return View(scorecards);
        }

        // GET: Scorecards/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var scorecard = await _context.Scorecards
                .Include(s => s.Match).ThenInclude(m => m.TeamA)
                .Include(s => s.Match).ThenInclude(m => m.TeamB)
                .Include(s => s.Player)
                .FirstOrDefaultAsync(s => s.ScorecardId == id);

            if (scorecard == null) return NotFound();

            return View(scorecard);
        }

        // GET: Scorecards/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: Scorecards/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Scorecard scorecard)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                Console.WriteLine($"Model error: {error.ErrorMessage}");

            if (ModelState.IsValid)
            {
                // Set MatchType for display
                var match = await _context.Matches.FindAsync(scorecard.MatchId);
                scorecard.MatchType = match?.MatchType;

                _context.Scorecards.Add(scorecard);
                await _context.SaveChangesAsync();

                // Update player stats
                await AppHelper.UpdatePlayerStats(_context, scorecard);

                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(scorecard);
            return View(scorecard);
        }


        // GET: Scorecards/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var scorecard = await _context.Scorecards
                .Include(s => s.Match).ThenInclude(m => m.TeamA)
                .Include(s => s.Match).ThenInclude(m => m.TeamB)
                .Include(s => s.Player)
                .FirstOrDefaultAsync(s => s.ScorecardId == id);

            if (scorecard == null) return NotFound();

            return View(scorecard);
        }

        // POST: Scorecards/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var scorecard = await _context.Scorecards
                .Include(s => s.Player)
                .Include(s => s.Match).ThenInclude(m => m.TeamA)
                .Include(s => s.Match).ThenInclude(m => m.TeamB)
                .FirstOrDefaultAsync(s => s.ScorecardId == id);

            if (scorecard != null)
            {
                // Reverse stats
                var reverseScorecard = new Scorecard
                {
                    PlayerId = scorecard.PlayerId,
                    RunsScored = -(scorecard.RunsScored ?? 0),
                    BallsFaced = -(scorecard.BallsFaced ?? 0),
                    WicketsTaken = -(scorecard.WicketsTaken ?? 0),
                    RunsConceded = -(scorecard.RunsConceded ?? 0),
                    OversBowled = -(scorecard.OversBowled ?? 0),
                    MatchId = scorecard.MatchId
                };

                await AppHelper.UpdatePlayerStats(_context, reverseScorecard);

                _context.Scorecards.Remove(scorecard);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"🗑 Deleted scorecard: {scorecard.Player?.FullName} ({scorecard.Match?.TeamA?.TeamName} vs {scorecard.Match?.TeamB?.TeamName})";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Scorecards/DeleteSelected
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["ErrorMessages"] = new List<string> { "No scorecards selected for deletion." };
                return RedirectToAction(nameof(Index));
            }

            var scorecards = await _context.Scorecards
                .Include(s => s.Player)
                .Include(s => s.Match).ThenInclude(m => m.TeamA)
                .Include(s => s.Match).ThenInclude(m => m.TeamB)
                .Where(s => selectedIds.Contains(s.ScorecardId))
                .ToListAsync();

            var deletedScores = new List<string>();

            foreach (var score in scorecards)
            {
                var reverseScorecard = new Scorecard
                {
                    PlayerId = score.PlayerId,
                    RunsScored = -(score.RunsScored ?? 0),
                    BallsFaced = -(score.BallsFaced ?? 0),
                    WicketsTaken = -(score.WicketsTaken ?? 0),
                    RunsConceded = -(score.RunsConceded ?? 0),
                    OversBowled = -(score.OversBowled ?? 0),
                    MatchId = score.MatchId
                };
                await AppHelper.UpdatePlayerStats(_context, reverseScorecard);

                deletedScores.Add($"{score.Player?.FullName} ({score.Match?.TeamA?.TeamName} vs {score.Match?.TeamB?.TeamName})");

                _context.Scorecards.Remove(score);
            }

            await _context.SaveChangesAsync();

            if (deletedScores.Any())
                TempData["SuccessMessage"] = $"🗑 Deleted scorecards: {string.Join(", ", deletedScores)}";

            return RedirectToAction(nameof(Index));
        }







        #region Helper Methods
        private void PopulateDropdowns(Scorecard scorecard = null)
        {
            // Matches dropdown: "TeamA vs TeamB (MatchType)"
            var matches = _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .ToList();

            ViewBag.MatchId = new SelectList(
                matches,
                "MatchId",
                "DisplayNameWithType", // updated property to include MatchType
                scorecard?.MatchId
            );

            // Players dropdown
            ViewBag.PlayerId = new SelectList(
                _context.Players.ToList(),
                "PlayerId",
                "FullName",
                scorecard?.PlayerId
            );

            // HowOut options
            ViewBag.HowOutOptions = new SelectList(new List<string>
    {
        "Bowled",
        "Caught",
        "LBW",
        "Run Out",
        "Stumped",
        "Hit Wicket",
        "Retired Hurt",
        "Not Out"
    }, scorecard?.HowOut);
        }
        #endregion

    }
}
