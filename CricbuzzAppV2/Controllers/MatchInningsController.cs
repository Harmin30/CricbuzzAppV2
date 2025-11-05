using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CricbuzzAppV2.Controllers
{
    public class MatchInningsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MatchInningsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /MatchInnings/Create?matchId=5
        public async Task<IActionResult> Create(int matchId)
        {
            var match = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .FirstOrDefaultAsync(m => m.MatchId == matchId);

            if (match == null)
                return NotFound();

            // 🔥 NEW: Check if an innings is already in progress
            var currentInnings = await _context.MatchInnings
     .Include(mi => mi.BattingTeam)
     .FirstOrDefaultAsync(mi =>
         mi.MatchId == matchId &&
         mi.Status == InningsStatus.InProgress &&
         mi.EndTime == null);


            ViewBag.Match = match;
            ViewBag.Teams = new[] { match.TeamA, match.TeamB };
            ViewBag.CurrentInnings = currentInnings;

            return View();
        }

        // POST: Start Innings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MatchInnings innings)
        {

            var openInnings = await _context.MatchInnings
    .Where(i => i.MatchId == innings.MatchId &&
                i.Status == InningsStatus.InProgress)
    .ToListAsync();

            foreach (var inn in openInnings)
            {
                inn.Status = InningsStatus.Completed;
                inn.EndTime = DateTime.Now;
            }
            await _context.SaveChangesAsync(); // 🔥 REQUIRED


            var match = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .FirstOrDefaultAsync(m => m.MatchId == innings.MatchId);

            if (match == null)
                return NotFound();


            // ❌ Same team cannot bat and bowl
            if (innings.BattingTeamId == innings.BowlingTeamId)
            {
                ModelState.AddModelError("", "Batting and bowling team cannot be the same.");
            }

            // ❌ Check max innings per team
            int inningsPlayedByTeam = await _context.MatchInnings.CountAsync(mi =>
                mi.MatchId == innings.MatchId &&
                mi.BattingTeamId == innings.BattingTeamId);

            // 🔐 SAFETY: default MaxInningsPerTeam if not set
            int maxInnings = match.MaxInningsPerTeam > 0
                ? match.MaxInningsPerTeam
                : (match.MatchType == "Test" ? 2 : 1);

            if (inningsPlayedByTeam >= maxInnings)
            {
                ModelState.AddModelError("",
                    "This team has already played all its allowed innings.");
            }


            if (!ModelState.IsValid)
            {
                ViewBag.Match = match;
                ViewBag.Teams = new[] { match.TeamA, match.TeamB };
                ViewBag.CurrentInnings = null;
                return View(innings);
            }

            // 🧮 Auto innings number
            int totalInnings = await _context.MatchInnings
                .CountAsync(mi => mi.MatchId == innings.MatchId);

            innings.InningsNumber = totalInnings + 1;
            innings.Status = InningsStatus.InProgress;

            _context.MatchInnings.Add(innings);

            match.Status = MatchStatus.Live;

            await _context.SaveChangesAsync();


            return RedirectToAction("Details", "Matches", new { id = innings.MatchId });
        }

        // POST: End Innings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndInnings(int id)
        {
            var innings = await _context.MatchInnings
                .Include(i => i.Match)
                .Include(i => i.BattingScorecards)
                .Include(i => i.BowlingScorecards)
                .FirstOrDefaultAsync(i => i.MatchInningsId == id);

            if (innings == null)
                return NotFound();

            // 1️⃣ Finalize innings stats
            innings.TotalRuns = innings.BattingScorecards.Sum(b => b.Runs);
            innings.WicketsLost = innings.BattingScorecards
                .Count(b => !string.IsNullOrWhiteSpace(b.HowOut)
                         && !b.HowOut.Equals("Not Out", StringComparison.OrdinalIgnoreCase));
            innings.OversBowled = innings.BowlingScorecards.Sum(b => b.Overs);

            // 2️⃣ End this innings
            innings.Status = InningsStatus.Completed;
            innings.EndTime = DateTime.Now;

            // 3️⃣ SAFETY: ensure NO other innings stays live
            var strayInnings = await _context.MatchInnings
                .Where(i => i.MatchId == innings.MatchId &&
                            i.Status == InningsStatus.InProgress &&
                            i.MatchInningsId != innings.MatchInningsId)
                .ToListAsync();

            foreach (var inn in strayInnings)
            {
                inn.Status = InningsStatus.Completed;
                inn.EndTime = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            // 🔒 CHECK IF MATCH SHOULD STILL ALLOW INNINGS
            var match = await _context.Matches.FindAsync(innings.MatchId);

            int maxInnings = match.MaxInningsPerTeam > 0
                ? match.MaxInningsPerTeam * 2
                : (match.MatchType == "Test" ? 4 : 2);

            int completedCount = await _context.MatchInnings
                .CountAsync(i => i.MatchId == innings.MatchId &&
                                 i.Status == InningsStatus.Completed);

            if (completedCount >= maxInnings)
            {
                await CalculateWinnerAsync(innings.MatchId);
            }


            // 4️⃣ 🔥 VERY IMPORTANT — calculate result & match completion
            await CalculateWinnerAsync(innings.MatchId);

            TempData["Success"] = "Innings ended successfully.";

            return RedirectToAction("Details", "Matches", new { id = innings.MatchId });
        }



        private async Task RecalculateInningsTotals(int inningsId)
        {
            var innings = await _context.MatchInnings
                .Include(mi => mi.BattingScorecards)
                .Include(mi => mi.BowlingScorecards)
                .FirstOrDefaultAsync(mi => mi.MatchInningsId == inningsId);

            if (innings == null) return;

            // 🏏 Batting totals
            innings.TotalRuns = innings.BattingScorecards.Sum(b => b.Runs);

            innings.WicketsLost = innings.BattingScorecards
                .Count(b => !string.IsNullOrWhiteSpace(b.HowOut)
                         && !b.HowOut.Equals("Not Out", StringComparison.OrdinalIgnoreCase));

            // 🎯 Bowling totals
            innings.OversBowled = innings.BowlingScorecards.Sum(b => b.Overs);

            await _context.SaveChangesAsync();
        }
        // 🔥 ADD THIS METHOD HERE (INSIDE THE CLASS)
        private async Task CalculateWinnerAsync(int matchId)
        {
            var match = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .FirstOrDefaultAsync(m => m.MatchId == matchId);

            if (match == null)
                return;

            var completedInnings = await _context.MatchInnings
                .Where(i => i.MatchId == matchId && i.Status == InningsStatus.Completed)
                .OrderBy(i => i.InningsNumber)
                .ToListAsync();

            // Limited overs → 2 innings
            if (completedInnings.Count < 2)
                return;

            var firstInnings = completedInnings[0];
            var secondInnings = completedInnings[1];

            // 🏏 Runs comparison
            if (firstInnings.TotalRuns > secondInnings.TotalRuns)
            {
                match.WinnerTeamId = firstInnings.BattingTeamId;
                match.ResultDescription =
                    $"won by {firstInnings.TotalRuns - secondInnings.TotalRuns} runs";
            }
            else if (secondInnings.TotalRuns > firstInnings.TotalRuns)
            {
                match.WinnerTeamId = secondInnings.BattingTeamId;

                int wicketsRemaining = 10 - secondInnings.WicketsLost;
                match.ResultDescription =
                    $"won by {wicketsRemaining} wickets";
            }
            else
            {
                match.WinnerTeamId = null;
                match.ResultDescription = "Match tied";
            }

            match.Status = MatchStatus.Completed;
            await _context.SaveChangesAsync();
        }

    }   // ✅ ONLY ONE closing brace for controller

}


