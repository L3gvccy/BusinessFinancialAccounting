using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;
using MAUI.Models;

namespace MAUI.ViewModels;

public class SalePageViewModel : INotifyPropertyChanged
{
    private readonly HttpClient _http;

    public ObservableCollection<CartItemDTO> Cart { get; set; } = new();

    public event Action<string>? OnMessage;

    private string _codeInput = "";
    public string CodeInput
    {
        get => _codeInput;
        set
        {
            _codeInput = value;
            OnPropertyChanged(nameof(CodeInput));
        }
    }

    // -------------------------
    // Загальна сума
    // -------------------------
    public decimal Total => Cart.Sum(x => x.Quantity * x.Price);

    public SalePageViewModel(HttpClient http)
    {
        _http = http;

        Cart.CollectionChanged += (s, e) => OnPropertyChanged(nameof(Total));
    }

    // -------------------------
    // Додавання товару
    // -------------------------
    public async Task AddProduct()
    {
        if (!int.TryParse(CodeInput, out int code))
        {
            OnMessage?.Invoke("Код має бути числом");
            return;
        }

        var response = await _http.PostAsJsonAsync("sale/add-by-code", new AddByCodeDTO
        {
            Code = code,
            Quantity = 0
        });

        if (!response.IsSuccessStatusCode)
        {
            OnMessage?.Invoke(await response.Content.ReadAsStringAsync());
            return;
        }

        var data = await response.Content.ReadFromJsonAsync<AddByCodeResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            });

        if (data == null)
        {
            OnMessage?.Invoke("Помилкова відповідь сервера");
            return;
        }

        var existing = Cart.FirstOrDefault(x => x.Code == data.Product.Code);
        if (existing != null)
        {
            if (existing.Quantity + 1 > data.MaxQty)
            {
                OnMessage?.Invoke($"Максимум {data.MaxQty} {existing.Units}");
                return;
            }

            existing.Quantity += 1m;
        }
        else
        {
            Cart.Add(new CartItemDTO
            {
                Code = data.Product.Code,
                Name = data.Product.Name,
                Price = data.Product.Price,
                Units = data.Product.Units,
                Quantity = 1,
                MaxQuantity = data.MaxQty
            });
        }

        OnPropertyChanged(nameof(Total));
    }

    // -------------------------
    // Зміна кількості
    // -------------------------
    public async Task ChangeQuantity(CartItemDTO item)
    {
        OnPropertyChanged(nameof(Total));

        if (item.Quantity <= 0)
        {
            Cart.Remove(item);
            OnPropertyChanged(nameof(Total));
            return;
        }

        if (item.Quantity > item.MaxQuantity)
        {
            item.Quantity = item.MaxQuantity;
            OnMessage?.Invoke($"Максимум {item.MaxQuantity} {item.Units}");
            return;
        }

        var response = await _http.PostAsJsonAsync("sale/change-qty", new ChangeQuantityDTO
        {
            Code = item.Code,
            Quantity = item.Quantity
        });

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            OnMessage?.Invoke(err);
        }

        OnPropertyChanged(nameof(Total));
    }

    // -------------------------
    // Оплата
    // -------------------------
    public async Task<bool> Pay(string method)
    {
        if (!Cart.Any())
        {
            OnMessage?.Invoke("Кошик порожній");
            return false;
        }

        var response = await _http.PostAsJsonAsync("sale/pay", new PayRequestDTO
        {   
            Method = method,
            Products = Cart.Select(x => new PayProductDTO
            {
                Code = x.Code,
                Quantity = x.Quantity
            }).ToList()
        });

        var text = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            OnMessage?.Invoke(text);
            return false;
        }

        OnMessage?.Invoke("Оплату виконано успішно");
        Cart.Clear();
        OnPropertyChanged(nameof(Total));
        return true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
