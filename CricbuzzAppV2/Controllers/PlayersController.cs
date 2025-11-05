using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using CricbuzzAppV2.ViewModels;
using CricbuzzAppV2.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CricbuzzAppV2.Controllers
{
    public class PlayersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PlayersController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ============================
        // INDEX
        // ============================
        public async Task<IActionResult> Index()
        {
            var players = await _context.Players
                .Include(p => p.Team)
                .OrderBy(p => p.FullName)
                .ToListAsync();

            return View(players);
        }

        // ============================
        // DETAILS
        // ============================
        public async Task<IActionResult> Details(int id)
        {
            var player = await _context.Players
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.PlayerId == id);

            if (player == null)
                return NotFound();

            return View(player);
        }

        // ============================
        // CREATE (GET)
        // ============================
        public async Task<IActionResult> Create()
        {
            ViewBag.Teams = await _context.Teams.ToListAsync();
            return View(new PlayerCreateViewModel());
        }

        // ============================
        // CREATE (POST)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlayerCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Teams = await _context.Teams.ToListAsync();
                return View(model);
            }

            model.FullName = StringHelper.Normalize(model.FullName);

            bool playerExists = await _context.Players.AnyAsync(p =>
                p.FullName.ToLower() == model.FullName.ToLower());

            if (playerExists)
            {
                ModelState.AddModelError("FullName", "This player already exists.");
                ViewBag.Teams = await _context.Teams.ToListAsync();
                return View(model);
            }



            string imagePath = "https://static.vecteezy.com/system/resources/previews/036/280/651/original/default-avatar-profile-icon-social-media-user-image-gray-avatar-icon-blank-profile-silhouette-illustration-vector.jpg";

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string folder = Path.Combine(_env.WebRootPath, "images/players");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                string filePath = Path.Combine(folder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);

                imagePath = "/images/players/" + fileName;
            }
            else if (!string.IsNullOrWhiteSpace(model.ImageUrlInput))
            {
                imagePath = model.ImageUrlInput;
            }

            var player = new Player
            {
                FullName = model.FullName,
                Role = model.Role,
                TeamId = model.TeamId,
                ImageUrl = imagePath
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            AppHelper.SetSuccess(this, "✅ Player added successfully.");
            return RedirectToAction(nameof(Index));
        }

        // ============================
        // EDIT (GET)
        // ============================
        public async Task<IActionResult> Edit(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null)
                return NotFound();

            var vm = new PlayerEditViewModel
            {
                PlayerId = player.PlayerId,
                FullName = player.FullName,
                Role = player.Role,
                TeamId = player.TeamId,
                ExistingImageUrl = player.ImageUrl
            };

            ViewBag.Teams = await _context.Teams.ToListAsync();
            return View(vm);
        }

        // ============================
        // EDIT (POST)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PlayerEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Teams = await _context.Teams.ToListAsync();
                return View(model);
            }
            model.FullName = StringHelper.Normalize(model.FullName);

            bool playerExists = await _context.Players.AnyAsync(p =>
    p.PlayerId != model.PlayerId &&
    p.FullName.ToLower() == model.FullName.ToLower());

            if (playerExists)
            {
                ModelState.AddModelError("FullName", "Another player with this name already exists.");
                ViewBag.Teams = await _context.Teams.ToListAsync();
                return View(model);
            }


            var player = await _context.Players.FindAsync(model.PlayerId);
            if (player == null)
                return NotFound();

            player.FullName = model.FullName;
            player.Role = model.Role;
            player.TeamId = model.TeamId;

            string imagePath = player.ImageUrl;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string folder = Path.Combine(_env.WebRootPath, "images/players");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                string filePath = Path.Combine(folder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);

                imagePath = "/images/players/" + fileName;
            }
            else if (!string.IsNullOrWhiteSpace(model.ImageUrlInput))
            {
                imagePath = model.ImageUrlInput;
            }

            player.ImageUrl = imagePath;
            await _context.SaveChangesAsync();

            AppHelper.SetSuccess(this, "✏️ Player updated successfully.");
            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null)
                return NotFound();

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();

            AppHelper.SetSuccess(this, $"🗑 Player '{player.FullName}' deleted.");
            return RedirectToAction(nameof(Index));
        }


        // ============================
        // BULK DELETE
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                AppHelper.SetError(this, "❌ No players selected.");
                return RedirectToAction(nameof(Index));
            }

            var players = await _context.Players
                .Where(p => selectedIds.Contains(p.PlayerId))
                .ToListAsync();

            var names = players.Select(p => p.FullName).ToList();

            _context.Players.RemoveRange(players);
            await _context.SaveChangesAsync();

            AppHelper.SetSuccess(this, $"🗑 Deleted players: {string.Join(", ", names)}");
            return RedirectToAction(nameof(Index));
        }
    }
}
