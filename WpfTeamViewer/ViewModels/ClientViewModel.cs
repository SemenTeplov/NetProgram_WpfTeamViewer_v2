using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
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

namespace WpfTeamViewer.ViewModels
{
    class ClientViewModel : ViewModelBase
    {
        private ImageSource image;
        public ImageSource Image { get => image; set => Set(ref image, value); }

        private string ip = "127.0.0.1";
        public string Ip { get => ip; set => Set(ref ip, value); }

        private int port = 8080;
        public int Port { get => port; set => Set(ref port, value); }

        private string chat = String.Empty;
        public string Chat { get => chat; set => Set(ref chat, value); }

        private string mess = String.Empty;
        public string Mess { get => mess; set => Set(ref mess, value); }

        private string messOut = String.Empty;
        public string MessOut { get => messOut; set => Set(ref messOut, value); }

        private RelayCommand connectCommand = null;
        public RelayCommand ConnectCommand => connectCommand ??= new RelayCommand(
            () =>
            {
                Task.Run(() =>
                {
                    UdpClient udpClient = new UdpClient(Port);

                    using (var memory = new MemoryStream())
                    {
                        while (true)
                        {
                            try
                            {
                                IPEndPoint endPoint = null;
                                byte[] data = udpClient.Receive(ref endPoint);
                                memory.Write(data, 0, data.Length);

                                if (data.Length == 3 && data[0] == 1 && data[1] == 2 && data[2] == 3)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        Image = ByteToImage(memory.GetBuffer());
                                        memory.SetLength(0);
                                    });
                                }
                                else if (data.Length == 3 && data[0] == 4 && data[1] == 5 && data[2] == 6)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        foreach (var s in Encoding.UTF8.GetString(memory.GetBuffer()))
                                        {
                                            Chat += s;
                                            if (s == '\n') break;
                                        }
                                        Chat += '\n';
                                        memory.SetLength(0);
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                    }
                });
            });

        public ImageSource ByteToImage(byte[] bytes)
        {
            try
            {
                BitmapImage bitmapImage = new BitmapImage();
                MemoryStream memory = new MemoryStream(bytes);

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();

                ImageSource imageSource = bitmapImage as ImageSource;

                return imageSource;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }
    }
}
