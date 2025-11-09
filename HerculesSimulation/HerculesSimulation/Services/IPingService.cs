using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace HerculesSimulation.Services
{
    public interface IPingService
    {
        // dinh nghia mot ham ping bat dong bo
        Task<PingReply> PingAsync(string ipAddress, int timeout);
    }
}
