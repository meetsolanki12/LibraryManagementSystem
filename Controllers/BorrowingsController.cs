using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    public class BorrowingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BorrowingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ADMIN: View all borrowing records
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var borrowings = await _context.Borrowings
                .Include(b => b.User)
                .Include(b => b.Book)
                .OrderByDescending(b => b.BorrowingId)
                .ToListAsync();

            return View(borrowings);
        }

        // ADMIN: Issue a requested book with a selected return period
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Issue(int id, int returnDays)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (returnDays < 1 || returnDays > 25)
            {
                TempData["Error"] = "Return period must be between 1 and 25 days.";
                return RedirectToAction("Index");
            }

            var borrowing = await _context.Borrowings
                .FirstOrDefaultAsync(b => b.BorrowingId == id);

            if (borrowing == null)
            {
                TempData["Error"] = "Borrowing record not found.";
                return RedirectToAction("Index");
            }

            if (borrowing.Status != "Requested")
            {
                TempData["Error"] = "This book is not waiting for issue.";
                return RedirectToAction("Index");
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.BookId == borrowing.BookId);

            if (book == null || book.AvailableQuantity <= 0)
            {
                TempData["Error"] = "This book is currently unavailable.";
                return RedirectToAction("Index");
            }

            borrowing.Status = "Borrowed";
            borrowing.IssueDate = DateTime.Now;
            borrowing.DueDate = DateTime.Today.AddDays(returnDays);

            book.AvailableQuantity--;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Book issued successfully. Return it within {returnDays} days.";

            return RedirectToAction("Index");
        }

        // USER: Return their own issued book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            if (HttpContext.Session.GetString("Role") != "User")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var borrowing = await _context.Borrowings
                .FirstOrDefaultAsync(b =>
                    b.BorrowingId == id &&
                    b.UserId == userId.Value);

            if (borrowing == null || borrowing.Status != "Borrowed")
            {
                TempData["Error"] = "This book cannot be returned.";
                return RedirectToAction("MyBooks");
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.BookId == borrowing.BookId);

            if (book == null)
            {
                TempData["Error"] = "Book record not found.";
                return RedirectToAction("MyBooks");
            }

            borrowing.Status = "Returned";
            borrowing.ReturnDate = DateTime.Now;

            book.AvailableQuantity++;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Book returned successfully.";

            return RedirectToAction("MyBooks");
        }

        // USER: View only their own borrowing records
        public async Task<IActionResult> MyBooks()
        {
            if (HttpContext.Session.GetString("Role") != "User")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var borrowings = await _context.Borrowings
                .Include(b => b.Book)
                .Where(b => b.UserId == userId.Value)
                .OrderByDescending(b => b.BorrowingId)
                .ToListAsync();

            return View(borrowings);
        }
    }
}