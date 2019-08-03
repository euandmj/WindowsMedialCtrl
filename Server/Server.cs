using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;

namespace Server
{
    class Server
    {
        public const int KEYEVENTF_EXTENDEDKEY = 1;
        public const int KEYEVENTF_KEYUP = 2;
        public const int VK_MEDIA_NEXT_TRACK = 0xB0;
        public const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        public const int VK_MEDIA_PREV_TRACK = 0xB1;
        public const int VK_VOLUME_DOWN = 0xAE;
        public const int VK_VOLUME_UP = 0xAF;
        public const int VK_VOLUME_MUTE = 0xAD;

        protected TcpListener listener;

        public Server()
        {

        }

        public void RunServer()
        {

            listener = new TcpListener(IPAddress.Parse("192.168.1.11"), 54001);
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
            byte[] tosend = new byte[1024];

            try
            {
                byte[] data = new byte[1024];
                string recv;

                Int32 bytes = ns.Read(data, 0, data.Length);

                recv = System.Text.Encoding.UTF8.GetString(data, 0, bytes);

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
                }
            }
            catch (System.IO.IOException ex)
            {
                Console.WriteLine($"IOException: {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine($"TimeoutException: {ex.Message}");
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($"ArgumentNullException: {ex.Message}");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"SocketException: {ex.Message}");
            }
        }


        [DllImport("user32.dll", SetLastError = true)]
        protected static extern void keybd_event(byte virtualkey, byte scancode, uint flags, IntPtr extrainfo);

    }
}
