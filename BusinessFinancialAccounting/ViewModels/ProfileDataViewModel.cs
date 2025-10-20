using System.ComponentModel.DataAnnotations;

namespace BusinessFinancialAccounting.Models
{
    public class ProfileDataViewModel
    {
        [Required(ErrorMessage = "Повне ім'я обов'язкове")]
        [MaxLength(500, ErrorMessage = "Повне ім'я не може перевищувати 500 символів")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Телефон обов'язковий")]
        [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Невірний формат телефону. Використовуйте формат +380XXXXXXXXX")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Електронна пошта обов'язкова")]
        [EmailAddress(ErrorMessage = "Невірний формат електронної пошти")]
        public string Email { get; set; }

        public bool IsGoogleLinked { get; set; }
    }
}
