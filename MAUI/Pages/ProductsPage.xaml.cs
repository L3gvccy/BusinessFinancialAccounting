using MAUI.Models;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text.Json;

namespace MAUI.Pages;

public partial class ProductsPage : ContentPage
{
    private readonly HttpClient _http;

    public ProductsPage(HttpClient http)
    {
        InitializeComponent();
        _http = http;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LoaderOverlay.IsVisible = true;
        try
        {
            await LoadProductsAsync();

        }
        finally
        {
            LoaderOverlay.IsVisible = false;
        }
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            var response = await _http.GetAsync("product/products");
            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlertAsync("Помилка", "Не вдалося завантажити товари", "OK");
                return;
            }

            var json = await response.Content.ReadAsStringAsync();

            var products = JsonSerializer.Deserialize<List<ProductDTO>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            });

            ProductsCollection.ItemsSource = products ?? new List<ProductDTO>();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Помилка", $"Сталася помилка при завантаженні: {ex.Message}", "OK");
        }
    }

    private async void OnAddProductClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddProductPage));
    }

    private async void OnEditProductClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int productId)
        {
            await Shell.Current.GoToAsync($"EditProductPage?id={productId}");
        }
    }
}
