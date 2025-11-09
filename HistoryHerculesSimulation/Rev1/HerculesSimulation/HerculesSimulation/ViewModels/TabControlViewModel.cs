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

            // Toàn bộ logic tạo tab được chuyển về đây
            Tabs.Add(new TabViewModel
            {
                Header = "UDP Setup",
                ContentViewModel = new UdpSetupViewModel()
            });

            Tabs.Add(new TabViewModel
            {
                Header = "Serial",
                ContentViewModel = new SerialViewModel()
            });
            Tabs.Add(new TabViewModel
            {
                Header = "TCP Client",
                ContentViewModel = new TcpClientViewModel()
            });

            // Chọn tab đầu tiên
            SelectedTab = Tabs[0];
        }

    }
}
