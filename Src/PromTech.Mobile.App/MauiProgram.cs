using CommunityToolkit.Maui;

using Microsoft.Extensions.Logging;

using PromTech.Mobile.App.Pages.Login;
using PromTech.Mobile.App.Pages.Messenger;
using PromTech.Mobile.Core.Interfaces;
using PromTech.Mobile.Core.Services.LocalStorage;
using PromTech.Mobile.TCP.Interfaces;
using PromTech.Mobile.TCP.Services;

namespace PromTech.Mobile.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .RegisterAppServices()
            .RegisterViewModels()
            .RegisterViews();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static MauiAppBuilder RegisterAppServices(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddSingleton<AppShell>();
        mauiAppBuilder.Services.AddSingleton<ITcpClient, TcpClientService>();
        mauiAppBuilder.Services.AddSingleton<ILocalStorage, PreferenceLocalStorage>();

        return mauiAppBuilder;
    }

    private static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddTransient<LoginPageViewModel>();
        mauiAppBuilder.Services.AddTransient<MessengerPageViewModel>();

        return mauiAppBuilder;
    }

    private static MauiAppBuilder RegisterViews(this MauiAppBuilder mauiAppBuilder)
    {
        mauiAppBuilder.Services.AddTransient<LoginPage>();
        mauiAppBuilder.Services.AddTransient<MessengerPage>();

        return mauiAppBuilder;
    }
}