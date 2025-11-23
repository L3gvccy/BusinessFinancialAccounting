using System.Net.Http.Json;
using System.Text.Json;

namespace MAUI.Pages;

public partial class CashPage : ContentPage
{
    private readonly HttpClient _http;

    public CashPage(HttpClient http)
    {
        InitializeComponent();
        LoaderOverlay.IsVisible = true;
        _http = http;
        LoadBalance();

    }

    private async void LoadBalance()
    {
        try
        {
            var response = await _http.GetAsync("cash/cashregister");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                decimal cash = doc.RootElement.GetProperty("cash").GetDecimal();
                decimal card = doc.RootElement.GetProperty("card").GetDecimal();

                CashBalanceLabel.Text = $"{cash} грн";
                CardBalanceLabel.Text = $"{card} грн";
            }
            else
            {
                await DisplayAlertAsync("Помилка", "Не вдалося завантажити баланс", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Помилка", ex.Message, "OK");
        }
        finally
        {
            LoaderOverlay.IsVisible = false;
        }
    }

    private async void OnTransactionClicked(object sender, EventArgs e)
    {
        if (AccountTypePicker.SelectedIndex == -1 || ActionTypePicker.SelectedIndex == -1 || string.IsNullOrWhiteSpace(AmountEntry.Text))
        {
            await DisplayAlertAsync("Помилка", "Будь ласка, заповніть всі поля", "OK");
            return;
        }

        if (!decimal.TryParse(AmountEntry.Text, out var amount) || amount <= 0)
        {
            await DisplayAlertAsync("Помилка", "Некоректна сума", "OK");
            return;
        }

        var model = new
        {
            AccountType = AccountTypePicker.SelectedIndex == 0 ? "cash" : "card",
            ActionType = ActionTypePicker.SelectedIndex == 0 ? "deposit" : "withdraw",
            MoneyAmount = amount
        };

        try
        {
            var response = await _http.PostAsJsonAsync("cash/transaction", model);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                string message = "Операція успішна";
                try
                {
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("message", out var msg))
                        message = msg.GetString();
                }
                catch { }

                await DisplayAlertAsync("Успіх", message, "OK");
                LoadBalance();
            }
            else
            {
                string errorMsg = "Помилка виконання операції";
                try
                {
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("message", out var msg))
                        errorMsg = msg.GetString();
                }
                catch { }

                await DisplayAlertAsync("Помилка", errorMsg, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Помилка", ex.Message, "OK");
        }
    }
}
