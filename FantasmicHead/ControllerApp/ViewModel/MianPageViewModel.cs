using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace ControllerApp.ViewModel
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        //TODO: ReactivePropertyにするか
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainPageViewModel()
        {
            StatusMessage = "Initializing...";
            MainMessage = "Message will appear here.";
        }

        string _MainMessage;
        public string MainMessage
        {
            get
            {
                return _MainMessage;
            }
            set
            {
                _MainMessage = value;
                NotifyPropertyChanged("MainMessage");
            }
        }

        string _StatusMessage;
        public string StatusMessage
        {
            get
            {
                return _StatusMessage;
            }
            set
            {
                _StatusMessage = value;
                NotifyPropertyChanged("StatusMessage");
            }
        }

        public ObservableCollection<DeviceInformation> DeviceInfoCollection { get; set; }
    }
}
