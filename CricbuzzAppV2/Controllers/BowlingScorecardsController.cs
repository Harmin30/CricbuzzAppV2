using CricbuzzAppV2.Data;
using CricbuzzAppV2.Helpers;
using CricbuzzAppV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CricbuzzAppV2.Controllers
{
    public class BowlingScorecardsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BowlingScorecardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /BowlingScorecards/Create?inningsId=5
        public async Task<IActionResult> Create(int inningsId)
        {
            var innings = await _context.MatchInnings
                .Include(mi => mi.BowlingTeam)
                .FirstOrDefaultAsync(mi => mi.MatchInningsId == inningsId);

            if (innings == null || innings.Status != InningsStatus.InProgress)
                return NotFound();

            var players = await _context.Players
                .Where(p => p.TeamId == innings.BowlingTeamId)
                .ToListAsync();

            ViewBag.Innings = innings;
            ViewBag.Players = players;

            return View(new BowlingScorecard
            {
                MatchInningsId = inningsId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BowlingScorecard scorecard)
        {
            var innings = await _context.MatchInnings
                .Include(mi => mi.BowlingTeam)
                .FirstOrDefaultAsync(mi => mi.MatchInningsId == scorecard.MatchInningsId);

            if (innings == null)
                return NotFound();

            // Duplicate bowler check
            bool exists = await _context.BowlingScorecards.AnyAsync(b =>
                b.MatchInningsId == scorecard.MatchInningsId &&
                b.PlayerId == scorecard.PlayerId);

            if (exists)
            {
                ModelState.AddModelError("PlayerId",
                    "This player already has a bowling entry.");
            }

            // 🧠 Logical bowling validations
            if (scorecard.Maidens > Math.Floor(scorecard.Overs))
            {
                ModelState.AddModelError("Maidens",
                    "Maidens cannot be greater than overs bowled.");
            }

            if (scorecard.Wickets > 10)
            {
                ModelState.AddModelError("Wickets",
                    "A bowler cannot take more than 10 wickets.");
            }

            // Overs format check (e.g. 4.6 max)
            var ballsPart = (scorecard.Overs * 10) % 10;
            if (ballsPart > 6)
            {
                ModelState.AddModelError("Overs",
                    "Invalid overs format. Max balls per over is 6 (e.g. 4.6).");
            }

            // ❗ Final validation check
            if (!ModelState.IsValid)
            {
                ViewBag.Innings = innings;
                ViewBag.Players = await _context.Players
                    .Where(p => p.TeamId == innings.BowlingTeamId)
                    .ToListAsync();

                return View(scorecard);
            }

            // ✅ Save
            _context.BowlingScorecards.Add(scorecard);
            await _context.SaveChangesAsync();

            await RecalculateInningsTotals(scorecard.MatchInningsId);

            int matchId = await _context.MatchInnings
                .Where(mi => mi.MatchInningsId == scorecard.MatchInningsId)
                .Select(mi => mi.MatchId)
                .FirstAsync();

            TempData["Success"] = "Bowling entry added successfully.";



            return RedirectToAction("Details", "Matches", new { id = matchId });
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

        // GET: BowlingScorecards/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var scorecard = await _context.BowlingScorecards
                .Include(b => b.MatchInnings)
                    .ThenInclude(i => i.BowlingTeam)
                .FirstOrDefaultAsync(b => b.BowlingScorecardId == id);

            if (scorecard == null)
                return NotFound();

            // ❌ lock edit if innings ended
            if (scorecard.MatchInnings.Status != InningsStatus.InProgress)
                return BadRequest("Innings already ended.");

            ViewBag.Innings = scorecard.MatchInnings;
            ViewBag.Players = await _context.Players
                .Where(p => p.TeamId == scorecard.MatchInnings.BowlingTeamId)
                .ToListAsync();

            return View(scorecard);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BowlingScorecard scorecard)
        {
            if (!ModelState.IsValid)
            {
                var innings = await _context.MatchInnings
                    .Include(i => i.BowlingTeam)
                    .FirstAsync(i => i.MatchInningsId == scorecard.MatchInningsId);

                ViewBag.Innings = innings;
                ViewBag.Players = await _context.Players
                    .Where(p => p.TeamId == innings.BowlingTeamId)
                    .ToListAsync();

                return View(scorecard);
            }

            // ✅ Load existing DB row
            var existing = await _context.BowlingScorecards
                .FirstOrDefaultAsync(b => b.BowlingScorecardId == scorecard.BowlingScorecardId);

            if (existing == null)
                return NotFound();

            // ✅ Update only editable fields
            existing.Overs = scorecard.Overs;
            existing.Maidens = scorecard.Maidens;
            existing.RunsConceded = scorecard.RunsConceded;
            existing.Wickets = scorecard.Wickets;

            await _context.SaveChangesAsync();

            AppHelper.SetSuccess(this, "✏ Bowling entry updated successfully.");

            // ✅ SAFELY get MatchId
            var matchId = await _context.MatchInnings
                .Where(i => i.MatchInningsId == existing.MatchInningsId)
                .Select(i => i.MatchId)
                .FirstAsync();

            return RedirectToAction("Details", "Matches", new { id = matchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var scorecard = await _context.BowlingScorecards
                .Include(b => b.MatchInnings)
                .FirstOrDefaultAsync(b => b.BowlingScorecardId == id);

            if (scorecard == null)
                return NotFound();

            int matchId = scorecard.MatchInnings.MatchId;

            _context.BowlingScorecards.Remove(scorecard);
            await _context.SaveChangesAsync();

            AppHelper.SetSuccess(this, "🗑 Bowling entry deleted.");

            return RedirectToAction("Details", "Matches", new { id = matchId });
        }



    }


}
