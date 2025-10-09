using System.ComponentModel.DataAnnotations;

namespace BusinessFinancialAccounting.Models
{
    /// <summary>
    /// Фінансова операція
    /// </summary>
    public class FinancialOperation
    {
        /// <summary>
        /// Унікальний ідентифікатор фінансової операції
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Користувач, який виконав операцію
        /// </summary>
        [Required]
        public User User { get; set; }
        
        public int CashBalanceIncrease { get; set; }
        public int CashBalanceDecrease { get; set; }
        public int CardBalanceIncrease { get; set; }
        public int CardBalanceDecrease { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now;
    }
}
