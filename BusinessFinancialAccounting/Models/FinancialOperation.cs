using System.ComponentModel.DataAnnotations;

namespace BusinessFinancialAccounting.Models
{
    public class FinancialOperation
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public User User { get; set; }
        public int CashBalanceIncrease { get; set; }
        public int CashBalanceDecrease { get; set; }
        public int CardBalanceIncrease { get; set; }
        public int CardBalanceDecrease { get; set; }
    }
}
