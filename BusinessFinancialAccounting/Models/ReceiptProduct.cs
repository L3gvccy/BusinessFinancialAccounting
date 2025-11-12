using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BusinessFinancialAccounting.Models
{
    /// <summary>
    /// Клас продукту в чеку
    /// </summary>
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
        [JsonIgnore]
        public Receipt Receipt { get; set; }
    }
}
