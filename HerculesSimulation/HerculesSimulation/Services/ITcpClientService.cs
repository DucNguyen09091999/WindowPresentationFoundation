using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HerculesSimulation.Services
{
    public interface ITcpClientService
    {
        // Trạng thái kết nối
        bool IsConnected { get; }

        // Sự kiện (event) khi nhận được dữ liệu
        event Action<byte[]> DataReceived;

        // Sự kiện khi mất kết nối (do server đóng hoặc lỗi)
        event Action ConnectionClosed;

        // Hàm kết nối
        Task<bool> ConnectAsync(string ipAddress, int port);

        // Hàm ngắt kết nối
        void Disconnect();

        // Hàm gửi dữ liệu
        Task SendAsync(byte[] data);
    }
}
