using HerculesSimulation.Cores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace HerculesSimulation.ViewModels
{
    public class UdpSetupViewModel : ViewModelBase
    {
        public string UdpWelcomeMessage { get; } = " UDP tab";
        public string DevicesFoundText { get; } = "0 devices were found:";
        public string DeviceType { get; } = "Unspecified device";
        public string FirmwareVersion { get; } = "Unknown";

        public PcNetworkSettingsViewModel NetworkSettingsVM { get; }

        public UdpSetupViewModel()
        {
            NetworkSettingsVM = new PcNetworkSettingsViewModel();
        }

    }
}
