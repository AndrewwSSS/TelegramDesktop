﻿using MessageLibrary;
using System;


namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class GroupJoinMessage : BaseMessage
    {
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public string Guid { get; set; }

        public GroupJoinMessage(int GroupId, int UserId, string Guid)
        {
            this.GroupId = GroupId;
            this.UserId = UserId;
            this.Guid = Guid;
        }

        public GroupJoinMessage() { }
    }
}
