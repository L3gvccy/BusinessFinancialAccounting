using System;
using System.Collections.Generic;
using System.Text;

namespace MAUI.Models
{
    class ProfileDTO
    {
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsGoogleLinked { get; set; }
    }

    public class UpdateProfileDTO
    {
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
    }

    public class ChangePasswordDTO
    {
        public string OldPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }
}
