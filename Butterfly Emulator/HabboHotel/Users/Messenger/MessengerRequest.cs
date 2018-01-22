using System;

using Butterfly.Messages;
using ButterStorm;

namespace Butterfly.HabboHotel.Users.Messenger
{
    class MessengerRequest
    {
        private readonly UInt32 ToUser;
        private readonly UInt32 FromUser;
        private readonly string mUsername;

        internal UInt32 To
        {
            get
            {
                return ToUser;
            }
        }

        internal UInt32 From
        {
            get
            {
                return FromUser;
            }
        }

        internal MessengerRequest(UInt32 ToUser, UInt32 FromUser, string pUsername)
        {
            this.ToUser = ToUser;
            this.FromUser = FromUser;
            this.mUsername = pUsername;
        }

        internal void Serialize(ServerMessage Request)
        {
            Request.AppendUInt(FromUser);
            Request.AppendString(mUsername);
            var user = UsersCache.getHabboCache(mUsername);
            try { Request.AppendString((user != null) ? user.Look : ""); } catch { Request.AppendString(""); }
        }
    }
}
