using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessFinancialAccounting.Models
{
    public class ReceiptProduct
    {
        [Key]
        public int Id { get; set; }
        public int Code { get; set; }
        public string Name { get; set; }
        public string Units { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        [ForeignKey(nameof(Receipt))]
        public int ReceiptId { get; set; }
        public Receipt Receipt { get; set; }
    }
}
