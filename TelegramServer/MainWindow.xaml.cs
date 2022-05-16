﻿using MessageLibrary;
using System.Collections.ObjectModel;
using System.Windows;
using CommonLibrary;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;
using System;

namespace TelegramServer
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<User> UsersOnline;
        private ObservableCollection<User> UsersOffline;
        private TelegramDb DbContext;

        private TcpServerWrap Server;

        public MainWindow()
        {
            InitializeComponent();

            UsersOnline = new ObservableCollection<User>();
            UsersOffline = new ObservableCollection<User>();
            Server = new TcpServerWrap();

        
            LB_UsersOffline.ItemsSource = UsersOffline;
            LB_UsersOnline.ItemsSource = UsersOnline;


            DbContext = new TelegramDb();


            foreach (User user in DbContext.Users.ToList()) {
                UsersOffline.Add(user);
            }
          





            Server.Started += OnServerStarted;
            Server.Stopped += OnServerStopped;
            Server.MessageReceived += ClientMessageRecived;
  
        }


        private void BtnStartServer_Click(object sender, RoutedEventArgs e)
        {
            int port;
            if(int.TryParse(TB_ListenerPort.Text, out port) && port > 999 && port < 10000)
            {
                BtnStartServer.IsEnabled = false;
                Server.Start(port, 1000);
            }
        }

        private void OnServerStarted(TcpServerWrap client)
        {
            Dispatcher.Invoke(() =>
            {
                BtnStopServer.IsEnabled = true;
            });
        }

        private void OnServerStopped(TcpServerWrap client)
        {
            Dispatcher.Invoke(() =>
            {
                BtnStopServer.IsEnabled = false;
                BtnStartServer.IsEnabled = true;
            });
        }

        private void ClientMessageRecived(TcpClientWrap client, Message msg)
        {
            switch (msg.GetType().Name)
            {
                case "SignUpMessage":
                {
                    SignUpMessage signUpMessage = (SignUpMessage)msg;
                    
                    if(DbContext.Users.Count(u => u.Email == signUpMessage.Email || u.Login == signUpMessage.Login) == 0)
                    {
                        User NewUser = new User()
                        {
                            Email = signUpMessage.Email,
                            Login = signUpMessage.Login,
                            Name = signUpMessage.Name,
                            Password = signUpMessage.Password,
                            RegistrationDate = DateTime.Now,
                            LastVisitDate = DateTime.Now
                        };

                        DbContext.Users.Add(NewUser);
                        Dispatcher.Invoke(() => UsersOnline.Add(NewUser));
                        DbContext.SaveChanges();

                        SignUpResultMessage ResultMessage
                                = new SignUpResultMessage(AuthenticationResult.Success).SetRegistrationDate(NewUser.RegistrationDate);
                        
                        client.SendAsync(ResultMessage);

                    }
                    else
                    {
                        SignUpResultMessage ResultMessage
                            = new SignUpResultMessage(AuthenticationResult.Denied, "Email or login already used to create an account");
                        client.SendAsync(ResultMessage);
                    }

                        
                    break;
                }
                case "LoginMessage":
                { 
                    LoginMessage loginMessage = (LoginMessage)msg;

                    User user =  DbContext.Users.FirstOrDefault(u => u.Login == loginMessage.Login || u.Email == loginMessage.Login);

                    if(user != null && user.Password == loginMessage.Password)
                    {
                      
                        user.client = client;
                        LoginResultMessage ResultMessage = new LoginResultMessage(AuthenticationResult.Success);

                        if (user.Login == loginMessage.Login)
                             ResultMessage.Login = user.Login;
                        else
                            ResultMessage.Email = user.Email;

                        ResultMessage.Name = user.Name;
                        ResultMessage.RegistrationDate = user.RegistrationDate;

                        client.SendAsync(ResultMessage);

                        Dispatcher.Invoke(() =>
                        {
                            UsersOffline.Remove(user);
                            UsersOnline.Add(user);
                        });
                      

                        if(user.MessagesToSend != null && user.MessagesToSend.Count > 0)
                        {
                            ArrayMessage<ChatMessage> Messages
                                    = new ArrayMessage<ChatMessage>(user.MessagesToSend);

                            client.SendAsync(Messages);

                            //temp. Must be after message sent
                            user.MessagesToSend.Clear();
                        }
                    }
                    else
                    {
                         LoginResultMessage ReultMessage = new LoginResultMessage(AuthenticationResult.Denied, "wrong login/email or password");
                         client.SendAsync(ReultMessage);
                    }
                    break;
                }
                case "ChatMessage":
                {
                     ChatMessage chatMessage = (ChatMessage)msg;
                   
                     GroupChat groupChat = DbContext.GroupChats.First(gc => gc.Id == chatMessage.Id);
                     groupChat.Messages.Add(chatMessage);

                     foreach(var user in chatMessage.Chat.Members)
                     {
                         
                         if(UsersOnline.FirstOrDefault(u => u.Id == user.Id && u.Id != chatMessage.FromUser.Id) != null) {
                              user.client.SendAsync(chatMessage);
                         }
                         else
                         {
                              if(user.Id != chatMessage.FromUser.Id)
                                   user.MessagesToSend.Add(chatMessage);
                         }
                     }
                    
                    break;
                }
                

                

            }

        }

    }
}
