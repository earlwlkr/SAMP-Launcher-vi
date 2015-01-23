using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net;

namespace SAMP_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string    _launcherTitle  = "SAMP Launcher";
        // Tên miền hoặc địa chỉ IP của server.
        private const string    _serverName     = "127.0.0.1";
        private const int       _serverPort     = 7777;
        // Password để vào server.
        private const string    _serverPassword = "";

        private string      _serverIP;
        private string      _playerName;
        private SAMP.Query  _query;

        public MainWindow()
        {
            InitializeComponent();

            MainWindow1.Title = _launcherTitle;
            lblSvrName.Content = _launcherTitle;
            lblStatus.Text = "Đang chờ";
        }

        private void MainWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            if (checkFiles() == false)
                Application.Current.Shutdown();

            // Lấy tên nhân vật từ registry và cho vào textbox.
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\SAMP");
            if (key != null)
            {
                Object value = key.GetValue("PlayerName");
                if (value != null)
                {
                    txtbxPlayerName.Text = value.ToString();
                }
            }
            _playerName = txtbxPlayerName.Text;


            IPAddress svIP;
            if (IPAddress.TryParse(_serverName, out svIP))
            {
                _serverIP = svIP.ToString();
            }
            else
            {
                // Phân giải tên miền.
                IPHostEntry hostEntry = Dns.GetHostEntry(_serverName);
                if (hostEntry.AddressList.Length == 0)
                {
                    lblStatus.Text = "<!> Đã có lỗi xảy ra với DNS. Xin vui lòng liên hệ Admin.";
                    return;
                }
                _serverIP = hostEntry.AddressList[0].ToString();
            }
            _query = new SAMP.Query(_serverIP, _serverPort, false);

            updateServerInfo();
        }

        private void updateServerInfo()
        {
            if (_query.Send((char)SAMP.Query.PaketOpcode.Info))
            {
                int count = _query.Receive();
                if (count == 0)
                {
                    lblStatus.Text = "<!> Không thể kết nối đến server.";
                    btnConnect.IsEnabled = false;
                    return;
                }

                lblServer.Content = _query.hostname;
                lblGameMode.Content = _query.gamemode;
                lblPlayersCount.Content = _query.players + '/' + _query.max_players;

                if (_query.Send((char)SAMP.Query.PaketOpcode.Rules))
                {
                    count = _query.Receive();
                    if (count == 0)
                    {
                        lblStatus.Text = "<!> Không thể kết nối đến server.";
                        btnConnect.IsEnabled = false;
                        return;
                    }
                    string[] rules = _query.Store(count);

                    for (int i = 0; i < rules.Length - 2; i += 2)
                    {
                        if (rules[i] == "mapname")
                        {
                            lblMap.Content = rules[i + 1];
                        }
                        else if (rules[i] == "weburl")
                        {
                            lblWebsite.Content = rules[i + 1];
                        }
                    }
                }
            }
            else
            {
                btnConnect.IsEnabled = false;
                lblStatus.Text = "<!> Không thể kết nối đến server.";
            }
        }

        private bool checkFiles()
        {
            if (File.Exists("samp.exe") == false)
            {
                MessageBox.Show("<!> Không tìm thấy samp.exe\n<!> Vui lòng bỏ Launcher vào cùng thư mục với file samp.exe (trong folder game).", "Lỗi");
                return false;
            }
            //if (File.Exists("d3d9.dll"))
            //{
            //    MessageBox.Show("<!> Hê thống nhận thấy bạn đang sử dụng phần mềm thứ 3 không hợp lệ (s0beit).\n<!> Hãy xóa các file đó hoặc re-install game.", "Lỗi");
            //    return false;
            //}
            //if (File.Exists("cleo.asi"))
            //{
            //    MessageBox.Show("<!> Hê thống nhận thấy bạn đang sử dụng phần mềm thứ 3 không hợp lệ (CLEO).\n<!> Hãy xóa các file đó hoặc re-install game.", "Lỗi");
            //    return false;
            //}
            return true;
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (checkFiles() == false)
                return;
     
            // kiểm tra tên nhân vật có thay đổi không
            lblStatus.Text = "[*] Đang kiểm tra tên nhân vật.";
            if (txtbxPlayerName.Text != _playerName)
            {
                // nếu tên mới không thỏa điều kiện
                if (txtbxPlayerName.Text.Length > 20 || txtbxPlayerName.Text.Length < 4)
                {
                    MessageBox.Show("<!> Tên không được ít hơn 4 hoặc nhiều hơn 20 ký tự.", "Tên không hợp lệ!");
                    return;
                }

                // chỉnh lại giá trị trong registry
                RegistryKey setKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\SAMP");
                if (setKey != null)
                {
                    setKey.SetValue("PlayerName", txtbxPlayerName.Text);
                }
            }
            //MessageBox.Show(_serverIP + ":" + _serverPort);

            lblStatus.Text = "[*] Đang tiến hành kết nối đến server.";
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "samp.exe";
            startInfo.Arguments = _serverIP + ":" + _serverPort + " " + _serverPassword;
            lblStatus.Text = "[*] Đang vào game.";
            Process.Start(startInfo);
            lblStatus.Text = "[*] Thành công.";
        }
    }
}
