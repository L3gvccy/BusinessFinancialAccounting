using System.ComponentModel.DataAnnotations;

namespace BusinessFinancialAccounting.Models
{
    /// <summary>
    /// Клас чеку
    /// </summary>
    public class Receipt
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public User User { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        [Required]
        public string PaymentMethod { get; set; }
        public ICollection<ReceiptProduct> Products { get; set; } = new List<ReceiptProduct>();
    }
}
