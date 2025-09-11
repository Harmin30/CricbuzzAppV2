using System.Diagnostics;
using CricbuzzAppV2.Data;   // ✅ add this (namespace for your DbContext)
using CricbuzzAppV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // for Include

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
            // Top Batsman by Runs
            var topBatsman = _context.PlayerStats
                .Include(ps => ps.Player)
                .OrderByDescending(ps => ps.Runs)
                .FirstOrDefault();

            // Top Bowler by Wickets
            var topBowler = _context.PlayerStats
                .Include(ps => ps.Player)
                .OrderByDescending(ps => ps.Wickets)
                .FirstOrDefault();

            // Top Team (just pick first team)
            var topTeam = _context.Teams.FirstOrDefault();

            ViewBag.TopBatsman = topBatsman;
            ViewBag.TopBowler = topBowler;
            ViewBag.TopTeam = topTeam;

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
