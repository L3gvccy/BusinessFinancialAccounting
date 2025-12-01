using System.Net.Http.Json;

namespace MAUI.Pages;

public partial class LoginPage : ContentPage
{
    private readonly HttpClient _http;

    public LoginPage(HttpClient http)
    {
        InitializeComponent();
        _http = http;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var model = new
        {
            Username = UsernameEntry.Text,
            Password = PasswordEntry.Text
        };

        var response = await _http.PostAsJsonAsync("account/login", model);

        if (response.IsSuccessStatusCode)
        {
            var shell = (AppShell)App.Current.MainPage;
            shell.ShowProfileButton(true);

            await Shell.Current.GoToAsync("//main");
        }
        else
        {
            await DisplayAlertAsync("Помилка", "Невірний логін або пароль", "OK");
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }
}
