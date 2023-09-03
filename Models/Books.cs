using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace staj22.Models
{
    public class Books
    {
        [Required]
        [Key]
        public string? BARCODE { get; set; }
        [Required]
        public string? NAME { get; set; }
        [Required]
        public string? AUTHOR { get; set; }
        [Required]
        public string? PAGE { get; set; }



    }
}
