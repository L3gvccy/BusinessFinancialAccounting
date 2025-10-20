using System.ComponentModel.DataAnnotations;

namespace BusinessFinancialAccounting.Models
{
    /// <summary>
    /// Користувач
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Логін обов'язковий")]
        [MaxLength(50, ErrorMessage = "Логін не може перевищувати 50 символів")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Повне ім'я обов'язкове")]
        [MaxLength(500, ErrorMessage = "Повне ім'я не може перевищувати 500 символів")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Пароль обов'язковий")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,16}$", ErrorMessage = "Пароль має бути від 8 до 16 символів, містити хоча б одну велику літеру, цифру та спеціальний символ")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Телефон обов'язковий")]
        [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Невірний формат телефону. Використовуйте формат +380XXXXXXXXX")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Електронна пошта обов'язкова")]
        [EmailAddress(ErrorMessage = "Невірний формат електронної пошти")]
        public string Email { get; set; }
        [MaxLength(100)]
        public string? OAuthId { get; set; }

        [MaxLength(50)]
        public string? OAuthProvider { get; set; }
    }
}
