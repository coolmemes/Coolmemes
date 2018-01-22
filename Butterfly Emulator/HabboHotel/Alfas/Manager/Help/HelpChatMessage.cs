using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Alfas.Manager
{
    class HelpChatMessage
    {
        internal Boolean NormalChat;
        internal UInt32 UserId;
        internal String Message;
        internal UInt32 RoomId;

        internal HelpChatMessage(Boolean normalChat, UInt32 userId, String message, UInt32 roomId)
        {
            this.NormalChat = normalChat;
            this.UserId = userId;
            this.Message = message;
            this.RoomId = roomId;
        }
    }
}
