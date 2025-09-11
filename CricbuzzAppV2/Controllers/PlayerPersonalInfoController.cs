using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CricbuzzAppV2.Controllers
{
    public class PlayerPersonalInfoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlayerPersonalInfoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PlayerPersonalInfo
        public async Task<IActionResult> Index()
        {
            var infos = await _context.PlayerPersonalInfos
                .Include(p => p.Player)
                .ThenInclude(p => p.Team)
                .OrderBy(p => p.Player.FullName)
                .ToListAsync();

            return View(infos);
        }

        // GET: PlayerPersonalInfo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var info = await _context.PlayerPersonalInfos
                .Include(p => p.Player)
                .ThenInclude(p => p.Team)
                .FirstOrDefaultAsync(m => m.PlayerPersonalInfoId == id);

            if (info == null) return NotFound();

            return View(info);
        }

        // GET: PlayerPersonalInfo/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Players"] = new SelectList(await _context.Players.ToListAsync(), "PlayerId", "FullName");
            return View();
        }

        // POST: PlayerPersonalInfo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlayerPersonalInfo info)
        {
            if (ModelState.IsValid)
            {
                _context.Add(info);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Player personal info added successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Players"] = new SelectList(await _context.Players.ToListAsync(), "PlayerId", "FullName", info.PlayerId);
            return View(info);
        }

        // GET: PlayerPersonalInfo/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var info = await _context.PlayerPersonalInfos.FindAsync(id);
            if (info == null) return NotFound();

            ViewData["Players"] = new SelectList(await _context.Players.ToListAsync(), "PlayerId", "FullName", info.PlayerId);
            return View(info);
        }

        // POST: PlayerPersonalInfo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PlayerPersonalInfo info)
        {
            if (id != info.PlayerPersonalInfoId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(info);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Player personal info updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PlayerPersonalInfos.Any(e => e.PlayerPersonalInfoId == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewData["Players"] = new SelectList(await _context.Players.ToListAsync(), "PlayerId", "FullName", info.PlayerId);
            return View(info);
        }

        // GET: PlayerPersonalInfo/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var info = await _context.PlayerPersonalInfos
                .Include(p => p.Player)
                .ThenInclude(p => p.Team)
                .FirstOrDefaultAsync(m => m.PlayerPersonalInfoId == id);

            if (info == null) return NotFound();

            return View(info);
        }

        // POST: PlayerPersonalInfo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var info = await _context.PlayerPersonalInfos.FindAsync(id);
            if (info != null)
            {
                _context.PlayerPersonalInfos.Remove(info);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Player personal info deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
