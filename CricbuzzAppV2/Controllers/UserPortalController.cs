using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CricbuzzAppV2.Controllers
{
    public class UserPortalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserPortalController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🏠 Home Page
        public IActionResult Index()
        {
            // Optionally fetch top players or recent matches here
            return View();
        }

        // 👥 Players List
        public IActionResult Players()
        {
            var players = _context.Players
                .Include(p => p.Team)
                .OrderBy(p => p.FullName)
                .ToList();

            return View(players); // Views/UserPortal/Players.cshtml
        }

        // 📋 Player Details
        public IActionResult PlayerDetails(int id)
        {
            var player = _context.Players
                .Include(p => p.Team)
                .Include(p => p.PlayerStats)
                .Include(p => p.PlayerPersonalInfo)
                .FirstOrDefault(p => p.PlayerId == id);

            if (player == null)
                return NotFound();

            return View(player); // Views/UserPortal/PlayerDetails.cshtml
        }

        // 📅 Matches List - /UserPortal/Matches
        public IActionResult Matches()
        {
            var matches = _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.Scorecards)
                .OrderByDescending(m => m.Date)
                .ToList();

            return View(matches); // Views/UserPortal/Matches.cshtml
        }

        // 🏏 Match Details - /UserPortal/MatchDetails/5
        public IActionResult MatchDetails(int id)
        {
            var match = _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.WinnerTeam)
                .Include(m => m.Scorecards)
                    .ThenInclude(s => s.Player)
                .FirstOrDefault(m => m.MatchId == id);

            if (match == null)
                return NotFound();

            return View(match); // Views/UserPortal/MatchDetails.cshtml
        }


        // 🧩 Player CRUD - Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Player model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.Players.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Player created successfully!";
            return RedirectToAction(nameof(Players));
        }
        // 📊 Player Stats - /UserPortal/Stats
        public IActionResult Stats(string? format)
        {
            var statsQuery = _context.PlayerStats
                .Include(ps => ps.Player)
                .AsQueryable();

            // Optional filter by format
            if (!string.IsNullOrEmpty(format))
            {
                if (Enum.TryParse<PlayerStats.CricketFormat>(format, out var selectedFormat))
                {
                    statsQuery = statsQuery.Where(ps => ps.Format == selectedFormat);
                }
            }

            var stats = statsQuery
                .Include(ps => ps.Player)
                .ToList();

            return View(stats); // Views/UserPortal/Stats.cshtml
        }

        // 🏆 Teams List
        public IActionResult Teams()
        {
            var teams = _context.Teams
                .Include(t => t.Players)
                .OrderBy(t => t.TeamName)
                .ToList();

            return View(teams); // Views/UserPortal/Teams.cshtml
        }

        // 📋 Team Details
        public IActionResult TeamDetails(int id)
        {
            var team = _context.Teams
                .Include(t => t.Players)
                .FirstOrDefault(t => t.TeamId == id);

            if (team == null)
                return NotFound();

            return View(team); // Views/UserPortal/TeamDetails.cshtml
        }

        // Top Batsmen
        public IActionResult TopBatsmen(string? format)
        {
            var batsmen = _context.PlayerStats
                .Include(ps => ps.Player)
                .ThenInclude(p => p.Team)
                .Where(ps => ps.Runs > 0) // only players who batted
                .AsQueryable();

            if (!string.IsNullOrEmpty(format) && Enum.TryParse<PlayerStats.CricketFormat>(format, out var selectedFormat))
            {
                batsmen = batsmen.Where(ps => ps.Format == selectedFormat);
            }

            var result = batsmen.OrderByDescending(ps => ps.Runs).ToList();
            ViewBag.SelectedFormat = format;
            return View(result);
        }

        // Top Bowlers
        public IActionResult TopBowlers(string? format)
        {
            var bowlers = _context.PlayerStats
                .Include(ps => ps.Player)
                .ThenInclude(p => p.Team)
                .Where(ps => ps.Wickets > 0) // only players who bowled
                .AsQueryable();

            if (!string.IsNullOrEmpty(format) && Enum.TryParse<PlayerStats.CricketFormat>(format, out var selectedFormat))
            {
                bowlers = bowlers.Where(ps => ps.Format == selectedFormat);
            }

            var result = bowlers.OrderByDescending(ps => ps.Wickets).ToList();
            ViewBag.SelectedFormat = format;
            return View(result);
        }

        // Top Teams
        public IActionResult TopTeams(string? format)
        {
            // Teams ranked by total runs or wins (example: total runs in selected format)
            var teams = _context.Teams
                .Include(t => t.Players)
                    .ThenInclude(p => p.PlayerStats)
                .ToList();

            if (!string.IsNullOrEmpty(format) && Enum.TryParse<PlayerStats.CricketFormat>(format, out var selectedFormat))
            {
                teams = teams
                    .OrderByDescending(t => t.Players
                        .SelectMany(p => p.PlayerStats)
                        .Where(ps => ps.Format == selectedFormat)
                        .Sum(ps => ps.Runs)) // total team runs in format
                    .ToList();
            }
            else
            {
                teams = teams
                    .OrderByDescending(t => t.Players.Sum(p => p.PlayerStats.Sum(ps => ps.Runs)))
                    .ToList();
            }

            ViewBag.SelectedFormat = format;
            return View(teams);
        }


        // ✏️ Player CRUD - Edit
        public IActionResult Edit(int id)
        {
            var player = _context.Players.Find(id);
            if (player == null)
                return NotFound();

            return View(player);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Player model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingPlayer = _context.Players.Find(id);
            if (existingPlayer == null)
                return NotFound();

            existingPlayer.FullName = model.FullName;
            existingPlayer.Role = model.Role;
            existingPlayer.TeamId = model.TeamId;
            existingPlayer.ImageUrl = model.ImageUrl;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Player updated successfully!";
            return RedirectToAction(nameof(Players));
        }

        // 🗑 Player CRUD - Delete
        public IActionResult Delete(int id)
        {
            var player = _context.Players.Find(id);
            if (player == null)
                return NotFound();

            return View(player);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var player = _context.Players.Find(id);
            if (player != null)
            {
                _context.Players.Remove(player);
                _context.SaveChanges();
            }

            TempData["SuccessMessage"] = "Player deleted successfully!";
            return RedirectToAction(nameof(Players));
        }
    }
}
