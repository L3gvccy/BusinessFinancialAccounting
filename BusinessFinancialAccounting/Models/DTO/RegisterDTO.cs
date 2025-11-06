namespace BusinessFinancialAccounting.Models.DTO
{
    public class RegisterDTO
    {
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public int CashBalance { get; set; }
        public int CardBalance { get; set; }
    }
}
