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
    class CheckCredit : ExtraMenuFeature
    {
        IUserManager Manager;
        public CheckCredit(IUserManager manager) : base("Actualizar credito")
        {
            Manager = manager;
        }

        public override async Task<string> ProcessClickAction(object sender, RoutedEventArgs args, UserSession selectedUser)
        {
            var r = await Manager.UpdateSessionCredit(selectedUser);
            if (r != string.Empty)
                return r;
            return "Credito actualizado!";
        }
    }
}
