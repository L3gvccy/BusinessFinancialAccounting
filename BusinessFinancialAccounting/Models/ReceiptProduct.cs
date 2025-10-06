using System.ComponentModel.DataAnnotations;

namespace BusinessFinancialAccounting.Models
{
    public class ReceiptProduct
    {
        [Key]
        public int Id { get; set; }
        public Product Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
