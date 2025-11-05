using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using CricbuzzAppV2.Helpers;
using Microsoft.AspNetCore.Hosting;
using CricbuzzAppV2.ViewModels;


namespace CricbuzzAppV2.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TeamsController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ============================
        // GET: Teams
        // ============================
        public async Task<IActionResult> Index()
        {
            var teams = await _context.Teams.ToListAsync();
            return View(teams);
        }

        // ============================
        // GET: Teams/Details/5
        // ============================
        public async Task<IActionResult> Details(int id)
        {
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.TeamId == id);

            if (team == null)
                return NotFound();

            return View(team);
        }

        // ============================
        // GET: Teams/Create
        // ============================
        public IActionResult Create()
        {
            ViewBag.Countries = CountryList.All;
            return View(new TeamCreateViewModel());
        }

        // ============================
        // POST: Teams/Create
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeamCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Countries = CountryList.All;
                return View(model);
            }


            string imagePath = "https://th.bing.com/th/id/OIP.FuJz0KDQ05jfCqQmo1rypwAAAA?w=167&h=176&c=7&r=0&o=7&pid=1.7&rm=3";

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string uploadFolder = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "images/teams");

                Directory.CreateDirectory(uploadFolder);

                string fileName = Guid.NewGuid() +
                                  Path.GetExtension(model.ImageFile.FileName);

                string filePath = Path.Combine(uploadFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);

                imagePath = "/images/teams/" + fileName;
            }
            else if (!string.IsNullOrWhiteSpace(model.ImageUrlInput))
            {
                imagePath = model.ImageUrlInput;
            }

            var team = new Team
            {
                TeamName = model.TeamName,
                Country = model.Country,
                Coach = model.Coach,
                ImageUrl = imagePath
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // ============================
        // GET: Teams/Edit/5
        // ============================
        public async Task<IActionResult> Edit(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
                return NotFound();

            var vm = new TeamEditViewModel
            {
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                Country = team.Country,
                Coach = team.Coach,
                ExistingImageUrl = team.ImageUrl
            };

            ViewBag.Countries = CountryList.All;
            return View(vm);
        }


        // ============================
        // POST: Teams/Edit/5
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TeamEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Countries = CountryList.All;
                return View(model);
            }

            var team = await _context.Teams.FindAsync(model.TeamId);
            if (team == null)
                return NotFound();

            // Update text fields
            team.TeamName = model.TeamName;
            team.Country = model.Country;
            team.Coach = model.Coach;

            // Default: keep existing image
            string imagePath = team.ImageUrl;

            // Same priority logic as Create
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string uploadFolder = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "images/teams");

                Directory.CreateDirectory(uploadFolder);

                string fileName = Guid.NewGuid() +
                                  Path.GetExtension(model.ImageFile.FileName);

                string filePath = Path.Combine(uploadFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);

                imagePath = "/images/teams/" + fileName;
            }
            else if (!string.IsNullOrWhiteSpace(model.ImageUrlInput))
            {
                imagePath = model.ImageUrlInput;
            }

            team.ImageUrl = imagePath;

            await _context.SaveChangesAsync();

            AppHelper.SetSuccess(this, "✏️ Team updated successfully.");
            return RedirectToAction(nameof(Index));
        }



        // ============================
        // GET: Teams/Delete/5
        // ============================
        public async Task<IActionResult> Delete(int id)
        {
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.TeamId == id);

            if (team == null)
                return NotFound();

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
                return NotFound();

            bool hasMatches = await _context.Matches
                .AnyAsync(m => m.TeamAId == id || m.TeamBId == id);

            if (hasMatches)
            {
                AppHelper.SetError(this,
                    $"❌ Team '{team.TeamName}' cannot be deleted because it has matches.");
                return RedirectToAction(nameof(Index));
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            AppHelper.SetSuccess(this,
                $"🗑 Team '{team.TeamName}' deleted successfully.");

            return RedirectToAction(nameof(Index));
        }


        // ============================
        // POST: Teams/Bulk Delete
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                AppHelper.SetError(this, "❌ No teams selected for deletion.");
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
                    .AnyAsync(m =>
                        m.TeamAId == team.TeamId ||
                        m.TeamBId == team.TeamId);

                if (hasMatches)
                {
                    skippedTeams.Add(
                        $"❌ Team '{team.TeamName}' cannot be deleted because it has matches.");
                    continue;
                }

                _context.Teams.Remove(team);
                deletedTeams.Add(team.TeamName);
            }

            await _context.SaveChangesAsync();

            if (deletedTeams.Any())
                AppHelper.SetSuccess(this,
                    $"🗑 Deleted teams: {string.Join(", ", deletedTeams)}");

            if (skippedTeams.Any())
                AppHelper.SetError(this, string.Join("<br/>", skippedTeams));

            return RedirectToAction(nameof(Index));
        }
    }
}
