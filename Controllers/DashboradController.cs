using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            string role = HttpContext.Session.GetString("Role");
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(role) || userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (role != "Admin" && role != "User")
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.TotalBooks = await _context.Books.CountAsync();

            ViewBag.AvailableBooks = await _context.Books
                .CountAsync(b => b.AvailableQuantity > 0);

            if (role == "Admin")
            {
                var overdueBorrowings = await _context.Borrowings
                    .Include(b => b.User)
                    .Include(b => b.Book)
                    .Where(b =>
                        b.Status == "Borrowed" &&
                        b.DueDate < DateTime.Now)
                    .OrderBy(b => b.DueDate)
                    .ToListAsync();

                ViewBag.OverdueBorrowings = overdueBorrowings;
                ViewBag.OverdueBooks = overdueBorrowings.Count;

                ViewBag.IssuedBooks = await _context.Borrowings
                    .CountAsync(b => b.Status == "Borrowed");

                ViewBag.TotalUsers = await _context.Users
                    .CountAsync();
            }
            else
            {
                ViewBag.OverdueBorrowings = new List<Borrowing>();

                ViewBag.IssuedBooks = await _context.Borrowings
                    .CountAsync(b =>
                        b.UserId == userId.Value &&
                        b.Status == "Borrowed");

                ViewBag.OverdueBooks = await _context.Borrowings
                    .CountAsync(b =>
                        b.UserId == userId.Value &&
                        b.Status == "Borrowed" &&
                        b.DueDate < DateTime.Now);
            }

            return View();
        }
    }
}