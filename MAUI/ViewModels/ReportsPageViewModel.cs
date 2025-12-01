using MAUI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Json;

public class ReportsPageViewModel : INotifyPropertyChanged
{
    private readonly HttpClient _http;

    public ObservableCollection<ReceiptDTO> Receipts { get; set; } = new();
    public ObservableCollection<ReportDTO> Reports { get; set; } = new();

    public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-7);
    public DateTime EndDate { get; set; } = DateTime.Today;

    public event Action<string>? OnMessage;

    public ReportsPageViewModel(HttpClient http)
    {
        _http = http;
    }

    public async Task LoadData()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponseDTO>("reports");
            if (response != null)
            {
                Receipts.Clear();
                foreach (var r in response.Receipts)
                    Receipts.Add(r);

                Reports.Clear();
                foreach (var rep in response.Reports)
                    Reports.Add(rep);
            }
        }
        catch (Exception ex)
        {
            OnMessage?.Invoke(ex.Message);
        }
    }

    public async Task GenerateReport()
    {
        try
        {
            var response = await _http.PostAsJsonAsync("reports/generate-report", new
            {
                StartDate,
                EndDate
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ReportResponseDTO>();
                if (result != null)
                    Reports.Add(result.Report);
            }
            else
            {
                var text = await response.Content.ReadAsStringAsync();
                OnMessage?.Invoke(text);
            }
        }
        catch (Exception ex)
        {
            OnMessage?.Invoke(ex.Message);
        }
    }

    public async Task ViewReport(int id)
    {
        try
        {
            var resp = await _http.GetFromJsonAsync<dynamic>($"reports/view-report/{id}");
            if (resp != null)
            {
                // Тут можна відкрити нову сторінку для детального перегляду
                OnMessage?.Invoke($"Перегляд звіту #{id} ({resp.receipts.Count} чеків)");
            }
        }
        catch (Exception ex)
        {
            OnMessage?.Invoke("Помилка перегляду: " + ex.Message);
        }
    }

    public async Task DeleteReport(int id)
    {
        try
        {
            var resp = await _http.PostAsync($"reports/delete-report/{id}", null);
            if (!resp.IsSuccessStatusCode)
            {
                OnMessage?.Invoke(await resp.Content.ReadAsStringAsync());
                return;
            }

            Reports.Remove(Reports.FirstOrDefault(r => r.Id == id)!);
            OnMessage?.Invoke($"Звіт #{id} видалено");
        }
        catch (Exception ex)
        {
            OnMessage?.Invoke("Помилка видалення: " + ex.Message);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class ApiResponseDTO
{
    public List<ReceiptDTO> Receipts { get; set; } = new();
    public List<ReportDTO> Reports { get; set; } = new();
}

public class ReportResponseDTO
{
    public ReportDTO Report { get; set; } = new();
}
