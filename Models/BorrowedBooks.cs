using System.ComponentModel.DataAnnotations;

namespace staj22.Models
{
    public class BorrowedBooks
    {
        [Required]
        [Key]
        public string? ID_NUMBER { get; set; }
        [Required]
        public string? FULL_NAME { get; set; }
        [Required]
        public string? PHONE_NUMBER { get; set; }
        [Required]
        public string? FDATE { get; set; }
        [Required]
        public string? LDATE { get; set; }
        [Required]
        public string? BBARCODE { get; set; }
        [Required]
        public string? BOOK_NAME { get; set; }
        
    }
}
