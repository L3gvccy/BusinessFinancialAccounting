using MAUI.Pages;
using System.Text.Json;

namespace MAUI
{
    public partial class AppShell : Shell
    {
        private readonly HttpClient _http;
        public AppShell(HttpClient http)
        {
            InitializeComponent();
            _http = http;

            Routing.RegisterRoute(nameof(Pages.LoginPage), typeof(Pages.LoginPage));
            Routing.RegisterRoute(nameof(Pages.RegisterPage), typeof(Pages.RegisterPage));
            Routing.RegisterRoute(nameof(Pages.HomePage), typeof(Pages.HomePage));
            Routing.RegisterRoute(nameof(Pages.ProfilePage), typeof (Pages.ProfilePage));
            Routing.RegisterRoute(nameof(Pages.CashPage), typeof(Pages.CashPage));
            Routing.RegisterRoute(nameof(Pages.ProductsPage), typeof(Pages.ProductsPage));
            Routing.RegisterRoute(nameof(Pages.AddProductPage), typeof(Pages.AddProductPage));
            Routing.RegisterRoute(nameof(Pages.EditProductPage), typeof(Pages.EditProductPage));
            Routing.RegisterRoute(nameof(Pages.SalePage), typeof(Pages.SalePage));
            Routing.RegisterRoute(nameof(Pages.ReportsPage), typeof(Pages.ReportsPage));
            Routing.RegisterRoute(nameof(Pages.ReportsDetailPage), typeof(Pages.ReportsDetailPage));

            ShowProfileButton(false);
        }

        public void ShowProfileButton(bool show)
        {
            if (show)
            {
                if (!ToolbarItems.Contains(ProfileButton))
                    ToolbarItems.Add(ProfileButton);
            }
            else
            {
                if (ToolbarItems.Contains(ProfileButton))
                    ToolbarItems.Remove(ProfileButton);
            }
        }

        private async void OnProfileClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheetAsync(
                "Профіль",
                "Скасувати",
                null,
                "Переглянути профіль",
                "Вийти"
            );

            switch (action)
            {
                case "Переглянути профіль":
                    await Shell.Current.GoToAsync(nameof(ProfilePage));
                    break;
                case "Вийти":
                    try
                    {
                        var response = await _http.PostAsync("account/logout", null);

                        string content = await response.Content.ReadAsStringAsync();
                        string message = "Ви успішно вийшли з акаунту!";

                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            try
                            {
   
                                using var doc = JsonDocument.Parse(content);
                                if (doc.RootElement.TryGetProperty("alertMsg", out var alertMsg))
                                {
                                    message = alertMsg.GetString();
                                }
                            }
                            catch
                            {
                                // JSON некоректний
                            }
                        }

                        ShowProfileButton(false);
                        await DisplayAlertAsync("Вихід", message, "OK");

                        await Shell.Current.GoToAsync("//LoginPage");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlertAsync("Помилка", $"Не вдалося вийти: {ex.Message}", "OK");
                    }

                    break;
            }
        }

    }
}
