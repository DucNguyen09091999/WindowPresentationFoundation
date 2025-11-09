using HerculesSimulation.Cores;
using System.Windows;
using System.Windows.Input;

namespace HerculesSimulation.ViewModels
{
    public class TitleBarViewModel : ViewModelBase
    {
        private WindowState _windowState;
        public WindowState WindowState
        {
            get => _windowState;
            set
            {
                _windowState = value;
                OnPropertyChanged();
            }
        }

        public ICommand MinimizeCommand { get;}
        public ICommand MaximizeCommand { get;}
        public ICommand CloseCommand { get;}

        public TitleBarViewModel()
        {
            MinimizeCommand = new RelayCommand(MinimizeWindow);
            MaximizeCommand = new RelayCommand(MaximizeWindow);
            CloseCommand = new RelayCommand(CloseWindow);
            WindowState = WindowState.Normal;

        }

        private void MinimizeWindow(object parameter)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow(object parameter)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void CloseWindow(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }
    }
}
