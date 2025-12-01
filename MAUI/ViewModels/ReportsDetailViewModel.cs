using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MAUI.Models;
using Microsoft.Maui.Controls;

namespace MAUI.ViewModels;

public class ReportsDetailViewModel : INotifyPropertyChanged
{
    private readonly HttpClient _http;

    public ReportDTO Report { get; set; } = new();
    public ObservableCollection<ReceiptDTO> Receipts { get; set; } = new();

    public event Action<string>? OnMessage;

    public Command<int> ViewReceiptCommand { get; }

    public ReportsDetailViewModel(HttpClient http)
    {
        _http = http;
        ViewReceiptCommand = new Command<int>(async (id) => await ViewReceipt(id));
    }

    public async Task LoadReport(int reportId)
    {
        try
        {
            var resp = await _http.GetFromJsonAsync<dynamic>($"reports/view-report/{reportId}");
            if (resp != null)
            {
                var r = resp.report;
                Report = new ReportDTO
                {
                    Id = r.id,
                    StartDate = r.startDate,
                    EndDate = r.endDate,
                    CashSales = r.cashSales,
                    CardSales = r.cardSales,
                    CashProfit = r.cashProfit,
                    CardProfit = r.cardProfit,
                    Tax = r.tax
                };

                Receipts.Clear();
                foreach (var rc in resp.receipts)
                {
                    var productsList = new List<ReceiptProductDTO>();
                    foreach (var p in rc.products)
                    {
                        productsList.Add(new ReceiptProductDTO
                        {
                            Name = p.name,
                            Quantity = (decimal)p.quantity,
                            Price = (decimal)p.price,
                            TotalPrice = (decimal)p.totalPrice,
                            Units = p.units
                        });
                    }

                    Receipts.Add(new ReceiptDTO
                    {
                        Id = rc.id,
                        TimeStamp = rc.timeStamp,
                        PaymentMethod = rc.paymentMethod,
                        Products = productsList
                    });
                }

                OnPropertyChanged(nameof(Report));
                OnPropertyChanged(nameof(Receipts));
            }
        }
        catch (Exception ex)
        {
            OnMessage?.Invoke("Помилка завантаження звіту: " + ex.Message);
        }
    }

    private async Task ViewReceipt(int receiptId)
    {
        try
        {
            var resp = await _http.GetFromJsonAsync<dynamic>($"reports/get-reciept-details?receiptId={receiptId}");
            if (resp != null)
            {
                // Тут можна відкрити нову сторінку з деталями чеку
                OnMessage?.Invoke($"Чек #{receiptId} містить {resp.receipt.products.Count} товарів");
            }
        }
        catch (Exception ex)
        {
            OnMessage?.Invoke("Помилка завантаження чеку: " + ex.Message);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
