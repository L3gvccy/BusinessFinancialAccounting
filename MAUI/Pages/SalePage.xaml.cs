using MAUI.ViewModels;
using MAUI.Models;

namespace MAUI.Pages;

public partial class SalePage : ContentPage
{
    private readonly SalePageViewModel _vm;

    public SalePage(HttpClient http)
    {
        InitializeComponent();
        _vm = new SalePageViewModel(http);
        BindingContext = _vm;

        _vm.OnMessage += async (msg) =>
        {
            await DisplayAlertAsync("Повідомлення", msg, "OK");
        };
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        _vm.CodeInput = CodeEntry.Text;
        await _vm.AddProduct();
        CodeEntry.Text = "";
    }

    private async void OnQtyChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry && entry.BindingContext is CartItemDTO item)
        {
            if (decimal.TryParse(entry.Text, out decimal q))
            {
                item.Quantity = q;
                await _vm.ChangeQuantity(item);
            }
        }
    }

    private async void OnPayCash(object sender, EventArgs e)
    {
        await _vm.Pay("cash");
    }

    private async void OnPayCard(object sender, EventArgs e)
    {
        await _vm.Pay("card");
    }
}
