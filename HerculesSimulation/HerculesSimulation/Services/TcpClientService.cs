using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HerculesSimulation.Services
{
    public class TcpClientService : ITcpClientService, IDisposable
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isIntentionalDisconnect = false;
        private bool _isCleaningUp = false;
        public bool IsConnected => _tcpClient?.Connected ?? false;

        public event Action<byte[]> DataReceived;
        public event Action<bool> ConnectionClosed;
        // Hàm kết nối
        public async Task<ConnectionResult> ConnectAsync(string ipAddress, int port)
        {
            if (IsConnected) return ConnectionResult.Success;
            _isCleaningUp = false;
            _isIntentionalDisconnect = false;
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(ipAddress, port);
                _stream = _tcpClient.GetStream();

                _cancellationTokenSource = new CancellationTokenSource();
                Task.Run(() => ListenForData(_cancellationTokenSource.Token));

                return ConnectionResult.Success; // Kết nối thành công
            }
            // PHẦN LOGIC BẮT LỖI QUAN TRỌNG
            catch (SocketException ex)
            {
                Cleanup();
                // Phân tích mã lỗi socket
                switch (ex.SocketErrorCode)
                {
                    // Lỗi: Máy chủ có đó, nhưng port không nghe
                    // (Server chưa bật, Port bị firewall chặn, Port đang bận)
                    case SocketError.ConnectionRefused:
                        return ConnectionResult.ConnectionRefused;

                    // Lỗi: Không tìm thấy IP hoặc tên DNS
                    case SocketError.HostNotFound:
                        return ConnectionResult.HostNotFound;

                    // Lỗi: Không có đường mạng (ví dụ: rút dây LAN)
                    case SocketError.NetworkUnreachable:
                        return ConnectionResult.NetworkUnreachable;

                    // Lỗi: Hết thời gian chờ
                    case SocketError.TimedOut:
                        return ConnectionResult.Timeout;

                    default:
                        return ConnectionResult.UnknownError;
                }
            }
            catch (Exception)
            {
                // Lỗi chung (ví dụ: IP sai định dạng)
                Cleanup();
                return ConnectionResult.UnknownError;
            }
        }

        // Hàm ngắt kết nối
        public void Disconnect()
        {
            if (!IsConnected) return;

            // ĐÁNH DẤU LÀ "CHỦ ĐỘNG"
            _isIntentionalDisconnect = true;
            Cleanup();
        }

        // Hàm gửi dữ liệu
        public async Task SendAsync(byte[] data)
        {
            if (!IsConnected || _stream == null || !_stream.CanWrite)
            {
                throw new InvalidOperationException("Not connected.");
            }

            try
            {
                await _stream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception)
            {
                // Nếu lỗi khi gửi (ví dụ: mất mạng), tự ngắt kết nối
                Cleanup();
            }
        }

        // Hàm chạy nền để lắng nghe
        private async void ListenForData(CancellationToken token)
        {
            var buffer = new byte[4096];
            try
            {
                while (IsConnected && !token.IsCancellationRequested)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, token);

                    // KHI SERVER SẬP (hoặc đóng kết nối)
                    // ReadAsync sẽ trả về 0
                    if (bytesRead == 0)
                    {
                        break; // Thoát vòng lặp
                    }
                    // ... (phần DataReceived?.Invoke...)
                }
            }
            catch (OperationCanceledException) { /* Bỏ qua, đây là Disconnect chủ động */ }
            catch (Exception)
            {
                // Bất kỳ lỗi nào khác (rút dây mạng, server crash)
                // cũng sẽ thoát vòng lặp.
            }
            finally
            {
                // KHI THOÁT VÒNG LẶP (vì server sập hoặc lỗi)
                // _isIntentionalDisconnect sẽ là 'false'
                Cleanup();
            }
        }

        // Hàm dọn dẹp tài nguyên
        private void Cleanup()
        {
            if (_isCleaningUp) return;
            _isCleaningUp = true;

            // SỬA LỖI: Ghi lại trạng thái TRƯỚC KHI dọn dẹp
            bool wasConnected = this.IsConnected;

            try
            {
                _cancellationTokenSource?.Cancel();
                _stream?.Close();
                _tcpClient?.Close();
            }
            catch (Exception) { /* Bỏ qua */ }
            finally
            {
                _stream?.Dispose();
                _tcpClient?.Dispose();
                _stream = null;
                _tcpClient = null;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;

                bool wasIntentional = _isIntentionalDisconnect;
                _isIntentionalDisconnect = false;

                // (Không reset _isCleaningUp ở đây)

                // SỬA LỖI:
                // CHỈ gửi thông báo "ConnectionClosed"
                // nếu chúng ta THỰC SỰ ĐÃ KẾT NỐI trước đó.
                if (wasConnected)
                {
                    ConnectionClosed?.Invoke(wasIntentional);
                }
            }
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}
