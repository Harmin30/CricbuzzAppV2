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

          // Log the creation
         await AuditHelper.LogCreate(_context, HttpContext, "Team", team.TeamId.ToString(), 
     $"Created team: {team.TeamName} from {team.Country}");

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

        // Log the update
 await AuditHelper.LogUpdate(_context, HttpContext, "Team", team.TeamId.ToString(),
        $"Updated team: {team.TeamName} from {team.Country}");

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

      // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
     [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.FindAsync(id);
  if (team == null)
            {
       AppHelper.SetError(this, "Team not found.");
                return RedirectToAction(nameof(Index));
            }

    // Check if team has matches
            bool hasMatches = await _context.Matches
       .AnyAsync(m => m.TeamAId == team.TeamId || m.TeamBId == team.TeamId);

          if (hasMatches)
  {
       AppHelper.SetError(this, $"❌ Team '{team.TeamName}' cannot be deleted because it has matches scheduled or completed.");
  return RedirectToAction(nameof(Index));
          }

      _context.Teams.Remove(team);
       await _context.SaveChangesAsync();

     // Log the deletion
            await AuditHelper.LogDelete(_context, HttpContext, "Team", id.ToString(),
             $"Deleted team: {team.TeamName} from {team.Country}");

       AppHelper.SetSuccess(this, $"✅ Team '{team.TeamName}' deleted successfully.");
            return RedirectToAction(nameof(Index));
        }

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

  // Log each deletion
  await AuditHelper.LogDelete(_context, HttpContext, "Team", team.TeamId.ToString(),
     $"Deleted team: {team.TeamName} from {team.Country}");
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
