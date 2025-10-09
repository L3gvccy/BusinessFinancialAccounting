using System.ComponentModel.DataAnnotations;

namespace BusinessFinancialAccounting.Models
{
    /// <summary>
    /// Модель касового апарату, що зберігає інформацію про баланс готівки та картки для користувача.
    /// </summary>
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
