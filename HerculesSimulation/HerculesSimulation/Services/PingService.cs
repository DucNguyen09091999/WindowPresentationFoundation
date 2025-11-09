using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace HerculesSimulation.Services
{
    public class PingService : IPingService
    {
        public async Task<PingReply> PingAsync(string ipAddress, int timeout)
        {
            // dung using de no tu dong don dep sau khi lam 
            using (Ping pinger = new Ping())
            {
                try
                {
                    // chay ping bat dong bo
                    return await pinger.SendPingAsync(ipAddress, timeout);
                }
                catch (PingException)
                {
                    // TODO: xu ly cac loi nhu khong tim thay host, port dang ban, ...
                    return null;
                }
            }
        }
    }
}
