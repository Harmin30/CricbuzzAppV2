using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
            {
                Console.WriteLine($"Model error: {error.ErrorMessage}");
            }
            if (ModelState.IsValid)
            {
                _context.Scorecards.Add(scorecard);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If validation fails, repopulate dropdowns
            PopulateDropdowns(scorecard);
            return View(scorecard);
        }

        // GET: Scorecards/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var scorecard = await _context.Scorecards.FindAsync(id);
            if (scorecard == null) return NotFound();

            PopulateDropdowns(scorecard);
            return View(scorecard);
        }

        // POST: Scorecards/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Scorecard scorecard)
        {
            if (id != scorecard.ScorecardId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(scorecard);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Scorecards.Any(e => e.ScorecardId == id))
                        return NotFound();
                    else
                        throw;
                }
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

        // POST: Scorecards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var scorecard = await _context.Scorecards.FindAsync(id);
            if (scorecard != null)
            {
                _context.Scorecards.Remove(scorecard);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        #region Helper Methods
        private void PopulateDropdowns(Scorecard scorecard = null)
        {
            // Matches dropdown: "TeamA vs TeamB"
            var matches = _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .ToList();

            ViewBag.MatchId = new SelectList(
                matches,
                "MatchId",
                "DisplayName", // computed property in Match: TeamA vs TeamB
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
