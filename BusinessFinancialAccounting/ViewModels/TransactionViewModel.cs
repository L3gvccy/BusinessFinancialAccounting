namespace BusinessFinancialAccounting.Models
{
    /// <summary>
    /// Модель для проведення фінансової операції
    /// </summary>
    public class TransactionViewModel
    {
        public string AccountType { get; set; }   // "cash" або "card"
        public string ActionType { get; set; }    // "deposit" або "withdraw"
        public int MoneyAmount { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
