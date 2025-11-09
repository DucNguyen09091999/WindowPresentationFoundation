using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabControl.ViewModels
{
    public class SettingViewModel : ViewModelBase
    {
        private bool _enableFeature;
        public bool EnableFeature
        {
            get => _enableFeature;
            set
            {
                _enableFeature = value;
                OnPropertyChanged();
            }
        }

        public SettingViewModel()
        {
            EnableFeature = true;
        }
    }
}
