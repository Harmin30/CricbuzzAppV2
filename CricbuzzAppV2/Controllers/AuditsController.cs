using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;

namespace CricbuzzAppV2.Controllers
{
    public class AuditsController : Controller
    {
    private readonly ApplicationDbContext _context;

   public AuditsController(ApplicationDbContext context)
        {
        _context = context;
        }

        // GET: Audits
        public async Task<IActionResult> Index(string searchString, string filterAction, string filterEntity)
    {
            var query = _context.Audits.AsQueryable();

       // Apply filters
            if (!string.IsNullOrEmpty(searchString))
     {
  query = query.Where(a =>
   a.UserName.Contains(searchString) ||
        a.EntityName.Contains(searchString) ||
    a.Details.Contains(searchString));
      }

     if (!string.IsNullOrEmpty(filterAction))
            {
         query = query.Where(a => a.Action == filterAction);
     }

            if (!string.IsNullOrEmpty(filterEntity))
   {
            query = query.Where(a => a.EntityName == filterEntity);
            }

  // Get distinct values for dropdowns
      ViewBag.Actions = await _context.Audits.Select(a => a.Action).Distinct().ToListAsync();
     ViewBag.Entities = await _context.Audits.Select(a => a.EntityName).Distinct().ToListAsync();

            // Order by most recent first
            var audits = await query.OrderByDescending(a => a.Timestamp).ToListAsync();
        return View(audits);
        }

        // GET: Audits/Details/5
        public async Task<IActionResult> Details(int? id)
    {
            if (id == null)
            {
  return NotFound();
     }

         var audit = await _context.Audits.FirstOrDefaultAsync(m => m.AuditId == id);
 if (audit == null)
   {
        return NotFound();
            }

     return View(audit);
        }
    }
}