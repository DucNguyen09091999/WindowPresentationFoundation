using HerculesSimulation.Cores;
using HerculesSimulation.Models;
using HerculesSimulation.Services;
using System;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Windows.Input;
using System.Globalization;
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
        private string _sendText1;
        public string SendText1
        {
            get => _sendText1;
            set
            {
                // Gọi SetProperty (từ ViewModelBase)
                if (SetProperty(ref _sendText1, value))
                {
                    // Thông báo cho Command 1 cập nhật
                    ((RelayCommand)Send1Command).RaiseCanExecuteChanged();
                }
            }
        }
        private bool _isHex1;
        public bool IsHex1
        {
            get => _isHex1;
            set => SetProperty(ref _isHex1, value);
        }

        private string _sendText2;
        public string SendText2
        {
            get => _sendText2;
            set
            {
                if (SetProperty(ref _sendText2, value))
                    ((RelayCommand)Send2Command).RaiseCanExecuteChanged();
            }
        }
        private bool _isHex2;
        public bool IsHex2
        {
            get => _isHex2;
            set => SetProperty(ref _isHex2, value);
        }

        private string _sendText3;
        public string SendText3
        {
            get => _sendText3;
            set
            {
                if (SetProperty(ref _sendText3, value))
                    ((RelayCommand)Send3Command).RaiseCanExecuteChanged();
            }
        }
        private bool _isHex3;
        public bool IsHex3
        {
            get => _isHex3;
            set => SetProperty(ref _isHex3, value);
        }
        // === 2. KHAI BÁO CÁC COMMAND "SEND" ===
        public ICommand Send1Command { get; }
        public ICommand Send2Command { get; }
        public ICommand Send3Command { get; }

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
            Send1Command = new RelayCommand(
                p => ExecuteSend(SendText1, IsHex1), // Hàm thực thi
                p => CanExecuteSend(SendText1)       // Hàm kiểm tra
            );
            Send2Command = new RelayCommand(
                p => ExecuteSend(SendText2, IsHex2),
                p => CanExecuteSend(SendText2)
            );
            Send3Command = new RelayCommand(
                p => ExecuteSend(SendText3, IsHex3),
                p => CanExecuteSend(SendText3)
            );
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

        // === 4. HÀM XỬ LÝ SỰ KIỆN TỪ SERVICE (Cập nhật) ===

        // Khi kết nối (hoặc ngắt kết nối)
        // Chúng ta phải báo cho CẢ 3 NÚT SEND cập nhật
        private void UpdateAllCommandStates()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
                ((RelayCommand)PingCommand).RaiseCanExecuteChanged();
                ((RelayCommand)Send1Command).RaiseCanExecuteChanged();
                ((RelayCommand)Send2Command).RaiseCanExecuteChanged();
                ((RelayCommand)Send3Command).RaiseCanExecuteChanged();
            });
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
            UpdateAllCommandStates();
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
                UpdateAllCommandStates();
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

        // === 5. LOGIC CỦA SEND (Phần chính) ===

        // Logic "CÓ THỂ GỬI" (chung cho cả 3 nút)
        private bool CanExecuteSend(string text)
        {
            // Chỉ cho phép Send khi:
            return _isConnected &&         // 1. Đã kết nối
                   !string.IsNullOrEmpty(text); // 2. Ô text không rỗng
        }

        // Logic "THỰC HIỆN GỬI" (chung cho cả 3 nút)
        private async void ExecuteSend(string text, bool isHex)
        {
            if (!CanExecuteSend(text)) return;

            byte[] dataToSend;
            string logText; // Chuỗi sẽ hiển thị trên log

            try
            {
                // Chuyển đổi text/HEX sang byte[]
                (dataToSend, logText) = ConvertTextToBytes(text, isHex);
            }
            catch (Exception ex)
            {
                LogEntries.Add(new LogEntry($"Lỗi định dạng HEX: {ex.Message}", LogEntry.ColorError));
                return;
            }

            // Gửi dữ liệu qua Service
            try
            {
                await _tcpClientService.SendAsync(dataToSend);
                LogEntries.Add(new LogEntry($"[TX] -> {logText}", LogEntry.ColorTx));
            }
            catch (Exception ex)
            {
                // Lỗi khi gửi (Service sẽ tự gọi OnConnectionClosed)
                LogEntries.Add(new LogEntry($"Lỗi khi gửi: {ex.Message}", LogEntry.ColorError));
            }
        }

        // 6. HÀM HỖ TRỢ CHUYỂN ĐỔI HEX
        private (byte[] data, string logText) ConvertTextToBytes(string text, bool isHex)
        {
            if (isHex)
            {
                // Xóa khoảng trắng và ký tự xuống dòng
                string hex = text.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "");

                // Đảm bảo số ký tự là chẵn
                if (hex.Length % 2 != 0)
                {
                    throw new FormatException("Chuỗi HEX phải có số ký tự chẵn.");
                }

                // Chuyển đổi
                byte[] data = new byte[hex.Length / 2];
                for (int i = 0; i < data.Length; i++)
                {
                    string byteString = hex.Substring(i * 2, 2);
                    data[i] = byte.Parse(byteString, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }
                return (data, text); // Trả về byte[] và chuỗi HEX gốc để log
            }
            else
            {
                // Chuyển đổi chuỗi text (dùng UTF-8)
                byte[] data = Encoding.UTF8.GetBytes(text);
                return (data, text); // Trả về byte[] và chuỗi text
            }
        }
    }
}
