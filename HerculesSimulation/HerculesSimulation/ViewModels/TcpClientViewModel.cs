using HerculesSimulation.Cores;
using HerculesSimulation.Models;
using HerculesSimulation.Services;
using System;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Windows.Input;

namespace HerculesSimulation.ViewModels
{
    public class TcpClientViewModel : ViewModelBase
    {
        public ObservableCollection<LogEntry> LogEntries { get; }
        //Services va flag disable/enable
        private readonly IPingService _pingService;
        private bool _isConnected = false; // <-- CỜ TRẠNG THÁI MỚI

        private readonly ITcpClientService _tcpClientService; // <-- SERVICE MỚI
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
                    ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
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
                    ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
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
            _tcpClientService = new TcpClientService();

            // Dang ky nhan su kien tu services
            _tcpClientService.DataReceived += OnDataReceived;
            _tcpClientService.ConnectionClosed += OnConnectionClosed;

            PingCommand = new RelayCommand(ExecutePing, CanExecutePing);
            ConnectCommand = new RelayCommand(ExecuteConnect, CanExecuteConnect);
            Send1Command = new RelayCommand(p => ExecuteSend(SendText1, IsHex1), CanExecuteSend);
            // ... (AuthorizeCommand, ReceiveTestDataCommand)
        }

        private bool CanExecutePing(object obj)
        {
            // chi cho phep ping khi khong dang ping va ModuleIp va Port khong rong
            return !_isPinging && !_isConnected && !string.IsNullOrEmpty(ModuleIp) && !string.IsNullOrEmpty(Port);
        }

        private async void ExecutePing(object obj)
        {
            _isPinging = true;
            ((RelayCommand)PingCommand).RaiseCanExecuteChanged(); // vo hieu hoa nut ping

            LogEntries.Add(new LogEntry($"Sending ICMP ECHO REQUEST to {ModuleIp}", LogEntry.ColorStatus));

            // goi services de ping
            PingReply reply = await _pingService.PingAsync(ModuleIp, 2000); // 2s timeout

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
            ((RelayCommand)PingCommand).RaiseCanExecuteChanged(); // kich hoat lai nut ping
        }

        private async void ExecuteConnect(object obj)
        {
            // Logic 1: Nếu đang kết nối -> Ngắt kết nối (Giữ nguyên)
            if (_isConnected)
            {
                _tcpClientService.Disconnect();
                return; // Hàm OnConnectionClosed sẽ xử lý phần còn lại
            }

            // Logic 2: Thử kết nối
            LogEntries.Add(new LogEntry($"Connecting to {ModuleIp}:{Port} ...", LogEntry.ColorStatus));

            // Vô hiệu hóa nút
            _isPinging = true;
            ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
            ((RelayCommand)PingCommand).RaiseCanExecuteChanged();

            ConnectionResult result = ConnectionResult.UnknownError;

            try
            {
                if (!int.TryParse(Port, out int portNumber))
                {
                    LogEntries.Add(new LogEntry("Invalid Port number.", LogEntry.ColorError));
                }
                else
                {
                    // GỌI SERVICE ĐÃ CẬP NHẬT
                    result = await _tcpClientService.ConnectAsync(ModuleIp, portNumber);
                }
            }
            catch (Exception ex)
            {
                LogEntries.Add(new LogEntry($"Connection error: {ex.Message}", LogEntry.ColorError));
                result = ConnectionResult.UnknownError;
            }

            // XỬ LÝ KẾT QUẢ TỪ SERVICE
            switch (result)
            {
                case ConnectionResult.Success:
                    _isConnected = true;
                    ConnectButtonText = "Disconnect";
                    LogEntries.Add(new LogEntry($"Connected to {ModuleIp}:{Port}", LogEntry.ColorStatus));
                    break;

                // 1. ĐÂY LÀ YÊU CẦU CỦA BẠN:
                // (Server chưa bật HOẶC Port đang bận/bị chặn)
                case ConnectionResult.ConnectionRefused:
                    _isConnected = false;
                    ConnectButtonText = "Connect";
                    LogEntries.Add(new LogEntry("Error: Connection Refused", LogEntry.ColorError));
                    LogEntries.Add(new LogEntry("Root cause: Server is down, wrong port id, blocked by firmware)", LogEntry.ColorError));
                    break;

                case ConnectionResult.HostNotFound:
                    _isConnected = false;
                    ConnectButtonText = "Connect";
                    LogEntries.Add(new LogEntry("Error: Host Not Found", LogEntry.ColorError));
                    break;

                case ConnectionResult.NetworkUnreachable:
                    _isConnected = false;
                    ConnectButtonText = "Connect";
                    LogEntries.Add(new LogEntry("Error: Network Unreachable", LogEntry.ColorError));
                    break;

                default: // UnknownError hoặc Timeout
                    _isConnected = false;
                    ConnectButtonText = "Connect";
                    LogEntries.Add(new LogEntry("Error: Unknown Error / Timeout", LogEntry.ColorError));
                    break;
            }

            // Kích hoạt lại các nút
            _isPinging = false;
            ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
            ((RelayCommand)PingCommand).RaiseCanExecuteChanged();
            ((RelayCommand)Send1Command).RaiseCanExecuteChanged();
        }

        private bool CanExecuteConnect(object obj)
        {
            return !string.IsNullOrEmpty(ModuleIp) &&
                   !string.IsNullOrEmpty(Port);
        }


        // === cac ham xu ly su kien tu services ===

        //Duoc goi khi service mat ket noi (do server dong hoac loi)
        private void OnConnectionClosed(bool wasIntentional)
        {
            _isConnected = false;

            App.Current.Dispatcher.Invoke(() =>
            {
                ConnectButtonText = "Connect";

                if (wasIntentional)
                {
            
                    LogEntries.Add(new LogEntry("Connection closed.", LogEntry.ColorStatus));
                }
                else
                {
                    // Server sập, mất mạng, v.v...
                    LogEntries.Add(new LogEntry("LỖI: Máy chủ đã ngắt kết nối! (Server dropped connection)", LogEntry.ColorError));
                }

                // Báo cho các nút cập nhật lại trạng thái
                ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
                ((RelayCommand)PingCommand).RaiseCanExecuteChanged();
                ((RelayCommand)Send1Command).RaiseCanExecuteChanged();
            });
        }

        // Duoc goi khi services nhan duoc du lieu
        private void OnDataReceived(byte[] data)
        {
            string receivedText = Encoding.ASCII.GetString(data); //Gia su la ASCII

            // cap nhat UI (phai dung Dispatcher)
            App.Current.Dispatcher.Invoke(() =>
            {
                LogEntries.Add(new LogEntry($"[RX] <- {receivedText}", LogEntry.ColorRx));
            });
        }

        // === logic cua send ===
        private bool CanExecuteSend(object obj)
        {
            //  chi cho send khi da ket noi
            return _isConnected &&
                   !string.IsNullOrEmpty(SendText1); 
        }

        private async void ExecuteSend(string text, bool isHex)
        {
            if (!CanExecuteSend(null)) return;

            // TODO: Thêm logic chuyển đổi HEX
            byte[] dataToSend = Encoding.ASCII.GetBytes(text);

            try
            {
                await _tcpClientService.SendAsync(dataToSend);
                LogEntries.Add(new LogEntry($"[TX] -> {text}", LogEntry.ColorTx));

                // (Tùy chọn: Xóa text sau khi gửi)
                // SendText1 = string.Empty; 
                // OnPropertyChanged(nameof(SendText1));
            }
            catch (Exception ex)
            {
                LogEntries.Add(new LogEntry($"Send error: {ex.Message}", LogEntry.ColorError));
                // Service sẽ tự động gọi Disconnect và kích hoạt OnConnectionClosed
            }
        }
    }
}
