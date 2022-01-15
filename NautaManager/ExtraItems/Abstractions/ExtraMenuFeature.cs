using NautaManager.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NautaManager.ExtraItems.Abstractions
{
    public abstract class ExtraMenuFeature
    {
        protected string MenuHeader { get; }
        protected Dictionary<string, object> ExtraMenuItemParameters { get; }
        public ExtraMenuFeature(string title)
        {
            MenuHeader = title;
            ExtraMenuItemParameters = new Dictionary<string, object>();
        }

        public static implicit operator MenuItem(ExtraMenuFeature self)
        {
            MenuItem item = new MenuItem() { Header = self.MenuHeader };
            foreach(var param in self.ExtraMenuItemParameters)
            {
                var property = typeof(MenuItem).GetProperty(param.Key);
                if(property != null)
                    property.SetValue(item, param.Value);
            }
            return item;
        }

        public abstract Task<string> ProcessClickAction(object sender, RoutedEventArgs args, UserSession selectedUser);
    }
}
