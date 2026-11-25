using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        public int BookId { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string ISBN { get; set; }

        public int Quantity { get; set; }

        public int AvailableQuantity { get; set; }

        // Foreign key - used internally/database
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        // Used only on the Create/Edit form
        // It is NOT stored in the Books table
        [NotMapped]
        public string CategoryName { get; set; }

        public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
    }
}