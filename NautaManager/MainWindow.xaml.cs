using NautaManager.ExtraItems.Abstractions;
using NautaManager.Handlers;
using NautaManager.Models;
using NautaManager.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NautaManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<UserSession> Sessions { get; set; }
        IUserManager UserManager { get; }
        public MainWindow(IUserManager manager)
        {
            InitializeComponent();
            UserManager = manager;
            Sessions = UserManager.GetAll();
            usersTable.ItemsSource = Sessions;
            LoadExtraFeatures();
        }

        private void LoadExtraFeatures()
        {
            foreach(var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(ExtraMenuFeature)) && !t.IsAbstract))
            {
                var element = ActivatorUtilities.CreateInstance(App.ServiceProvider, type) as ExtraMenuFeature;
                MenuItem item = element;
                item.Click += async (sender, args) =>
                {
                    lastError.Text = await element.ProcessClickAction(sender, args, usersTable.SelectedItem as UserSession);
                };
                menuExtraItems.Items.Add(item);
            }
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            var form = new CreateUser(UserManager, usersTable.SelectedItem as UserSession);
            if (form.ShowDialog() ?? false)
            {
                usersTable.Items.Refresh();
            }
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if(usersTable.SelectedItem != null)
            {
                var session = usersTable.SelectedItem as UserSession;
                var result = await UserManager.LoginUser(session);
                if (!result)
                    lastError.Text = result.Message;
                else
                    lastError.Text = $"{session.Username}: Conectado!";
            }
        }

        private async void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            var session = UserManager.ActiveSession;
            var result = await UserManager.CloseSession();
            if (!result)
                lastError.Text = result.Message;
            else
                lastError.Text = $"{session.Username}: Desconectado!";
        }        
        
        private async void btnSwap_Click(object sender, RoutedEventArgs e)
        {
            var session = usersTable.SelectedItem as UserSession;
            var r = await UserManager.SwapSessions(session);
            if (r)
                lastError.Text = $"Se ha cerrado la sesion anterior para abrir {session.Username}";
            else
                lastError.Text = r.Message;
        }        
        
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var model = usersTable.SelectedItem as UserSession;
            if (UserManager.RemoveSession(model))
            {
                Sessions.Remove(model);
                usersTable.Items.Refresh();
            }
        }
        private void addUserMenu_Click(object sender, RoutedEventArgs e)
        {
            var form = new CreateUser(UserManager);
            if (form.ShowDialog() ?? false)
            {
                Sessions.Add(form.NewSession);
                usersTable.Items.Refresh();
            }
        }

        private void saveMenu_Click(object sender, RoutedEventArgs e)
        {
            UserManager.Save();
            lastError.Text = "Informacion actualizada";
        }
    }
}
