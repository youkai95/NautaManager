using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;

namespace NautaManager.Models
{
    public class AccountStatus : INotifyPropertyChanged
    {
        TimeSpan _remaining;
        string _credit;
        public TimeSpan RTime { 
            get => _remaining;
            set
            {
                _remaining = value;
                RaisePropertyChanged("RemainingTime");
            }
        }
        [JsonIgnore]
        public string RemainingTime
        {
            get => string.Format("{0:D2}:{1:D2}:{2:D2}", (int)Math.Floor(_remaining.TotalHours), _remaining.Minutes, _remaining.Seconds);
        }
        public string AvailableCredit
        {
            get => _credit;
            set
            {
                _credit = value;
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
    }
}
