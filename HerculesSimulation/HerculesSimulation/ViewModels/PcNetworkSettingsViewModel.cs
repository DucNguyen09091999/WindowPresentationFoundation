using HerculesSimulation.Cores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HerculesSimulation.ViewModels
{
    public class PcNetworkSettingsViewModel : ViewModelBase
    {
        private string _pcIP;
        public string PcIP
        {
            get => _pcIP;
            set => SetProperty(ref _pcIP, value);
        }

        private string _pcMask;
        public string PcMask
        {
            get => _pcMask;
            set => SetProperty(ref _pcMask, value);
        }

        private string _pcGateway;
        public string PcGateway
        {
            get => _pcGateway;
            set => SetProperty(ref _pcGateway, value);
        }

        public PcNetworkSettingsViewModel()
        {
            LoadPcNetworkSettings();
        }

        private void LoadPcNetworkSettings()
        {
            PcIP = "Not found";
            PcMask = "Not found";
            PcGateway = "Not found";

            try
            {
                var activeInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(ni =>
                        ni.OperationalStatus == OperationalStatus.Up &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                        ni.GetIPProperties().GatewayAddresses.Any());

                if (activeInterface == null)
                {
                    activeInterface = NetworkInterface.GetAllNetworkInterfaces()
                        .FirstOrDefault(ni =>
                            ni.OperationalStatus == OperationalStatus.Up &&
                            (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                             ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211));
                }

                if (activeInterface != null)
                {
                    var ipProps = activeInterface.GetIPProperties();
                    var ipv4Info = ipProps.UnicastAddresses
                        .FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork);

                    if (ipv4Info != null)
                    {
                        PcIP = ipv4Info.Address.ToString();
                        PcMask = ipv4Info.IPv4Mask.ToString();
                    }

                    var gatewayInfo = ipProps.GatewayAddresses.FirstOrDefault();
                    if (gatewayInfo != null)
                    {
                        PcGateway = gatewayInfo.Address.ToString();
                    }
                    else if (PcGateway == "Not found")
                    {
                        PcGateway = "0.0.0.0";
                    }
                }
            }
            catch
            {
                PcIP = "Error";
                PcMask = "Error";
                PcGateway = "Error";
            }
        }
    }
}
