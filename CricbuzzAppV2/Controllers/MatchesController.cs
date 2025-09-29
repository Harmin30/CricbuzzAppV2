using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using CricbuzzAppV2.Helpers;


namespace CricbuzzAppV2.Controllers
{
    public class MatchesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MatchesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Matches
        public async Task<IActionResult> Index()
        {
            var matches = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.WinnerTeam)
                .ToListAsync();
            return View(matches);
        }

        // GET: Matches/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var match = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.WinnerTeam)
                .FirstOrDefaultAsync(m => m.MatchId == id);

            if (match == null)
                return NotFound();

            return View(match);
        }

        // GET: Matches/Create
        public IActionResult Create()
        {
            ViewData["Teams"] = _context.Teams.ToList();
            return View();
        }

        // POST: Matches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Match match)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Model error: {error.ErrorMessage}");
            }

            if (ModelState.IsValid)
            {
                _context.Matches.Add(match);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Teams"] = _context.Teams.ToList();
            return View(match);
        }

        // GET: Matches/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null)
                return NotFound();

            ViewData["Teams"] = _context.Teams.ToList();
            return View(match);
        }

        // POST: Matches/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Match match)
        {
            if (id != match.MatchId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(match);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Matches.Any(m => m.MatchId == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewData["Teams"] = _context.Teams.ToList();
            return View(match);
        }

        // GET: Matches/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var match = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.WinnerTeam)
                .FirstOrDefaultAsync(m => m.MatchId == id);

            if (match == null)
                return NotFound();

            return View(match);
        }

        // POST: Matches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match != null)
            {
                _context.Matches.Remove(match);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                AppHelper.SetError(this, "No matches selected for deletion.");
                return RedirectToAction(nameof(Index));
            }

            var matches = await _context.Matches
                .Where(m => selectedIds.Contains(m.MatchId))
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .ToListAsync();

            var deletedMatches = new List<string>();
            var skippedMatches = new List<string>();

            foreach (var match in matches)
            {
                bool hasScorecards = await _context.Scorecards
                    .AnyAsync(s => s.MatchId == match.MatchId);

                if (hasScorecards)
                {
                    skippedMatches.Add($"❌ Match '{match.DisplayNameWithType}' cannot be deleted because it has scorecards.");
                    continue; // skip deletion
                }

                _context.Matches.Remove(match);
                deletedMatches.Add(match.DisplayNameWithType);
            }

            await _context.SaveChangesAsync();

            // ✅ Success messages
            if (deletedMatches.Any())
                AppHelper.SetSuccess(this, $"🗑 Deleted matches: {string.Join(", ", deletedMatches)}");

            // ✅ Error messages
            if (skippedMatches.Any())
                AppHelper.SetError(this, string.Join("<br/>", skippedMatches));

            return RedirectToAction(nameof(Index));
        }

    }
}
