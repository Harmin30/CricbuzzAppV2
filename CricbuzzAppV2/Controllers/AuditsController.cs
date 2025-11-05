using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using CricbuzzAppV2.ViewModels;
using CricbuzzAppV2.Helpers;

namespace CricbuzzAppV2.Controllers
{
    public class AuditsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? searchString,
            string? filterAction,
            string? filterEntity,
            int page = 1)
        {
            const int PageSize = 25;

            IQueryable<Audit> query = _context.Audits.AsNoTracking();

            // 🔍 Search
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(a =>
                    a.UserName.Contains(searchString) ||
                    a.EntityName.Contains(searchString) ||
                    (a.Details != null && a.Details.Contains(searchString))
                );
            }

            // 🎯 Filters
            if (!string.IsNullOrWhiteSpace(filterAction))
                query = query.Where(a => a.Action == filterAction);

            if (!string.IsNullOrWhiteSpace(filterEntity))
                query = query.Where(a => a.EntityName == filterEntity);

            // 📊 Dropdowns
            ViewBag.Actions = await _context.Audits
                .Select(a => a.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

            ViewBag.Entities = await _context.Audits
                .Select(a => a.EntityName)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();

            // 📄 Pagination
            int totalRecords = await query.CountAsync();

            var audits = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // 🔎 Team lookup (ID → Name)
            var teamLookup = await _context.Teams
                .AsNoTracking()
                .ToDictionaryAsync(t => t.TeamId.ToString(), t => t.TeamName);

            // ✅ STEP 2 CORE FIX:
            // Convert Audit → AuditViewModel (NO LOGIC IN VIEW)
            var teams = await _context.Teams
    .ToDictionaryAsync(t => t.TeamId.ToString(), t => t.TeamName);

            var auditVMs = audits.Select(a =>
            {
                var formatted = AuditFormatter.Format(
                    a.Action,
                    a.EntityName,
                    a.Details,
                    teams
                );

                return new AuditViewModel
                {
                    Timestamp = a.Timestamp,
                    UserName = a.UserName,
                    Action = a.Action,
                    Summary = formatted.Summary,
                    Groups = formatted.Groups
                };
            }).ToList();




            // 📦 Paging metadata
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);
            ViewBag.TotalRecords = totalRecords;

            ViewBag.SearchString = searchString;
            ViewBag.FilterAction = filterAction;
            ViewBag.FilterEntity = filterEntity;

            return View(auditVMs);
        }
    }
}
