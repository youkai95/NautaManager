using NautaManager.ExtraItems.Abstractions;
using NautaManager.Handlers;
using NautaManager.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NautaManager.ExtraItems
{
    class CheckTime : ExtraMenuFeature
    {
        IUserManager Manager;
        public CheckTime(IUserManager manager) : base("Actualizar tiempo")
        {
            Manager = manager;
        }

        public override async Task<string> ProcessClickAction(object sender, RoutedEventArgs args, UserSession selectedUser)
        {
            var r = await Manager.UpdateSessionTime(selectedUser);
            if (r != string.Empty)
                return r;
            return "Tiempo disponible actualizado!";
        }
    }
}
