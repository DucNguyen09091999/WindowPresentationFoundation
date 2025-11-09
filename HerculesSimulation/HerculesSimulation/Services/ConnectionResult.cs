using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HerculesSimulation.Services
{
    public enum ConnectionResult
    {
        Success,              // Thành công
        ConnectionRefused,    // LỖI: Máy chủ không bật / Port bị từ chối
        HostNotFound,         // LỖI: Không tìm thấy IP / DNS
        NetworkUnreachable,   // LỖI: Không có đường mạng
        Timeout,              // LỖI: Hết thời gian chờ
        UnknownError          // LỖI: Chung chung
    }
}
