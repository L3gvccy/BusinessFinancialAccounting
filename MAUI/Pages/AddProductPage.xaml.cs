// AddProductPage.xaml.cs
using MAUI.Models;
using System.Net.Http.Json;

namespace MAUI.Pages;

public partial class AddProductPage : ContentPage
{
    private readonly HttpClient _http;

    public AddProductPage(HttpClient http)
    {
        InitializeComponent();
        _http = http;
    }

    private async void OnFindByCodeClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(CodeEntry.Text, out var code))
        {
            await DisplayAlertAsync("Помилка", "Некоректний код товару", "OK");
            return;
        }

        try
        {
            var product = await _http.GetFromJsonAsync<ProductBriefDTO>($"product/find-by-code?code={code}");
            if (product != null)
            {
                NameEntry.Text = product.Name;
                UnitsPicker.SelectedItem = product.Units;
                PriceEntry.Text = product.Price.ToString();
            }
            else
            {
                await DisplayAlertAsync("Інформація", "Товар не знайдено, можна додати новий", "OK");
                NameEntry.Text = "";
                UnitsPicker.SelectedIndex = -1;
                PriceEntry.Text = "";
            }
        }
        catch
        {
            await DisplayAlertAsync("Помилка", "Не вдалося знайти товар", "OK");
        }
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(CodeEntry.Text, out var code) || code <= 0)
        {
            await DisplayAlertAsync("Помилка", "Введіть правильний код товару", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlertAsync("Помилка", "Введіть назву товару", "OK");
            return;
        }

        if (UnitsPicker.SelectedItem == null)
        {
            await DisplayAlertAsync("Помилка", "Виберіть одиниці виміру", "OK");
            return;
        }

        if (!decimal.TryParse(QuantityEntry.Text, out var quantity) || quantity <= 0)
        {
            await DisplayAlertAsync("Помилка", "Введіть правильну кількість", "OK");
            return;
        }

        if (!decimal.TryParse(PriceEntry.Text, out var price) || price <= 0)
        {
            await DisplayAlertAsync("Помилка", "Введіть правильну ціну", "OK");
            return;
        }

        var model = new ProductCreateDTO
        {
            Code = code,
            Name = NameEntry.Text.Trim(),
            Units = UnitsPicker.SelectedItem.ToString(),
            Quantity = quantity,
            Price = price
        };

        try
        {
            var response = await _http.PostAsJsonAsync("product/add", model);
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlertAsync("Успіх", "Товар додано", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                await DisplayAlertAsync("Помилка", content, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Помилка", ex.Message, "OK");
        }
    }
}
