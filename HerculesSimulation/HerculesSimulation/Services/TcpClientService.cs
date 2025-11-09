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

        public bool IsConnected => _tcpClient?.Connected ?? false;

        public event Action<byte[]> DataReceived;
        public event Action ConnectionClosed;

        // Hàm kết nối
        public async Task<bool> ConnectAsync(string ipAddress, int port)
        {
            if (IsConnected) return true;

            try
            {
                _tcpClient = new TcpClient();
                // Dùng ConnectAsync để không làm đơ UI
                await _tcpClient.ConnectAsync(ipAddress, port);
                _stream = _tcpClient.GetStream();

                // Tạo một token để có thể hủy tác vụ "lắng nghe"
                _cancellationTokenSource = new CancellationTokenSource();
                // Bắt đầu một Task chạy nền để lắng nghe dữ liệu
                Task.Run(() => ListenForData(_cancellationTokenSource.Token));

                return true;
            }
            catch (Exception)
            {
                // Nếu lỗi, dọn dẹp và trả về false
                Cleanup();
                return false;
            }
        }

        // Hàm ngắt kết nối
        public void Disconnect()
        {
            if (!IsConnected) return;
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
                    if (bytesRead == 0)
                    {
                        // Server đã đóng kết nối
                        break;
                    }

                    var receivedData = new byte[bytesRead];
                    Array.Copy(buffer, 0, receivedData, 0, bytesRead);

                    // Phát sự kiện "DataReceived"
                    DataReceived?.Invoke(receivedData);
                }
            }
            catch (OperationCanceledException)
            {
                // Người dùng nhấn Disconnect, đây là hành vi mong muốn
            }
            catch (Exception)
            {
                // Lỗi thực sự (ví dụ: rút dây mạng)
            }
            finally
            {
                // Dù lý do gì, khi thoát vòng lặp là mất kết nối
                Cleanup();
            }
        }

        // Hàm dọn dẹp tài nguyên
        private void Cleanup()
        {
            try
            {
                _cancellationTokenSource?.Cancel(); // Hủy Task lắng nghe
                _stream?.Close();
                _tcpClient?.Close();
            }
            catch (Exception) { /* Bỏ qua lỗi khi dọn dẹp */ }
            finally
            {
                _stream?.Dispose();
                _tcpClient?.Dispose();
                _stream = null;
                _tcpClient = null;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;

                // Phát sự kiện "ConnectionClosed"
                ConnectionClosed?.Invoke();
            }
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}
