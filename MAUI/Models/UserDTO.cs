using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUI.Models
{
    public class UserDTO
    {
        public bool IsAuthenticated { get; set; }
        public string Username { get; set; } = "";
        public string UserId { get; set; } = "";
    }
}
