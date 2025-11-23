using MAUI.Models;
using System.Net.Http.Json;

namespace MAUI.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly HttpClient _http;

    public ProfilePage(HttpClient http)
    {
        InitializeComponent();
        _http = http;

        LoadProfile();
    }

    private async void LoadProfile()
    {
        try
        {
            var response = await _http.GetAsync("account/profile");
            if (response.IsSuccessStatusCode)
            {
                var profile = await response.Content.ReadFromJsonAsync<ProfileDTO>();
                if (profile != null)
                {
                    FullNameEntry.Text = profile.FullName;
                    PhoneEntry.Text = profile.Phone;
                    EmailEntry.Text = profile.Email;
                }
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                await DisplayAlertAsync("Помилка", json, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Помилка", ex.Message, "OK");
        }
    }

    private bool ValidateProfile()
    {
        if (string.IsNullOrWhiteSpace(FullNameEntry.Text))
        {
            DisplayAlertAsync("Помилка", "Вкажіть повне ім'я", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(PhoneEntry.Text))
        {
            DisplayAlertAsync("Помилка", "Вкажіть телефон", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            DisplayAlertAsync("Помилка", "Вкажіть email", "OK");
            return false;
        }

        if (!EmailEntry.Text.Contains("@"))
        {
            DisplayAlertAsync("Помилка", "Некоректний email", "OK");
            return false;
        }

        return true;
    }


    private bool ValidatePassword()
    {
        if (string.IsNullOrWhiteSpace(OldPasswordEntry.Text))
        {
            DisplayAlertAsync("Помилка", "Вкажіть старий пароль", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(NewPasswordEntry.Text))
        {
            DisplayAlertAsync("Помилка", "Вкажіть новий пароль", "OK");
            return false;
        }

        if (NewPasswordEntry.Text.Length < 6)
        {
            DisplayAlertAsync("Помилка", "Новий пароль повинен містити щонайменше 6 символів", "OK");
            return false;
        }

        return true;
    }


    private async void OnUpdateProfileClicked(object sender, EventArgs e)
    {
        if (!ValidateProfile())
            return;

        var model = new UpdateProfileDTO
        {
            FullName = FullNameEntry.Text,
            Phone = PhoneEntry.Text,
            Email = EmailEntry.Text
        };

        var response = await _http.PutAsJsonAsync("account/profile", model);
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            await DisplayAlertAsync("Успіх", "Дані профілю оновлено", "OK");
        else
            await DisplayAlertAsync("Помилка", content, "OK");
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        if (!ValidatePassword())
            return;

        var model = new ChangePasswordDTO
        {
            OldPassword = OldPasswordEntry.Text,
            NewPassword = NewPasswordEntry.Text
        };

        var response = await _http.PostAsJsonAsync("account/change-password", model);
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            await DisplayAlertAsync("Успіх", "Пароль змінено", "OK");
            OldPasswordEntry.Text = string.Empty;
            NewPasswordEntry.Text = string.Empty;
        }
        else
            await DisplayAlertAsync("Помилка", content, "OK");
    }
}