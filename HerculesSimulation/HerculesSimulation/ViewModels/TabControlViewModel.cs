using HerculesSimulation.Cores;
using System.Collections.ObjectModel;

namespace HerculesSimulation.ViewModels
{
    public class TabControlViewModel : ViewModelBase
    {
        private TabViewModel _selectedTab;
        public ObservableCollection<TabViewModel> Tabs { get;}

        public TabViewModel SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                OnPropertyChanged();
            }
        }

        public TabControlViewModel()
        {
            Tabs = new ObservableCollection<TabViewModel>();

            Tabs.Add(new TabViewModel("UPD Setup", typeof(UdpSetupViewModel)));
            Tabs.Add(new TabViewModel("Serial", typeof(SerialViewModel)));
            Tabs.Add(new TabViewModel("TCP Client", typeof(TcpClientViewModel)));
            // chon tab dau tien
            SelectedTab = Tabs[0];
        }

    }
}
