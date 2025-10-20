using System.ComponentModel.DataAnnotations;

namespace BusinessFinancialAccounting.Models
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,16}$",
            ErrorMessage = "Пароль має бути від 8 до 16 символів, містити хоча б одну велику літеру, цифру та символ.")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; }
    }
}
