using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Server
{
    class Server
    {
        protected const int KEYEVENTF_EXTENDEDKEY = 1;
        protected const int KEYEVENTF_KEYUP       = 2;
        protected const int VK_MEDIA_NEXT_TRACK   = 0xB0;
        protected const int VK_MEDIA_PLAY_PAUSE   = 0xB3;
        protected const int VK_MEDIA_PREV_TRACK   = 0xB1;
        protected const int VK_VOLUME_DOWN        = 0xAE;
        protected const int VK_VOLUME_UP          = 0xAF;
        protected const int VK_VOLUME_MUTE        = 0xAD;

        protected TcpListener listener;

        public Server()
        {

        }

        public void RunServer()
        {
            listener = new TcpListener(localaddr: IPAddress.Any, port: 54001);
            listener.Start();
            Console.WriteLine("listening...");
            while (true)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();

                    new Thread(() => HandleRequest(client)).Start();

                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }

            }
        }

        protected void HandleRequest(TcpClient client)
        {
            var ns = client.GetStream();

            try
            {
                byte[] data = new byte[1024];
                string recv;

                int bytes = ns.Read(data, 0, data.Length);

                recv = Encoding.UTF8.GetString(data, 0, bytes);

                switch (recv.TrimEnd())
                {
                    case "VK_MEDIA_PLAY_PAUSE":
                        keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                        break;
                    case "VK_MEDIA_NEXT_TRACK":
                        keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                        break;
                    case "VK_MEDIA_PREV_TRACK":
                        keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                        break;
                    case "VK_VOLUME_DOWN":
                        keybd_event(VK_VOLUME_DOWN, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                        break;
                    case "VK_VOLUME_UP":
                        keybd_event(VK_VOLUME_UP, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                        break;
                    case "VK_VOLUME_MUTE":
                        keybd_event(VK_VOLUME_MUTE, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                        break;
                    case "DEVICE_SHUTDOWN":
                        Shutdown();
                        break;
                    case "DEVICE_RESTART":
                        Restart();
                        break;
                    case "DEVICE_LOCK":
                        LockWorkStation();
                        break;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.GetType()}: {ex.Message}");
            }
        }

        protected void Shutdown()
        {
            Process.Start(new ProcessStartInfo("shutdown", "/s /t 5")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }

        protected void Restart()
        {
            Process.Start(new ProcessStartInfo("shutdown", "-r /t 5")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }

        [DllImport("user32.dll", SetLastError = true)]
        protected static extern void keybd_event(byte virtualkey, byte scancode, uint flags, IntPtr extrainfo);

        [DllImport("user32.dll")]
        protected static extern void LockWorkStation();

    }
}
