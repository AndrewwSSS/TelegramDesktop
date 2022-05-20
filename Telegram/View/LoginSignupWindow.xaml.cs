using CommonLibrary.Messages.Auth;
using CommonLibrary.Messages.Users;
using MessageLibrary;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;

namespace Telegram.View
{
    /// <summary>
    /// Логика взаимодействия для LoginSignupWindow.xaml
    /// </summary>
    public partial class LoginSignupWindow : Window
    {
        private TcpClientWrap client;
        public LoginSignupWindow()
        {
            InitializeComponent();

            tabControl.IsEnabled = false;
            client = App.Client;
            Closed += LoginSignupWindow_Closed;
        }

        private void LoginSignupWindow_Closed(object sender, System.EventArgs e)
        {
            if (wnd == null) 
                client.Disconnect();
        }
        private void Client_ConnectFailed(TcpClientWrap obj) => client.ConnectAsync();

        private void Client_Connected(TcpClientWrap client)
        {
            Dispatcher.Invoke(() =>
            {
                tabControl.IsEnabled = true;
                client.Connected -= Client_Connected;
                client.ConnectFailed -= Client_ConnectFailed;
                client.MessageReceived += Client_MessageReceived;
            });
        }

        public static MainWindow wnd;
        private void Client_MessageReceived(TcpClientWrap client, Message msg)
        {
            if (msg is SignUpResultMessage)
            {
                Dispatcher.Invoke(() =>
                {
                    var result = msg as SignUpResultMessage;
                    if (result.Result == AuthenticationResult.Success)
                        MessageBox.Show("Регистрация прошла успешно.");
                    else
                        MessageBox.Show("Регистрация не произошла.");

                    tabControl.IsEnabled = true;
                });
            } else if(msg is LoginResultMessage)
            {
                Dispatcher.Invoke(() => {
                    var result = msg as LoginResultMessage;
                    if(result.Result == AuthenticationResult.Success)
                    {
                        var info = result.UserInfo;
                        if (info.Login == null)
                            info.Login = TB_Login_Id.Text;
                        client.MessageReceived -= Client_MessageReceived;


                        wnd = new MainWindow(info);
                        wnd.Client = App.Client;
                        wnd.Show();
                        Close();
                    } else
                        tabControl.IsEnabled = true;
                });
            }
        }

        private void ButtonLoginSend_Click(object sender, RoutedEventArgs e)
        {
            string email = TB_Login_Id.Text,
                password = TB_Login_Password.Text;
            if(email.Length == 0||password.Length == 0)
            {
                MessageBox.Show("Не все поля заполнены");
                return;
            }
            var msg = new LoginMessage(email, password);
            tabControl.IsEnabled = false;
            client.SendAsync(msg);
            client.ReceiveAsync();  
        }

        private bool IsValidPassword(string pw)
        {
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMinimum8Chars = new Regex(@".{8,}");
            return hasNumber.IsMatch(pw) && hasUpperChar.IsMatch(pw) && hasMinimum8Chars.IsMatch(pw);
        }

        private void ButtonSignUpSend_Click(object sender, RoutedEventArgs e)
        {
            string login = TB_SignUp_Login.Text,
                password = TB_SignUp_Password.Text,
                email = TB_SignUp_Email.Text,
                name = TB_SignUp_UserName.Text;
            if (login.Length==0 ||
               password.Length==0 ||
               email.Length==0 ||
               name.Length == 0)
            {
                MessageBox.Show("Не все поля заполнены");
                return;
            }

            if (!IsValidPassword(password))
            {
                MessageBox.Show("Пароль должен быть не менее восьми символов в длину и содержать большую букву и цифру");
                return;
            }
            var msg = new SignUpMessage() { 
                Login = login,
                Password = password,
                Email = email,
                Name = name
            };
            tabControl.IsEnabled = false;
            client.SendAsync(msg);
            client.ReceiveAsync();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            client.Connected += Client_Connected;
            client.ConnectFailed += Client_ConnectFailed;
            client.ConnectAsync();
        }
    }
}
