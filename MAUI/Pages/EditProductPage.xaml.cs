using System.Net.Http.Json;
using MAUI.Models;

namespace MAUI.Pages;

[QueryProperty(nameof(ProductId), "id")]
public partial class EditProductPage : ContentPage
{
    private readonly HttpClient _http;
    public int ProductId { get; set; }

    public EditProductPage(HttpClient http)
    {
        InitializeComponent();
        _http = http;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (ProductId == 0) return;

        try
        {
            var product = await _http.GetFromJsonAsync<ProductDTO>($"product/edit/{ProductId}");
            if (product != null)
            {
                CodeEntry.Text = product.Code.ToString();
                NameEntry.Text = product.Name;
                UnitsPicker.SelectedItem = product.Units;
                QuantityEntry.Text = product.Quantity.ToString("0.##");
                PriceEntry.Text = product.Price.ToString("0.##");
            }
        }
        catch
        {
            await DisplayAlertAsync("Помилка", "Не вдалося завантажити товар", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlertAsync("Помилка", "Назва не може бути порожньою", "OK");
            return;
        }

        if (UnitsPicker.SelectedItem == null)
        {
            await DisplayAlertAsync("Помилка", "Виберіть одиниці виміру", "OK");
            return;
        }

        if (!decimal.TryParse(QuantityEntry.Text, out var quantity) || quantity <= 0)
        {
            await DisplayAlertAsync("Помилка", "Кількість введена некоректно", "OK");
            return;
        }

        if (!decimal.TryParse(PriceEntry.Text, out var price) || price < 0)
        {
            await DisplayAlertAsync("Помилка", "Ціна введена некоректно", "OK");
            return;
        }

        var model = new ProductUpdateDTO
        {
            Id = ProductId,
            Code = int.Parse(CodeEntry.Text),
            Name = NameEntry.Text.Trim(),
            Units = UnitsPicker.SelectedItem.ToString(),
            Quantity = quantity,
            Price = price
        };

        try
        {
            var response = await _http.PostAsJsonAsync("product/edit", model);
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlertAsync("Успіх", "Товар оновлено", "OK");
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

