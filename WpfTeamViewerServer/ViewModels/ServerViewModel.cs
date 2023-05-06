using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfTeamViewerServer.ViewModels
{
    class ServerViewModel : ViewModelBase
    {
        private string ip = "127.0.0.1";
        public string Ip { get => ip; set => Set(ref ip, value); }

        private int port = 8080;
        public int Port { get => port; set => Set(ref port, value); }

        private string info;
        public string Info { get => info; set => Set(ref info, value); }

        private string chat = String.Empty;
        public string Chat { get => chat; set => Set(ref chat, value); }

        private string mess = String.Empty;
        public string Mess { get => mess; set => Set(ref mess, value); }

        private string messOut = String.Empty;
        public string MessOut { get => messOut; set => Set(ref messOut, value); }

        public bool screen = false;

        private RelayCommand startCommand;
        public RelayCommand StartCommand => startCommand ??= new RelayCommand(
            () =>
            {
                Task.Run(() =>
                {
                    UdpClient udpClient = new UdpClient();

                    while (true)
                    {
                        try
                        {
                            byte[] data = CaptureScreen(ref screen);
                            byte[] chunk = new byte[10000];
                            int bytesCount = 0;

                            if (MessOut != String.Empty)
                            {
                                using (var memory = new MemoryStream(Encoding.UTF8.GetBytes(MessOut + '\n')))
                                {
                                    while ((bytesCount = memory.Read(chunk, 0, 10000)) != 0)
                                    {
                                        udpClient.Send(chunk, bytesCount, Ip, Port);
                                    }
                                }

                                udpClient.Send(new byte[] { 4, 5, 6 }, 3, Ip, Port);
                                MessOut = String.Empty;
                            }
                            else
                            {
                                using (var memory = new MemoryStream(data))
                                {
                                    while ((bytesCount = memory.Read(chunk, 0, 10000)) != 0)
                                    {
                                        udpClient.Send(chunk, bytesCount, Ip, Port);
                                    }
                                }

                                udpClient.Send(new byte[] { 1, 2, 3 }, 3, Ip, Port);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка " + ex.Message);
                        }
                    }
                });
            });

        private RelayCommand sendCommand;
        public RelayCommand SendCommand => sendCommand ??= new RelayCommand(
            async () =>
            {
                MessOut = Mess;
                Mess = String.Empty;
            });

        private RelayCommand screenCommand;
        public RelayCommand ScreenCommand => screenCommand ??= new RelayCommand(
            async () =>
            {
                screen = true;
            });

        public byte[] CaptureScreen(ref bool screen)
        {
            var width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            var height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

            using (var memory = new MemoryStream())
            {
                Bitmap bmp = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(0, 0, 0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size);
                    Bitmap objBitmap = new Bitmap(bmp, width, height);
                    if (screen) 
                    {
                        objBitmap.Save(@"screen.jpg", ImageFormat.Jpeg);
                        screen = false;
                    }
                    objBitmap.Save(memory, ImageFormat.Jpeg);
                }
                return memory.GetBuffer();
            }
        }
    }
}
