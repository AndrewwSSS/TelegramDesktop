using CommonLibrary.Messages.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public enum GroupType
    {
        Personal,
        Public
    }

    [Serializable]
    public class GroupChat : UserEntity, INotifyPropertyChanged
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public GroupType Type { get; set; }
        public DateTime DateCreated { get; set; }
        public virtual List<ChatMessage> Messages { get; set; }
        public virtual List<User> Members { get; set; } = new List<User>();

        public ChatMessage LastMessage => Messages != null ? Messages.LastOrDefault() : null;

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;


        public GroupChat() 
        {
            Messages = new List<ChatMessage>();
            Members = new List<User>();
        }


        public bool AddMessage(ChatMessage message)
        {
            if (message != null)
            {
                Messages.Add(message);
                OnPropertyChanged(nameof(Messages));
                OnPropertyChanged(nameof(LastMessage));
                return true;
            }
            else
                return false;
        }

        public bool RemoveMessage(ChatMessage message)
        {
            if (message != null && Messages.Remove(message))
            {
                OnPropertyChanged(nameof(Messages));
                OnPropertyChanged(nameof(LastMessage));
                return true;
            }
            else
                return false;
        }

        public bool AddMember(User member)
        {
            if (member != null)
            {
                Members.Add(member);
                OnPropertyChanged(nameof(Members));
                return true;
            }
            else
                return false;
        }

        public bool DeleteMember(User member)
        {
            if (member != null && Members.Remove(member))
            {
                OnPropertyChanged(nameof(Members));
                return true;
            }
            else
                return false;

        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
