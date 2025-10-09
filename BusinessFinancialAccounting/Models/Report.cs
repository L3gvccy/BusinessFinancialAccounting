using System.ComponentModel.DataAnnotations;

namespace BusinessFinancialAccounting.Models
{
    /// <summary>
    /// Звіт
    /// </summary>
    public class Report
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal CashSales { get; set; }
        public decimal CardSales { get; set; }
        public decimal CashWithdrawals { get; set; }
        public decimal CardWithdrawals { get; set; }
        public decimal CashDeposits { get; set; }
        public decimal CardDeposits { get; set; }

        public decimal CashProfit { get; set; }
        public decimal CardProfit { get; set; }

        public decimal Tax { get; set; }  // наприклад 20% від продажів
    }

}
