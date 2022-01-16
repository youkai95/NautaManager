using NautaManager.Handlers;
using NautaManager.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NautaManager
{
    /// <summary>
    /// Interaction logic for CreateUser.xaml
    /// </summary>
    public partial class CreateUser : Window
    {
        IUserManager Manager;
        public UserSession NewSession { get; set; }
        public CreateUser(IUserManager manager, UserSession model = null)
        {
            InitializeComponent();
            Manager = manager;
            if(model != null)
            {
                username.Text = model.Username;
                NewSession = model;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (NewSession == null)
            {
                if (username.Text != null && username.Text != string.Empty &&
                    password.Password != null && password.Password != string.Empty)
                {
                    NewSession = await Manager.CreateSession(username.Text, password.Password);
                    DialogResult = true;
                    Close();
                }
            }
            else
            {
                string oldName = NewSession.Username;
                NewSession.Username = username.Text;
                NewSession.Password = password.Password;
                DialogResult = Manager.UpdateSession(oldName, NewSession.Username);
                Close();
            }
        }
    }
}
