using HerculesSimulation.ViewModels;
using System.Windows;

namespace HerculesSimulation
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainViewModel vm = new MainViewModel();
            DataContext = vm;
            // Register Event when window state changed
            this.StateChanged += Window_StateChanged;
        }

        private void Window_StateChanged(object sender, System.EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                // If window state is Maximize, set limitation/bound
                // MaxWidth/Height is equal to "Work Area"
                this.MaxWidth = SystemParameters.WorkArea.Width;
                this.MaxHeight = SystemParameters.WorkArea.Height;
            }
            else
            {
                // if window state is normal, remove limitation/bound
                this.MaxWidth = double.PositiveInfinity;
                this.MaxHeight = double.PositiveInfinity;
            }
        }
    }
}
