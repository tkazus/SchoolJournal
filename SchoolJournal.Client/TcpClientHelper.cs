using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SchoolJournal.Client
{
    public static class TcpClientHelper
    {
        private static string _serverAddress = "localhost";
        private static int _port = 8888;

        public static string ServerAddress
        {
            get => _serverAddress;
            set => _serverAddress = value;
        }

        public static int Port
        {
            get => _port;
            set => _port = value;
        }

        public static async Task<string> SendRequestAsync(string request)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    await client.ConnectAsync(_serverAddress, _port);
                    using (var stream = client.GetStream())
                    {
                        byte[] data = Encoding.UTF8.GetBytes(request);
                        await stream.WriteAsync(data, 0, data.Length);

                        byte[] buffer = new byte[8192];
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        return Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    }
                }
            }
            catch (Exception ex)
            {
                return $"ERROR|{ex.Message}";
            }
        }

        public static string SendRequest(string request)
        {
            return SendRequestAsync(request).GetAwaiter().GetResult();
        }
    }
}