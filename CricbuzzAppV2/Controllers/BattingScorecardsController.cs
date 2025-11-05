using CricbuzzAppV2.Data;
using CricbuzzAppV2.Helpers;
using CricbuzzAppV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CricbuzzAppV2.Controllers
{
    public class BattingScorecardsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BattingScorecardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /BattingScorecards/Create?inningsId=5
        public async Task<IActionResult> Create(int inningsId)
        {
            var innings = await _context.MatchInnings
                .Include(mi => mi.BattingTeam)
                .FirstOrDefaultAsync(mi => mi.MatchInningsId == inningsId);

            if (innings == null || innings.Status != InningsStatus.InProgress)
                return NotFound();

            var players = await _context.Players
                .Where(p => p.TeamId == innings.BattingTeamId)
                .ToListAsync();

            ViewBag.Innings = innings;
            ViewBag.Players = players;

            return View(new BattingInningsScorecard
            {
                MatchInningsId = inningsId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BattingInningsScorecard scorecard)
        {
            var innings = await _context.MatchInnings
                .Include(mi => mi.BattingTeam)
                .FirstOrDefaultAsync(mi => mi.MatchInningsId == scorecard.MatchInningsId);

            if (innings == null)
                return NotFound();

            // Duplicate batsman check
            bool exists = await _context.BattingScorecards.AnyAsync(b =>
                b.MatchInningsId == scorecard.MatchInningsId &&
                b.PlayerId == scorecard.PlayerId);

            if (exists)
            {
                ModelState.AddModelError("PlayerId", "This player already has a batting entry.");
            }

            // 🧠 Logical cricket validations (MUST be before ModelState check)
            if (scorecard.Fours * 4 + scorecard.Sixes * 6 > scorecard.Runs)
            {
                ModelState.AddModelError("Runs",
                    "Runs cannot be less than runs scored from fours and sixes.");
            }

            if (scorecard.BallsFaced == 0 && scorecard.Runs > 0)
            {
                ModelState.AddModelError("BallsFaced",
                    "Balls faced must be greater than 0 if runs are scored.");
            }

            // ❗ NOW check ModelState
            if (!ModelState.IsValid)
            {
                ViewBag.Innings = innings;
                ViewBag.Players = await _context.Players
                    .Where(p => p.TeamId == innings.BattingTeamId)
                    .ToListAsync();

                return View(scorecard);
            }

            // ✅ Save only when everything is valid
            _context.BattingScorecards.Add(scorecard);
            await _context.SaveChangesAsync();

            await RecalculateInningsTotals(scorecard.MatchInningsId);

            int matchId = await _context.MatchInnings
                .Where(mi => mi.MatchInningsId == scorecard.MatchInningsId)
                .Select(mi => mi.MatchId)
                .FirstAsync();

            TempData["Success"] = "Batting entry added successfully.";

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


        // GET: BattingScorecards/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var scorecard = await _context.BattingScorecards
                .Include(b => b.MatchInnings)
                    .ThenInclude(i => i.BattingTeam)
                .FirstOrDefaultAsync(b => b.BattingScorecardId == id);

            if (scorecard == null)
                return NotFound();

            // 🔒 Prevent editing if innings ended
            if (scorecard.MatchInnings.Status != InningsStatus.InProgress)
                return BadRequest("Innings has already ended.");

            ViewBag.Innings = scorecard.MatchInnings;
            ViewBag.Players = await _context.Players
                .Where(p => p.TeamId == scorecard.MatchInnings.BattingTeamId)
                .ToListAsync();

            return View(scorecard);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BattingInningsScorecard scorecard)
        {
            if (!ModelState.IsValid)
            {
                var innings = await _context.MatchInnings
                    .Include(i => i.BattingTeam)
                    .FirstAsync(i => i.MatchInningsId == scorecard.MatchInningsId);

                ViewBag.Innings = innings;
                ViewBag.Players = await _context.Players
                    .Where(p => p.TeamId == innings.BattingTeamId)
                    .ToListAsync();

                return View(scorecard);
            }

            // ✅ Load existing row
            var existing = await _context.BattingScorecards
                .FirstOrDefaultAsync(b => b.BattingScorecardId == scorecard.BattingScorecardId);

            if (existing == null)
                return NotFound();

            // ✅ Update fields
            existing.Runs = scorecard.Runs;
            existing.BallsFaced = scorecard.BallsFaced;
            existing.Fours = scorecard.Fours;
            existing.Sixes = scorecard.Sixes;
            existing.BattingPosition = scorecard.BattingPosition;
            existing.HowOut = scorecard.HowOut;

            await _context.SaveChangesAsync();

            // ✅ GET MATCH ID CORRECTLY
            int matchId = await _context.MatchInnings
                .Where(i => i.MatchInningsId == existing.MatchInningsId)
                .Select(i => i.MatchId)
                .FirstAsync();

            TempData["Success"] = "✏ Batting entry updated successfully.";

            return RedirectToAction("Details", "Matches", new { id = matchId });
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var scorecard = await _context.BattingScorecards
                .Include(b => b.MatchInnings)
                .FirstOrDefaultAsync(b => b.BattingScorecardId == id);

            if (scorecard == null)
                return NotFound();

            // ❌ Block delete if innings ended
            if (scorecard.MatchInnings.Status != InningsStatus.InProgress)
                return BadRequest("Cannot delete after innings is ended.");

            int matchId = scorecard.MatchInnings.MatchId;

            _context.BattingScorecards.Remove(scorecard);
            await _context.SaveChangesAsync();

            AppHelper.SetSuccess(this, "🗑 Batting entry deleted successfully.");

            return RedirectToAction("Details", "Matches", new { id = matchId });
        }



    }
}
