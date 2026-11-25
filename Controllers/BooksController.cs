
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;

namespace LibraryManagementSystem.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Everyone can view books
        public async Task<IActionResult> Index(bool availableOnly = false)
        {
            if (HttpContext.Session.GetString("Role") == null)
                return RedirectToAction("Index", "Login");

            var booksQuery = _context.Books
                .Include(b => b.Category)
                .AsQueryable();

            if (availableOnly)
            {
                booksQuery = booksQuery
                    .Where(b => b.AvailableQuantity > 0);
            }

            ViewBag.AvailableOnly = availableOnly;

            var books = await booksQuery.ToListAsync();

            return View(books);
        }

        // Everyone can view book details
        public async Task<IActionResult> Details(int? bookid)
        {
            if (HttpContext.Session.GetString("Role") == null)
                return RedirectToAction("Index", "Login");

            if (bookid == null)
                return NotFound();

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.BookId == bookid);

            if (book == null)
                return NotFound();

            return View(book);
        }

        // ADMIN ONLY - Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction(nameof(Index));

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("BookId,Title,Author,ISBN,Quantity,AvailableQuantity,CategoryName")]
            Book book)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction(nameof(Index));

            if (ModelState.IsValid)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c =>
                        c.CategoryName == book.CategoryName);

                if (category == null)
                {
                    category = new Category
                    {
                        CategoryName = book.CategoryName
                    };

                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                }

                book.CategoryId = category.CategoryId;

                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(book);
        }

        // ADMIN ONLY - Edit
        public async Task<IActionResult> Edit(int? bookid)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction(nameof(Index));

            if (bookid == null)
                return NotFound();

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.BookId == bookid);

            if (book == null)
                return NotFound();

            book.CategoryName = book.Category?.CategoryName;

            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int bookid,
            [Bind("BookId,Title,Author,ISBN,Quantity,AvailableQuantity,CategoryName")]
            Book book)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction(nameof(Index));

            if (bookid != book.BookId)
                return NotFound();

            if (ModelState.IsValid)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c =>
                        c.CategoryName == book.CategoryName);

                if (category == null)
                {
                    category = new Category
                    {
                        CategoryName = book.CategoryName
                    };

                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                }

                book.CategoryId = category.CategoryId;

                _context.Update(book);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(book);
        }

        // ADMIN ONLY - Delete
        public async Task<IActionResult> Delete(int? bookid)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction(nameof(Index));

            if (bookid == null)
                return NotFound();

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.BookId == bookid);

            if (book == null)
                return NotFound();

            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? bookid)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction(nameof(Index));

            var book = await _context.Books.FindAsync(bookid);

            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // USER ONLY - Request to borrow book
        public async Task<IActionResult> Borrow(int? bookid)
        {
            if (HttpContext.Session.GetString("Role") != "User")
                return RedirectToAction(nameof(Index));

            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Index", "Login");

            if (bookid == null)
                return NotFound();

            var book = await _context.Books.FindAsync(bookid);

            if (book == null)
                return NotFound();

            if (book.AvailableQuantity <= 0)
            {
                TempData["Message"] = "This book is currently unavailable.";
                return RedirectToAction(nameof(Index));
            }

            var borrowing = new Borrowing
            {
                UserId = userId.Value,
                BookId = book.BookId,
                IssueDate = DateTime.MinValue,
                DueDate = DateTime.MinValue,
                ReturnDate = null,
                Status = "Requested"
            };

            _context.Borrowings.Add(borrowing);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyBooks", "Borrowings");
        }
    }
}
