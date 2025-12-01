namespace MAUI.Pages;

public partial class ReportsPage : ContentPage
{
    private readonly ReportsPageViewModel _vm;

    public ReportsPage(HttpClient http)
    {
        InitializeComponent();
        _vm = new ReportsPageViewModel(http);
        BindingContext = _vm;

        _vm.OnMessage += async (msg) =>
        {
            await DisplayAlertAsync("Повідомлення", msg, "OK");
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        LoaderOverlay.IsVisible = true;
        try
        {
            await _vm.LoadData();
        }
        finally
        {
            LoaderOverlay.IsVisible = false;
        }
    }

    private async void OnGenerateReportClicked(object sender, EventArgs e)
    {
        await _vm.GenerateReport();
    }

    private async void OnViewReportClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int reportId)
        {
            await Shell.Current.GoToAsync($"{nameof(Pages.ReportsDetailPage)}?reportId={reportId}");
        }
    }

    private async void OnDeleteReportClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int reportId)
        {
            var confirmed = await DisplayAlertAsync("Підтвердження", $"Видалити звіт #{reportId}?", "Так", "Ні");
            if (!confirmed) return;

            var vm = BindingContext as ReportsPageViewModel;
            if (vm != null)
            {
                await vm.DeleteReport(reportId);
            }
        }
    }

}