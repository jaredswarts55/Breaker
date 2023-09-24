using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Breaker.App.ViewModels;
using Breaker.App.Views;
using Breaker.Core.Events;
using Breaker.Core.Services;
using Breaker.Core.Services.Base;
using Breaker.Infra.Stub;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Breaker.App
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var services = new ServiceCollection();

                services.AddSingleton<MainWindow>();

                services.AddSingleton<MainWindowViewModel>();
                
                services.AddSingleton<IEventAggregator, EventAggregatorStubService>();
                services.AddSingleton<IClipboardService, ClipboardStubService>();
                services.AddSingleton<IUserSettingsService, UserSettingsService>();
                
                services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssembly(typeof(Breaker.Core.Commands.Requests.ExecuteCopyGuidRequest).Assembly));
                
                var container = services.BuildServiceProvider();

                var viewModel = ActivatorUtilities.CreateInstance<MainWindowViewModel>(container);
                // Build the service provider and set it to AvaloniaLocator.CurrentMutable

                desktop.MainWindow = new MainWindow
                {
                    DataContext = viewModel
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}