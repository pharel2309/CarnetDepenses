using CarnetDepenses.Services;
using CarnetDepenses.ViewModels;
using Microsoft.Extensions.Logging;

namespace CarnetDepenses
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<IDepenseRepository, SqliteDepenseRepository>();
            builder.Services.AddSingleton(
               new HttpClient(new FakeApiHandler()));
            builder.Services.AddSingleton<IApiService, ApiService>();
            builder.Services.AddSingleton<DepensesViewModel>();
            builder.Services.AddSingleton<MainPage>();

            return builder.Build();
        }
    }
}
