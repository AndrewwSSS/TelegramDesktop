﻿using MessageLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace CommonLibrary
{
    [Serializable]
    public class ChatMessage : Message, INotifyPropertyChanged
    {
        private string text;
        private bool showAvatar =false;

        public string Text
        {
            get => text;
            set
            {
                text = value;
                OnPropertyChanged();
            }
        }
        public User FromUser { get; set; }
        public User ToUser { get; set; }
        public User ResendUser { get; set; }
        public ChatMessage RespondingTo { get; set; }
        public List<ImageContainer> Images { get; set; }
        public List<FileContainer> Files { get; set; }

        public ChatMessage(string text)
        {

            Text = text;
            Type = MessageType.Custom;
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ChatMessage SetFrom(User user)
        {
            FromUser = user;
            return this;
        }

        public ChatMessage SetTo(User user)
        {
            ToUser = user;
            return this;
        }

        public ChatMessage SetResendUser(User user)
        {
            ResendUser = user;
            return this;
        }

        public ChatMessage SetRespondingTo(ChatMessage msg)
        {
            RespondingTo = msg;
            return this;
        }

        public bool ShowAvatar { 
            get => showAvatar;
            set
            {
                showAvatar = value;
                OnPropertyChanged();
            }
        }

    }
}
