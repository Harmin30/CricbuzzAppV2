using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Collections.Generic;

namespace CricbuzzAppV2.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // -------------------- LIST USERS --------------------
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "Account");

            var users = _context.Users.ToList();
            ViewBag.Role = role;
            return View(users);
        }

        // -------------------- DELETE USER --------------------
        public IActionResult Delete(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "SuperAdmin")
            {
                TempData["ErrorMessage"] = "You do not have permission to delete users!";
                return RedirectToAction("Index");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found!";
                return RedirectToAction("Index");
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "User deleted successfully!";
            return RedirectToAction("Index");
        }

        // -------------------- GET EDIT USER --------------------
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "SuperAdmin" && role != "Editor")
            {
                TempData["ErrorMessage"] = "You do not have permission to edit users!";
                return RedirectToAction("Index");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found!";
                return RedirectToAction("Index");
            }

            if (role == "SuperAdmin")
            {
                ViewBag.Roles = new List<SelectListItem>
                {
                    new SelectListItem { Text = "SuperAdmin", Value = "SuperAdmin" },
                    new SelectListItem { Text = "Editor", Value = "Editor" },
                    new SelectListItem { Text = "Viewer", Value = "Viewer" }
                };

                foreach (var item in ViewBag.Roles as List<SelectListItem>)
                {
                    if (item.Value == user.Role)
                        item.Selected = true;
                }
            }

            return View(user);
        }

        // -------------------- POST EDIT USER --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(User model)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "SuperAdmin" && role != "Editor")
            {
                TempData["ErrorMessage"] = "You do not have permission to edit users!";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users.FirstOrDefault(u => u.UserId == model.UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found!";
                return RedirectToAction("Index");
            }

            user.Username = model.Username;

            if (role == "SuperAdmin" && !string.IsNullOrEmpty(model.Role))
            {
                user.Role = model.Role;
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "User updated successfully!";
            return RedirectToAction("Index");
        }
    }
}
