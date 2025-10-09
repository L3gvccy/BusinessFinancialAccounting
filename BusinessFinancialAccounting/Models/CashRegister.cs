using System.ComponentModel.DataAnnotations;

namespace BusinessFinancialAccounting.Models
{
    /// <summary>
    /// Модель касового апарату, що зберігає інформацію про баланс готівки та картки для користувача.
    /// </summary>
    public class CashRegister
    {
        /// <summary>
        /// Унікальний ідентифікатор касового апарату.
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Користувач, якому належить цей касовий апарат.
        /// </summary>
        [Required]
            
        public User User { get; set; }
        /// <summary>
        /// Баланс готівки в касовому апараті.
        /// </summary>
        public int CashBalance { get; set; }
        /// <summary>
        /// Баланс картки в касовому апараті.
        /// </summary>
        public int CardBalance { get; set; }
    }
}
