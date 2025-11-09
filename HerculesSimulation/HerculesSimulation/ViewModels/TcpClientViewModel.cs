using HerculesSimulation.Cores;
using HerculesSimulation.Models;
using HerculesSimulation.Services;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;
using System.Windows.Input;

namespace HerculesSimulation.ViewModels
{
    public class TcpClientViewModel : ViewModelBase
    {
        public ObservableCollection<LogEntry> LogEntries { get; }
        //Services va flag disable/enable
        private readonly IPingService _pingService;
        private bool _isPinging = false; // flag de disable nut ping
        // === Send ===
        public string SendText1 { get; set; }
        public bool IsHex1 { get; set; }
        public ICommand Send1Command { get; }

        // === TCP Settings ===
        private string _moduleIp;
        public string ModuleIp
        {
            get => _moduleIp;
            set
            {
                // dung SetProperty de thong bao cho view
                if (SetProperty(ref _moduleIp, value))
                {
                    // khi Ip thay doi, thong bao cho PingCommand kiem tra lai
                    ((RelayCommand)PingCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private string _port;
        public string Port
        {
            get => _port;
            set
            {
                if (SetProperty(ref _port, value))
                {
                    // khi Port thay doi, thong bao cho PingCommand kiem tra lai
                    ((RelayCommand)PingCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private string _connectButtonText = "Connect";
        public string ConnectButtonText
        {
            get => _connectButtonText;
            set => SetProperty(ref _connectButtonText, value);
        }

        // === TEA ===
        public string TeaKey1 { get; set; } = "01020304";
        public string TeaKey2 { get; set; } = "05060708";
        public string TeaKey3 { get; set; } = "090A0B0C";
        public string TeaKey4 { get; set; } = "0D0E0F10";
        public string AuthCode { get; set; }

        // === PortStore ===
        public bool IsNvtDisabled { get; set; }
        public bool IsRedirectToUdpEnabled { get; set; }

        // === Commands ===
        public ICommand PingCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand AuthorizeCommand { get; }
        public ICommand ReceiveTestDataCommand { get; }
        public string Version { get; set; } = "Version 3.2.8";
        public TcpClientViewModel()
        {
            LogEntries = new ObservableCollection<LogEntry>();
            _pingService = new PingService();

            PingCommand = new RelayCommand(ExecutePing, CanExecutePing);
            ConnectCommand = new RelayCommand(ExecuteConnect, CanExecuteConnect);
            Send1Command = new RelayCommand(p => ExecuteSend(SendText1, IsHex1));
            // ... (AuthorizeCommand, ReceiveTestDataCommand)
        }

        private bool CanExecutePing(object obj)
        {
            // chi cho phep ping khi khong dang ping va ModuleIp va Port khong rong
            return !_isPinging && !string.IsNullOrEmpty(ModuleIp) && !string.IsNullOrEmpty(Port);
        }

        private async void ExecutePing(object obj)
        {
            _isPinging = true;
            ((RelayCommand)PingCommand).RaiseCanExecuteChanged(); // vo hieu hoa nut ping

            LogEntries.Add(new LogEntry($"Sending ICMP ECHO REQUEST to {ModuleIp}", LogEntry.ColorStatus));

            // goi services de ping
            PingReply reply = await _pingService.PingAsync(ModuleIp, 2000); // 2 giây timeout

            // xu ly ket qua
            if (reply != null && reply.Status == IPStatus.Success)
            {
                LogEntries.Add(new LogEntry($"Received ICMP ECHO REPLY from {ModuleIp} (Time: {reply.RoundtripTime}ms)", LogEntry.ColorStatus));
            }
            else if (reply != null)
            {
                LogEntries.Add(new LogEntry($"Ping failed: {reply.Status}", LogEntry.ColorError));
            }
            else
            {
                LogEntries.Add(new LogEntry($"Ping failed: Invalid host or DNS error.", LogEntry.ColorError));
            }

            _isPinging = false;
            ((RelayCommand)PingCommand).RaiseCanExecuteChanged(); // Kích hoạt lại nút
        }

        private void ExecuteConnect(object obj)
        {
            // TODO: them logic ket noi/ngat ket noi
            if (ConnectButtonText == "Connect")
            {
                LogEntries.Add(new LogEntry($"Connecting to {ModuleIp}:{Port} ...", LogEntry.ColorStatus));
                // log sau khi ket noi thanh cong + broadcast
                // LogEntries.Add(new LogEntry($"Connected to {ModuleIp}:{Port}", LogEntry.ColorStatus));
                ConnectButtonText = "Disconnect";
            }
            else
            {
                // TODO: Them logic ngat ket noi
                LogEntries.Add(new LogEntry("Connection closed", LogEntry.ColorError));
                ConnectButtonText = "Connect";
            }
        }
        private bool CanExecuteConnect(object obj)
        {
            return !string.IsNullOrEmpty(ModuleIp) &&
                   !string.IsNullOrEmpty(Port);
        }
        private void ExecuteSend(string text, bool isHex)
        {
            // TODO: Them logic gui du lieu
            if (!string.IsNullOrEmpty(text))
            {
                LogEntries.Add(new LogEntry($"{text}", LogEntry.ColorTx)); // Giả lập Echo Server
                LogEntries.Add(new LogEntry($"{text}", LogEntry.ColorRx)); // Giả lập Echo Server
            }
        }
    }
}
