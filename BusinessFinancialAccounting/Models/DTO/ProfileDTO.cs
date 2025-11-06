using System.ComponentModel.DataAnnotations;

namespace BusinessFinancialAccounting.Models.DTO
{
    public class ProfileDTO
    {
        public string FullName { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string Email { get; set; } = default!;
        public bool IsGoogleLinked { get; set; }
    }

    public class UpdateProfileDTO
    {
        public string FullName { get; set; } = default!;
        public string Phone { get; set; } = default!;

        public string Email { get; set; } = default!;
    }

    public class ChangePasswordDTO
    {
        public string NewPassword { get; set; } = default!;

        public string? ConfirmPassword { get; set; }
    }
}
