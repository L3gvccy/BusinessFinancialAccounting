using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUI.Models
{
    public class RegisterDTO
    {
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public int CashBalance { get; set; } = 0;
        public int CardBalance { get; set; } = 0;
    }
}
