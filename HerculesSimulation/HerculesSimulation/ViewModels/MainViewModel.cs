using HerculesSimulation.Cores;

namespace HerculesSimulation.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public TitleBarViewModel TitleBarVM { get;}
        public TabControlViewModel TabControlVM { get;}
        public MainViewModel()
        {
            TitleBarVM = new TitleBarViewModel();
            TabControlVM = new TabControlViewModel();
        }
    }
}
