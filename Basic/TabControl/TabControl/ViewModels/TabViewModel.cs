using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabControl.ViewModels
{
    public class TabViewModel : ViewModelBase
    {
        public string Header { get; set; }
        public ViewModelBase ContentViewModel { get; set; }
    }
}
