using System.ComponentModel.DataAnnotations;

namespace BusinessFinancialAccounting.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public User User { get; set; }
        [Required]
        public int Code { get; set; }
        [Required]
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        [Required]
        public string Units { get; set; }
        [Required]
        public decimal Price { get; set; }  

    }
}
