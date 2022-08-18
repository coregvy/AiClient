using AiClient.Properties;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Windows;

namespace AiClient
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ViewModel viewModel = new ViewModel();
        private readonly WSCtrl wsCtrl = new WSCtrl();
        private int myPlayer = -1;
        private string ServerUrl = "www.tomiko.cf/red";
        private string Protocol = "https";
        //private const string URL_BASE = "https://www.tomiko.cf/red/api/";

        public MainWindow()
        {
            DataContext = viewModel;
            InitializeComponent();
            Title += " ver." + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            ServerUrl = Settings.Default.ServerUrl;
            Protocol = Settings.Default.IsSecure ? "https" : "http";
            ReloadRoom_Click(null, null);
            Command.Text = Settings.Default.Command;
            CurrentDir.Text = Settings.Default.CurrentDir;
            wsCtrl.ReceiveMessage += (sender, e) =>
            {
                Debug.WriteLine($"get ws message: {sender}");
                var msg = sender as WSMessage;
                if (!string.IsNullOrWhiteSpace(msg.Message))
                {
                    if (string.IsNullOrWhiteSpace(MessageStr.Text)) MessageStr.Text = msg.Message;
                    else MessageStr.Text += "\n" + msg.Message;
                }
                switch(msg.Call)
                {
                    case "step":
                        Debug.WriteLine($"player: {msg.Player}, user0: {msg.User0}, use1: {msg.User1}");
                        if (msg.Player != myPlayer) break;
                        InputStr.Text = msg.Stdin;
                        msg.Stdout = RunCommand();
                        msg.Message = null;
                        _ = wsCtrl.SendMessage(msg);
                        break;
                    case "end":
                        Debug.WriteLine($"game end");
                        MessageBox.Show(msg.Message);
                        viewModel.IsGameStart = false;
                        myPlayer = -1;
                        _ = wsCtrl.CloseAsync();
                        ReloadRoom_Click(null, null);
                        break;
                }
            };
        }

        private void Debug_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.IsGameStart) return;
            OutputStr.Text = "";
            if (string.IsNullOrEmpty(InputStr.Text))
            {
                MessageBox.Show("入力がありません.");
                return;
            }
            if (string.IsNullOrWhiteSpace(Command.Text))
            {
                MessageBox.Show("実行コマンドを入力してください.");
                return;
            }
            SaveSetting();
            RunCommand();
            MessageStr.Text = "テスト実行が完了しました。";
        }

        private string RunCommand()
        {
            Debug.WriteLine("run command begin.");
            var p = new Process();
            //ComSpec(cmd.exe)のパスを取得して、FileNameプロパティに指定
            p.StartInfo.FileName = Environment.GetEnvironmentVariable("ComSpec");
            //出力を読み取れるようにする
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;
            //ウィンドウを表示しないようにする
            p.StartInfo.CreateNoWindow = true;
            //コマンドラインを指定（"/c"は実行後閉じるために必要）
            p.StartInfo.Arguments = @"/c " + Command.Text;
            if (!string.IsNullOrEmpty(CurrentDir.Text)) p.StartInfo.WorkingDirectory = CurrentDir.Text;
            var stdout = "";
            p.OutputDataReceived += (sender2, e2) =>
            {
                stdout += e2.Data;
            };
            var stderr = "";
            p.ErrorDataReceived += (sender2, e2) =>
            {
                stderr += e2.Data + "\n";
            };
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.StandardInput.WriteLine(InputStr.Text);
            var res = p.WaitForExit(Settings.Default.AppTimeout);
            //OutputStr.Text = p.StandardOutput.ReadToEnd();
            p.Close();
            if (!res) OutputStr.Text = stdout + "\n***** プログラムが終了しませんでした. *****";
            else if (!string.IsNullOrEmpty(stdout)) OutputStr.Text = stdout;
            ErrorStr.Text = stderr;
            return stdout;
        }


        private async void ReloadRoom_Click(object sender, RoutedEventArgs e)
        {
            viewModel.RoomList = RoomInfo.ReloadRoom;
            var res = await ApiCall.GetAllRoom();
            viewModel.RoomList = res;
        }

        private async void Login0_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.IsGameStart) return;
            if (string.IsNullOrWhiteSpace(viewModel.UserName))
            {
                MessageBox.Show("User Nameを入力してください.");
                return;
            }
            if (RoomList.SelectedIndex < 0)
            {
                MessageBox.Show("参加するルームを選択してください.");
                return;
            }
            if (string.IsNullOrWhiteSpace(Command.Text))
            {
                MessageBox.Show("実行コマンドを入力してください.");
                return;
            }
            viewModel.IsGameStart = true;
            InputStr.Text = "";
            OutputStr.Text = "";
            SaveSetting();
            Debug.WriteLine($"user: {viewModel.UserName} {RoomList.SelectedIndex}");
            var sendMsg = new WSMessage()
            {
                Call = "login"
            };
            var room = viewModel.RoomList.ToArray()[RoomList.SelectedIndex];
            sendMsg.User0 = UserName.Text;
            sendMsg.Player = 0;
            myPlayer = 0;

            await wsCtrl.OpenAsync(room, viewModel.UserName, sendMsg);
            Debug.WriteLine("call openAsync end");
            
        }
        private async void Login1_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.IsGameStart) return;
            if (string.IsNullOrWhiteSpace(viewModel.UserName))
            {
                MessageBox.Show("User Nameを入力してください.");
                return;
            }
            if (RoomList.SelectedIndex < 0)
            {
                MessageBox.Show("参加するルームを選択してください.");
                return;
            }
            if (string.IsNullOrWhiteSpace(Command.Text))
            {
                MessageBox.Show("実行コマンドを入力してください.");
                return;
            }
            viewModel.IsGameStart = true;
            InputStr.Text = "";
            OutputStr.Text = "";
            SaveSetting();
            Debug.WriteLine($"user: {viewModel.UserName} {RoomList.SelectedIndex}");
            var sendMsg = new WSMessage()
            {
                Call = "login"
            };
            var room = viewModel.RoomList.ToArray()[RoomList.SelectedIndex];
            sendMsg.User1 = UserName.Text;
            sendMsg.Player = 1;
            myPlayer = 1;

            await wsCtrl.OpenAsync(room, viewModel.UserName, sendMsg);
            Debug.WriteLine("call openAsync end");

        }

        private void SaveSetting()
        {
            Settings.Default.Command = Command.Text;
            Settings.Default.CurrentDir = CurrentDir.Text;
            // AppTimeoutをXMLに出す
            Settings.Default.AppTimeout = Settings.Default.AppTimeout;
            Settings.Default.ServerUrl = Settings.Default.ServerUrl;
            Settings.Default.IsSecure = Settings.Default.IsSecure;
            Settings.Default.Save();
        }

        private void OpenBrowser_Click(object sender, RoutedEventArgs e)
        {
            //WSMessage msg = new WSMessage() { Call = "call1", Message = "msg1", Player = 0, Ready = true };
            //Debug.WriteLine(msg.ToString());
            if (RoomList.SelectedIndex < 0)
            {
                MessageBox.Show("ルームを選択してください.");
                return;
            }
            var selRoom = viewModel.RoomList.ToArray()[RoomList.SelectedIndex];
            Process.Start($"{Protocol}://{ServerUrl}/con4/room/{selRoom.Path}.html");
        }
    }
}
