using MAUI.Models;
using Microcharts;
using SkiaSharp;
using System.Net.Http.Json;

namespace MAUI.Pages;

[QueryProperty(nameof(ReportId), "reportId")]
public partial class ReportsDetailPage : ContentPage
{
    private readonly HttpClient _http;

    public int ReportId { get; set; }

    public ReportsDetailPage(HttpClient http)
    {
        InitializeComponent();
        _http = http;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LoaderOverlay.IsVisible = true;

        if (ReportId == 0) return;

        try
        {
            var response = await _http.GetFromJsonAsync<ReportDetailsDTO>($"reports/view-report/{ReportId}");
            if (response != null)
            {
                BindingContext = response;

                var productTotals = response.Receipts
                    .SelectMany(r => r.Products)
                    .GroupBy(p => p.Name)
                    .Select(g => new
                    {
                        Name = g.Key,
                        Quantity = g.Sum(p => p.Quantity)
                    })
                    .ToList();

                var chartEntries = productTotals
                    .Select(p => new ChartEntry((float)p.Quantity)
                    {
                        Label = p.Name,
                        ValueLabel = p.Quantity.ToString("F2"),
                        Color = SKColor.Parse("#3074A1")
                    })
                    .ToList();

                Dispatcher.Dispatch(() =>
                {
                    SalesChart.Chart = new BarChart { Entries = chartEntries };
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Помилка", $"Не вдалося завантажити звіт: {ex.Message}", "OK");
        }
        finally
        {
            LoaderOverlay.IsVisible = false;
        }
    }

}
