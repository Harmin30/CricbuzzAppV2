using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;

namespace CricbuzzAppV2.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
  {
            _context = context;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
      // Get counts for dashboard
         ViewBag.TotalPlayers = await _context.Players.CountAsync();
         ViewBag.TotalTeams = await _context.Teams.CountAsync();
            ViewBag.TotalMatches = await _context.Matches.CountAsync();
  ViewBag.TotalUsers = await _context.Users.CountAsync();

 // Get recent activities
        ViewBag.RecentActivities = await _context.Audits
      .OrderByDescending(a => a.Timestamp)
     .Take(10)
       .ToListAsync();

            // Get upcoming matches
   ViewBag.UpcomingMatches = await _context.Matches
        .Include(m => m.TeamA)
       .Include(m => m.TeamB)
         .Where(m => m.Date >= DateTime.Today)
          .OrderBy(m => m.Date)
     .Take(5)
         .ToListAsync();

        return View();
 }
    }
}