using HerculesSimulation.Cores;

namespace HerculesSimulation.ViewModels
{
    public class TabViewModel : ViewModelBase
    {
        public string Header { get; set; }
        public ViewModelBase ContentViewModel { get; set; }
    }
}
