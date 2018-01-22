using System;

using Butterfly.Messages;
using ButterStorm;
using HabboEvents;
using System.Collections.Generic;
namespace Butterfly.HabboHotel.Support
{
    enum TicketStatus
    {
        OPEN,
        PICKED,
        RESOLVED,
        ABUSIVE,
        INVALID,
        DELETED
    }

    class SupportTicket
    {
        private uint Id;
        internal int Score;
        internal int Type;

        internal TicketStatus Status;

        internal uint SenderId;
        internal int ReportedId;
        internal uint ModeratorId;

        internal string Message;

        internal uint RoomId;
        internal string RoomName;

        internal double Timestamp;

        internal string[] Chats;

        private string SenderName;
        private string ReportedName;
        private string ModName;

        internal int TabId
        {
            get
            {
                if (Status == TicketStatus.OPEN)
                {
                    return 1;
                }

                if (Status == TicketStatus.PICKED)
                {
                    return 2;
                }

                if (Status == TicketStatus.ABUSIVE || Status == TicketStatus.INVALID || Status == TicketStatus.RESOLVED)
                    return 0;

                if (Status == TicketStatus.DELETED)
                    return 0;

                return 0;
            }
        }

        internal UInt32 TicketId
        {
            get
            {
                return Id;
            }
        }

        internal SupportTicket(Int32 Id, int Score, int Type, UInt32 SenderId, String SenderName, Int32 ReportedId, String Message, UInt32 RoomId, String RoomName, Double Timestamp, string[] Chats)
        {
            this.Id = (uint)Id;
            this.Score = Score;
            this.Type = Type;
            this.Status = TicketStatus.OPEN;
            this.SenderId = SenderId;
            this.SenderName = SenderName;
            this.ReportedId = ReportedId;
            this.ModeratorId = 0;
            this.Message = Message;
            this.RoomId = RoomId;
            this.RoomName = RoomName;
            this.Timestamp = Timestamp;
            this.ReportedName = "";
            if(ReportedId > 0)
                this.ReportedName = OtanixEnvironment.GetGame().GetClientManager().GetNameById((uint)ReportedId);
            this.ModName = "";
            this.Chats = Chats;
        }

        internal void SerializeBody(ServerMessage message)
        {
            message.AppendUInt(Id); // id
            message.AppendInt32(TabId); // state
            message.AppendInt32(7); // type (3 or 4 for new style)
            message.AppendInt32(Type); // cat id
            message.AppendInt32((OtanixEnvironment.GetUnixTimestamp() - (int)Timestamp) * 1000); // issueAgeInMilliseconds
            message.AppendInt32(Score); // priority
            message.AppendUInt(1); // ensures that more tickets of the same reporter/reported user get merged
            message.AppendUInt(SenderId); // caller_user_info
            message.AppendString(SenderName);
            message.AppendInt32(ReportedId); // reported id
            message.AppendString(ReportedName);
            message.AppendUInt((Status == TicketStatus.PICKED) ? ModeratorId : 0); // mod id
            message.AppendString(ModName);
            message.AppendString(Message); // issue message
            message.AppendUInt(0); // roomid

            message.AppendInt32(Chats.Length);
            foreach (string str in Chats)
            {
                message.AppendString(str); // pattern
                message.AppendInt32(-1); // startIndex
                message.AppendInt32(-1); // endIndex
            }
        }
    }
}
