using System.Diagnostics;
using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CricbuzzAppV2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Summary stats
            ViewBag.TotalPlayers = _context.Players.Count();
            ViewBag.TotalTeams = _context.Teams.Count();
            ViewBag.TotalMatches = _context.Matches.Count();
            ViewBag.TotalScorecards = _context.Scorecards.Count();

            // Highlights
            var topBatsman = _context.PlayerStats
                .Include(ps => ps.Player)
                .OrderByDescending(ps => ps.Runs)
                .FirstOrDefault();

            var topBowler = _context.PlayerStats
                .Include(ps => ps.Player)
                .OrderByDescending(ps => ps.Wickets)
                .FirstOrDefault();

            var topTeam = _context.Teams.FirstOrDefault();

            ViewBag.TopBatsman = topBatsman;
            ViewBag.TopBowler = topBowler;
            ViewBag.TopTeam = topTeam;

            // Recent Matches
            var recentMatches = _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .OrderByDescending(m => m.Date)
                .Take(5)
                .ToList();

            ViewBag.RecentMatches = recentMatches;

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
