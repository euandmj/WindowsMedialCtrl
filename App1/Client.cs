using App1.Events;
using System;
using System.Net.Sockets;
using System.Text;

namespace App1
{
    public class RemoteClient
    {
        public event EventHandler<SnackbarEventArgs> SnackEvent;

        public string Hostname { get; set; }
        public int Port { get; set; }

        public async void SendDWORD(string dword)
        {
            try
            {
                using var client = new TcpClient()
                {
                    ReceiveTimeout = 1000,
                    SendTimeout = 1000
                };
                
                await client.ConnectAsync(Hostname, Port);

                using var ns = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(dword);
                await ns.WriteAsync(data, 0, data.Length);
            }
            catch (InvalidOperationException)
            {
                SnackEvent?.Invoke(this, new SnackbarEventArgs("Unable to send request."));
            }
            catch (TimeoutException)
            {
                SnackEvent?.Invoke(this, new SnackbarEventArgs("Request Timed Out"));
            }
            catch (Exception ex)
            {
                SnackEvent?.Invoke(this, new SnackbarEventArgs(ex.Message, ex));
            }
        }
    }
}