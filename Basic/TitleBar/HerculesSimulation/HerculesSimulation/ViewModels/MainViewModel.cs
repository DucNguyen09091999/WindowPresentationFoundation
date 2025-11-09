using HerculesSimulation.Cores;

namespace HerculesSimulation.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public TitleBarViewModel TitleBarVM { get;}
        public MainViewModel()
        {
            TitleBarVM = new TitleBarViewModel();
        }
    }
}
