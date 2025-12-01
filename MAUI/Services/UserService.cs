using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using MAUI.Models;

namespace MAUI.Services
{
    internal class UserService
    {
        private readonly HttpClient _httpClient;
        public UserService(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<UserDTO?> GetCurrentUserAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<UserDTO>("home/me");
            return response;
        }
    }
}
