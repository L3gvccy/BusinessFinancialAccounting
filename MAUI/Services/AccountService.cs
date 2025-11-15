using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using MAUI.Models;

namespace MAUI.Services
{
    internal class AccountService
    {
        private readonly HttpClient _httpClient;
        public AccountService(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<ApiResponse> RegisterAsync(RegisterDTO model)
        {
            var response = await _httpClient.PostAsJsonAsync("account/register", model);
            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return new ApiResponse { Success = response.IsSuccessStatusCode, Message = content?.Message ?? "" };
        }

        public async Task<ApiResponse> LoginAsync(LoginDTO model)
        {
            var response = await _httpClient.PostAsJsonAsync("account/login", model);
            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return new ApiResponse { Success = response.IsSuccessStatusCode, Message = content?.Message ?? "" };
        }
    }
}
