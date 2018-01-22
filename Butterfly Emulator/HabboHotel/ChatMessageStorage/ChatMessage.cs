using System;
using Butterfly.Messages;

namespace Butterfly.HabboHotel.ChatMessageStorage
{
    class ChatMessage
    {
        internal readonly uint userID;
        private readonly string username;
        internal readonly uint roomID;
        internal readonly string message;
        internal readonly bool isTalking;
        internal readonly DateTime timeSpoken;

        public ChatMessage(uint userID, string username, uint roomID,  string message, DateTime timeSpoken, bool isTalking)
        {
            this.userID = userID;
            this.username = username;
            this.roomID = roomID;
            this.message = message;
            this.timeSpoken = timeSpoken;
            this.isTalking = isTalking;
        }

        internal void Serialize(ref ServerMessage packet)
        {
            packet.AppendString(timeSpoken.ToShortTimeString());
            packet.AppendUInt(userID);
            packet.AppendString(username);
            packet.AppendString(message);
            packet.AppendBoolean(!isTalking); // negrita?
        }
    }
}
