using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using System.Net.Http.Headers;
using MAUI.Services;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .ConfigureSyncfusionToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            string apiBase;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                apiBase = "http://10.0.2.2:5081/api/";
            }
            else if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                apiBase = "http://localhost:5081/api/";
            }
            else
            {
                apiBase = "http://192.168.1.100:5081/api/";
            }

            var httpClient = new HttpClient { BaseAddress = new Uri(apiBase) };
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            builder.Services.AddSingleton(httpClient);
            builder.Services.AddSingleton<AccountService>();
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<AppShell>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
