using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabControl.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private TabViewModel _selectedTab;

        // Dùng ObservableCollection để khi thêm/xóa tab, View tự động cập nhật
        public ObservableCollection<TabViewModel> Tabs { get; }

        public TabViewModel SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            Tabs = new ObservableCollection<TabViewModel>();

            // Tạo các tab mặc định khi khởi động
            Tabs.Add(new TabViewModel
            {
                Header = "Trang chủ",
                ContentViewModel = new HomeViewModel()
            });

            Tabs.Add(new TabViewModel
            {
                Header = "Cài đặt",
                ContentViewModel = new SettingViewModel()
            });

            // Chọn tab đầu tiên
            SelectedTab = Tabs[0];
        }
    }
}
