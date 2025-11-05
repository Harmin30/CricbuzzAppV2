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
        private void NormalizeMatch(Match match)
        {
            match.MatchType = match.MatchType.Trim();
            match.Venue = match.Venue.Trim();
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

            var inningsList = await _context.MatchInnings
                .Include(i => i.BattingTeam)
                .Include(i => i.BowlingTeam)
                .Include(i => i.BattingScorecards).ThenInclude(b => b.Player)
                .Include(i => i.BowlingScorecards).ThenInclude(b => b.Player)
                .Where(i => i.MatchId == id)
                .OrderBy(i => i.InningsNumber)
                .ToListAsync();

            // 🔒 HARDEN: allow ONLY ONE live innings
            var liveInnings = inningsList
    .Where(i => i.Status == InningsStatus.InProgress && i.EndTime == null)
    .OrderByDescending(i => i.InningsNumber)
    .ToList();

            // 🔒 HARD FIX: no innings OR more than one = force close
            if (liveInnings.Count != 1)
            {
                foreach (var inn in liveInnings)
                {
                    inn.Status = InningsStatus.Completed;
                    inn.EndTime = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                ViewBag.CurrentInnings = null;
            }
            else
            {
                ViewBag.CurrentInnings = liveInnings.First();
            }



            // If exactly one live innings → allow it
            // Else (0 or >1) → treat as NO live innings
            ViewBag.CurrentInnings = liveInnings.Count == 1
                ? liveInnings.First()
                : null;


            var completedInnings = inningsList
                .Where(i => i.Status == InningsStatus.Completed)
                .ToList();

            ViewBag.CompletedInnings = completedInnings;

            // ✅ Calculate max innings
            int maxInnings = match.MaxInningsPerTeam > 0
                ? match.MaxInningsPerTeam * 2
                : (match.MatchType == "Test" ? 4 : 2);

            ViewBag.CanManageInnings = completedInnings.Count < maxInnings;

            return View(match);
        }




        // GET: Matches/Create
        public IActionResult Create()
        {
            ViewData["Teams"] = _context.Teams.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Match match)
        {
            NormalizeMatch(match);

            // ❌ Same team selected
            if (match.TeamAId == match.TeamBId)
            {
                ModelState.AddModelError("TeamBId",
                    "Team A and Team B cannot be the same.");
            }

            // ❌ Duplicate match check
            bool exists = await _context.Matches.AnyAsync(m =>
                m.TeamAId == match.TeamAId &&
                m.TeamBId == match.TeamBId &&
                m.Date.Date == match.Date.Date &&
                m.MatchType == match.MatchType &&
                m.Venue == match.Venue);

            if (exists)
            {
                ModelState.AddModelError(string.Empty,
                    "This match already exists with the same details.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Teams"] = _context.Teams.ToList();
                return View(match);
            }


            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            AppHelper.SetSuccess(this, "✅ Match created successfully.");
            return RedirectToAction("Details", new { id = match.MatchId });
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

            NormalizeMatch(match);

            bool exists = await _context.Matches.AnyAsync(m =>
                m.MatchId != match.MatchId &&
                m.TeamAId == match.TeamAId &&
                m.TeamBId == match.TeamBId &&
                m.Date.Date == match.Date.Date &&
                m.MatchType == match.MatchType &&
                m.Venue == match.Venue);

            if (exists)
            {
                ModelState.AddModelError(string.Empty,
                    "❌ Another match with the same details already exists.");
            }

            // ❌ Same team selected
            if (match.TeamAId == match.TeamBId)
            {
                ModelState.AddModelError("TeamBId",
                    "Team A and Team B cannot be the same.");
            }


            if (!ModelState.IsValid)
            {
                ViewData["Teams"] = _context.Teams.ToList();
                return View(match);
            }


            _context.Update(match);
            await _context.SaveChangesAsync();

            AppHelper.SetSuccess(this, "✏ Match updated successfully.");
            return RedirectToAction(nameof(Index));
        }



        // ============================
        // SINGLE DELETE (Unified Modal)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null)
                return NotFound();

            bool hasScorecards = await _context.Scorecards
                .AnyAsync(s => s.MatchId == id);

            if (hasScorecards)
            {
                AppHelper.SetError(this,
                    $"❌ Match '{match.DisplayNameWithType}' cannot be deleted because it has scorecards.");
                return RedirectToAction(nameof(Index));
            }

            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();

            AppHelper.SetSuccess(this,
                $"🗑 Match '{match.DisplayNameWithType}' deleted successfully.");

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


        private async Task CalculateWinnerAsync(int matchId)
        {
            var match = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .FirstOrDefaultAsync(m => m.MatchId == matchId);

            if (match == null) return;

            var innings = await _context.MatchInnings
                .Where(i => i.MatchId == matchId && i.Status == InningsStatus.Completed)
                .ToListAsync();

            if (innings.Count < 2) return; // need both innings

            int teamAScore = innings
                .Where(i => i.BattingTeamId == match.TeamAId)
                .Sum(i => i.TotalRuns);

            int teamBScore = innings
                .Where(i => i.BattingTeamId == match.TeamBId)
                .Sum(i => i.TotalRuns);

            if (teamAScore > teamBScore)
                match.WinnerTeamId = match.TeamAId;
            else if (teamBScore > teamAScore)
                match.WinnerTeamId = match.TeamBId;
            else
                match.WinnerTeamId = null; // Tie

            match.Status = MatchStatus.Completed;

            await _context.SaveChangesAsync();
        }

    }
}
