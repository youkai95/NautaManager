using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NautaManager.Handlers;
using NautaManager.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NautaManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider;
        public App()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                    services.AddSingleton<IUserManager, UserManager>()
                            .AddSingleton<IUsersRepository, UsersRepository>()
                            .AddSingleton<MainWindow>())
                .Build();
            ServiceProvider = host.Services;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = ServiceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
    }
}
