using CommonLibrary.Messages.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace CommonLibrary.Messages.Groups
{
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
        public List<ChatMessage> Messages { get; set; }
        public ChatMessage LastMessage => Messages.LastOrDefault();
        public List<User> Members { get; set; }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public bool AddMessage(ChatMessage message)
        {
            if (message != null)
            {
                Messages.Add(message);
                OnPropertyChanged(nameof(Messages));
                return true;
            }
            else
                return false;
        }

        public bool DelleteMessage(ChatMessage message)
        {
            if(message != null && Messages.Remove(message))
            {
                OnPropertyChanged(nameof(Messages));
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


        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
