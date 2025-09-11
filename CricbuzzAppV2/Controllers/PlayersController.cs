using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CricbuzzAppV2.Controllers
{
    public class PlayersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlayersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Players
        public async Task<IActionResult> Index()
        {
            var players = await _context.Players
                .Include(p => p.Team)
                .OrderBy(p => p.FullName)
                .ToListAsync();
            return View(players);
        }

        // GET: Players/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
                return BadRequest();

            var player = await _context.Players
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.PlayerId == id);

            if (player == null)
                return NotFound();

            return View(player);
        }

        // GET: Players/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Teams"] = await _context.Teams.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Player player)
        {
            if (ModelState.IsValid)
            {
                _context.Players.Add(player);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "✅ Player added successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Teams"] = await _context.Teams.ToListAsync();
            return View(player);
        }


        // GET: Players/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return BadRequest();

            var player = await _context.Players.FindAsync(id);
            if (player == null)
                return NotFound();

            ViewData["Teams"] = await _context.Teams.ToListAsync();
            return View(player);
        }

        // POST: Players/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Player player)
        {
            if (id != player.PlayerId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Entry(player).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Players.Any(p => p.PlayerId == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewData["Teams"] = await _context.Teams.ToListAsync();
            return View(player);
        }

        // GET: Players/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var player = await _context.Players
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.PlayerId == id);

            if (player == null)
                return NotFound();

            return View(player);
        }

        // POST: Players/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player != null)
            {
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
