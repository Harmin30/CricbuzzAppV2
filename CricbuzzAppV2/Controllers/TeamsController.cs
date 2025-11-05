using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using CricbuzzAppV2.Helpers;
using Microsoft.AspNetCore.Hosting;

namespace CricbuzzAppV2.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TeamsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
        }

        // GET: Teams
        public async Task<IActionResult> Index()
        {
            var teams = await _context.Teams.ToListAsync();
            return View(teams);
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.TeamId == id);
            if (team == null)
                return NotFound();

            return View(team);
        }

        // GET: Teams/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Team team)
        {
            if (ModelState.IsValid)
            {
                _context.Teams.Add(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        // GET: Teams/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
                return NotFound();

            return View(team);
        }

        // POST: Teams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Team team)
        {
            if (id != team.TeamId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Entry(team).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Teams.Any(t => t.TeamId == id))
                        return NotFound();
                    else
                        throw;
                }
            }
            return View(team);
        }

        // GET: Teams/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.TeamId == id);
            if (team == null)
                return NotFound();

            return View(team);
        }

        //
        // ✅ Bulk Delete Action
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                AppHelper.SetError(this, "No teams selected for deletion.");
                return RedirectToAction(nameof(Index));
            }

            var teams = await _context.Teams
  .Where(t => selectedIds.Contains(t.TeamId))
          .ToListAsync();

            var deletedTeams = new List<string>();
            var skippedTeams = new List<string>();

            foreach (var team in teams)
            {
                bool hasMatches = await _context.Matches
                       .AnyAsync(m => m.TeamAId == team.TeamId || m.TeamBId == team.TeamId);

                if (hasMatches)
                {
                    skippedTeams.Add($"❌ Team '{team.TeamName}' cannot be deleted because it has matches scheduled or completed.");
                    continue;
                }

                _context.Teams.Remove(team);
                deletedTeams.Add(team.TeamName);

                //// Log each deletion
                //await AuditHelper.LogDelete(_context, HttpContext, "Team", team.TeamId.ToString(),
                //$"Deleted team: {team.TeamName} from {team.Country}");
            }

            await _context.SaveChangesAsync();

            // Success messages
            if (deletedTeams.Any())
                AppHelper.SetSuccess(this, $"🗑 Deleted teams: {string.Join(", ", deletedTeams)}");

            // Error messages for skipped teams
            if (skippedTeams.Any())
                AppHelper.SetError(this, string.Join("<br/>", skippedTeams));

            return RedirectToAction(nameof(Index));
        }
    }
}
