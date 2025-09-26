using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;

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

            // Highlights: Top Batsman
            var topBatsman = _context.PlayerStats
                .Include(ps => ps.Player)
                .OrderByDescending(ps => ps.Runs)
                .FirstOrDefault();

            // Top Bowler
            var topBowler = _context.PlayerStats
                .Include(ps => ps.Player)
                .OrderByDescending(ps => ps.Wickets)
                .FirstOrDefault();

            // Top Team logic
            var teamWins = _context.Matches
                .Where(m => m.WinnerTeamId != null)
                .GroupBy(m => m.WinnerTeamId)
                .Select(g => new
                {
                    TeamId = g.Key,
                    TotalWins = g.Count(),
                    TestWins = g.Count(m => m.MatchType == "Test"),
                    ODIWins = g.Count(m => m.MatchType == "ODI"),
                    T20Wins = g.Count(m => m.MatchType == "T20")
                })
                .ToList();

            // Get max total wins
            var maxWins = teamWins.Max(t => t.TotalWins);
            var topTeams = teamWins.Where(t => t.TotalWins == maxWins).ToList();

            // If tie, select team with more Test wins
            var topTeamData = topTeams.OrderByDescending(t => t.TestWins).FirstOrDefault();

            // Get team details from Teams table
            Team topTeam = null;
            int topTeamTestWins = 0, topTeamODIWins = 0, topTeamT20Wins = 0;
            if (topTeamData != null)
            {
                topTeam = _context.Teams.Find(topTeamData.TeamId);
                topTeamTestWins = topTeamData.TestWins;
                topTeamODIWins = topTeamData.ODIWins;
                topTeamT20Wins = topTeamData.T20Wins;
            }

            ViewBag.TopTeam = topTeam;
            ViewBag.TopTeamTestWins = topTeamTestWins;
            ViewBag.TopTeamODIWins = topTeamODIWins;
            ViewBag.TopTeamT20Wins = topTeamT20Wins;

            // Recent Matches
            var recentMatches = _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .Include(m => m.WinnerTeam) // Make sure WinnerTeam navigation exists
                .OrderByDescending(m => m.Date)
                .Take(5)
                .ToList();

            ViewBag.RecentMatches = recentMatches;
            ViewBag.TopBatsman = topBatsman;
            ViewBag.TopBowler = topBowler;

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
