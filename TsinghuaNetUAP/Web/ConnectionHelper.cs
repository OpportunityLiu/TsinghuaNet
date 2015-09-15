using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.Sockets;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace Web
{
    public static class ConnectionHelper
    {
        public static IAsyncAction CheckConnection(string hostName, string portName, int timeOutInMillisecond)
        {
            if(string.IsNullOrEmpty(hostName))
                throw new ArgumentNullException(nameof(hostName));
            if(string.IsNullOrEmpty(portName))
                throw new ArgumentNullException(nameof(portName));
            return Run(async token =>
            {
                using(var tcpClient = new StreamSocket())
                {
                    var connect = tcpClient.ConnectAsync(new Windows.Networking.HostName(hostName), portName, SocketProtectionLevel.PlainSocket);
                    token.Register(connect.Cancel);
                    var cancel = Task.Delay(timeOutInMillisecond).ContinueWith(task =>
                    {
                        if(connect.Status == AsyncStatus.Started)
                            connect.Cancel();
                    });
                    await connect;
                }
            });
        }

        public static IAsyncAction CheckConnection(string hostName, int timeOutInMillisecond)
        {
            return CheckConnection(hostName, "80", timeOutInMillisecond);
        }
    }
}
