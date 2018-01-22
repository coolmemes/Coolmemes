using Butterfly.Messages;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uber.HabboHotel.Rooms;

namespace Butterfly.HabboHotel.Rooms
{
    class RoomChat
    {
        private RoomUser Parent;

        private string chatColor;
        private InvokedChatMessage Message;

        public RoomUser GetParent()
        {
            return Parent;
        }


        public RoomChat(RoomUser Parent, InvokedChatMessage Message)
        {
            this.Parent = Parent;
            this.Message = Message;
            if (Parent.GetClient() != null && Parent.GetClient().GetHabbo() != null)
                this.chatColor = Parent.GetClient().GetHabbo().ChatColor + " ";
        }

        public ServerMessage GenerateMessage(bool OldChat)
        {
            var ChatMessage = new ServerMessage((Message.shout) ? Outgoing.Shout : Outgoing.Talk);
            ChatMessage.AppendInt32(Parent.VirtualId);
            ChatMessage.AppendString(OldChat ? Message.message : chatColor + Message.message.Replace("<", "¤"));
            ChatMessage.AppendInt32(RoomUser.GetSpeechEmotion(Message.message)); // gesture
            ChatMessage.AppendInt32(Message.color); // styleId
            ChatMessage.AppendInt32(0); // links (foreach)
            // String, String, Boolean
            ChatMessage.AppendInt32(-1); // ¿timer?
            return ChatMessage;
        }
    }
}
