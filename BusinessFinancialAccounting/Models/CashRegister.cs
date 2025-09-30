using System.ComponentModel.DataAnnotations;

namespace BusinessFinancialAccounting.Models
{
    public class CashRegister
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public User User { get; set; }
        public int CashBalance { get; set; }
        public int CardBalance { get; set; }
    }
}
