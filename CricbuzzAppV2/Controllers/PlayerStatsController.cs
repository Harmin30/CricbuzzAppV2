using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CricbuzzAppV2.Helpers;


namespace CricbuzzAppV2.Controllers
{
    public class PlayerStatsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlayerStatsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PlayerStats
        public async Task<IActionResult> Index()
        {
            var stats = await _context.PlayerStats
                .Include(p => p.Player)
                .OrderBy(p => p.Player.FullName)
                .ToListAsync();
            return View(stats);
        }
        // GET: PlayerStats/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound(); // No ID provided

            var playerStats = await _context.PlayerStats
                .Include(p => p.Player)   // Include Player for display
                .FirstOrDefaultAsync(p => p.PlayerStatsId == id);

            if (playerStats == null)
                return NotFound(); // No record found for this ID

            return View(playerStats); // Pass to Details view
        }

        // GET: PlayerStats/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Players"] = new SelectList(await _context.Players.ToListAsync(), "PlayerId", "FullName");
            ViewData["Formats"] = new SelectList(Enum.GetValues(typeof(PlayerStats.CricketFormat)));
            return View();
        }

        // POST: PlayerStats/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlayerStats stats)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Model error: {error.ErrorMessage}");
            }
            if (ModelState.IsValid)
            {
                _context.PlayerStats.Add(stats);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Player stats added successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Players"] = new SelectList(await _context.Players.ToListAsync(), "PlayerId", "FullName", stats.PlayerId);
            ViewData["Formats"] = new SelectList(Enum.GetValues(typeof(PlayerStats.CricketFormat)), stats.Format);
            return View(stats);
        }

        // GET: PlayerStats/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var stats = await _context.PlayerStats.FindAsync(id);
            if (stats == null) return NotFound();

            ViewData["Players"] = new SelectList(await _context.Players.ToListAsync(), "PlayerId", "FullName", stats.PlayerId);
            ViewData["Formats"] = new SelectList(Enum.GetValues(typeof(PlayerStats.CricketFormat)), stats.Format);
            return View(stats);
        }

        // POST: PlayerStats/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PlayerStats stats)
        {
            if (id != stats.PlayerStatsId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stats);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Player stats updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PlayerStats.Any(e => e.PlayerStatsId == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewData["Players"] = new SelectList(await _context.Players.ToListAsync(), "PlayerId", "FullName", stats.PlayerId);
            ViewData["Formats"] = new SelectList(Enum.GetValues(typeof(PlayerStats.CricketFormat)), stats.Format);
            return View(stats);
        }
        // GET: PlayerStats/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var stats = await _context.PlayerStats
                .Include(p => p.Player)
                .FirstOrDefaultAsync(p => p.PlayerStatsId == id);

            if (stats == null) return NotFound();

            return View(stats);
        }

        // POST: PlayerStats/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stats = await _context.PlayerStats
                .Include(p => p.Player)
                .FirstOrDefaultAsync(p => p.PlayerStatsId == id);

            if (stats != null)
            {
                _context.PlayerStats.Remove(stats);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"🗑 Deleted stats for player: {stats.Player?.FullName} ({stats.Format})";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: PlayerStats/DeleteSelected
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["ErrorMessages"] = new List<string> { "No player stats selected for deletion." };
                return RedirectToAction(nameof(Index));
            }

            var statsList = await _context.PlayerStats
                .Include(p => p.Player)
                .Where(p => selectedIds.Contains(p.PlayerStatsId))
                .ToListAsync();

            var deletedPlayers = new List<string>();

            foreach (var stat in statsList)
            {
                deletedPlayers.Add($"{stat.Player?.FullName} ({stat.Format})");
                _context.PlayerStats.Remove(stat);
            }

            await _context.SaveChangesAsync();

            if (deletedPlayers.Any())
                TempData["SuccessMessage"] = $"🗑 Deleted stats for: {string.Join(", ", deletedPlayers)}";

            return RedirectToAction(nameof(Index));
        }

    }



}

