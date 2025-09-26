using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using CricbuzzAppV2.Helpers;


namespace CricbuzzAppV2.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeamsController(ApplicationDbContext context)
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
                return NotFound();

            // ✅ Check if team has matches before deleting
            bool hasMatches = await _context.Matches.AnyAsync(m =>
                m.TeamAId == id || m.TeamBId == id || m.WinnerTeamId == id);

            if (hasMatches)
            {
                AppHelper.SetError(this, "🚫 Cannot delete this team. Matches are already scheduled or completed with this team.");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Teams.Remove(team);
                await _context.SaveChangesAsync();
                AppHelper.SetSuccess(this, "✅ Team deleted successfully!");
            }
            catch (Exception ex)
            {
                // Just in case something unexpected still happens
                AppHelper.SetError(this, $"⚠️ Error while deleting team: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }


    }
}
