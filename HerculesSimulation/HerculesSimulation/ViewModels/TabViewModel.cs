using HerculesSimulation.Cores;
using System;

namespace HerculesSimulation.ViewModels
{
    public class TabViewModel : ViewModelBase
    {
        public string Header { get; }

        // Luu kieu cua ViewModel thay vi mot the hien (instance)
        private readonly Type _contentViewModelType;

        // Day la the hien (instance) thuc su, no se la null luc dau
        private ViewModelBase _contentViewModel;

        // ap dung lazy loading
        public ViewModelBase ContentViewModel
        {
            get
            {
                // trong truong hop content chua duoc tao thi moi tao no
                if (_contentViewModel == null)
                {
                    // dung activator de tao mot the hien (instance) tu type
                    _contentViewModel = (ViewModelBase)Activator.CreateInstance(_contentViewModelType);
                }

                // tra ve the hien (instance) da duoc tao hoac tao moi
                return _contentViewModel;
            }
        }

        // Ham hoi tao viewModel
        public TabViewModel(string header, Type contentViewModelType)
        {
            Header = header;
            _contentViewModelType = contentViewModelType;
            _contentViewModel = null; // Important: bat buoc bat dau bang null
        }
    }
}
