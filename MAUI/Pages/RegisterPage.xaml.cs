using MAUI.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace MAUI.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly HttpClient _http;

    public RegisterPage(HttpClient http)
    {
        InitializeComponent();
        _http = http;
    }

    private bool ValidateInputs(out string errorMessage)
    {
        errorMessage = "";

        if (string.IsNullOrWhiteSpace(UsernameEntry.Text))
        {
            errorMessage = "Будь ласка, введіть логін.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(FullNameEntry.Text))
        {
            errorMessage = "Будь ласка, введіть ПІБ.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text) || PasswordEntry.Text.Length < 6)
        {
            errorMessage = "Пароль має містити принаймні 6 символів.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(PhoneEntry.Text))
        {
            errorMessage = "Будь ласка, введіть номер телефону.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || !EmailEntry.Text.Contains("@"))
        {
            errorMessage = "Будь ласка, введіть коректний Email.";
            return false;
        }

        if (!decimal.TryParse(CashBalanceEntry.Text, out var cash) || cash < 0)
        {
            errorMessage = "Баланс готівки має бути невід'ємним числом.";
            return false;
        }

        if (!decimal.TryParse(CardBalanceEntry.Text, out var card) || card < 0)
        {
            errorMessage = "Баланс картки має бути невід'ємним числом.";
            return false;
        }

        return true;
    }


    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        if (!ValidateInputs(out string validationError))
        {
            await DisplayAlertAsync("Помилка", validationError, "OK");
            return;
        }

        var model = new
        {
            Username = UsernameEntry.Text,
            FullName = FullNameEntry.Text,
            Password = PasswordEntry.Text,
            Phone = PhoneEntry.Text,
            Email = EmailEntry.Text,
            CashBalance = decimal.Parse(CashBalanceEntry.Text),
            CardBalance = decimal.Parse(CardBalanceEntry.Text)
        };

        var response = await _http.PostAsJsonAsync("account/register", model);

        string contentString = await response.Content.ReadAsStringAsync();

        // Спроба прочитати usernameErr з JSON
        if (!string.IsNullOrWhiteSpace(contentString))
        {
            try
            {
                using var doc = JsonDocument.Parse(contentString);
                if (doc.RootElement.TryGetProperty("usernameErr", out var usernameErr))
                {
                    await DisplayAlertAsync("Помилка", usernameErr.GetString(), "OK");
                    return;
                }
            }
            catch
            {
                // Некоректний JSON — нічого не робимо
            }
        }

        // Якщо статус 200 OK, показуємо повідомлення про успіх
        if (response.IsSuccessStatusCode)
        {
            await DisplayAlertAsync("Успіх", "Реєстрація успішна!", "OK");
            await Shell.Current.GoToAsync("//LoginPage");
        }
        else
        {
            // BadRequest або інші помилки без usernameErr
            string message = !string.IsNullOrWhiteSpace(contentString) ? contentString : response.ReasonPhrase;
            await DisplayAlertAsync("Помилка", message, "OK");
        }
    }



    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
