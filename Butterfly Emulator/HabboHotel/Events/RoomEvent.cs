using System;
using System.Collections.Generic;
using System.Collections;
using Butterfly.Messages;
using Butterfly.HabboHotel.GameClients;
using HabboEvents;
using ButterStorm;

namespace Butterfly.HabboHotel.Rooms
{
    class RoomEvent
    {
        internal UInt32 EventId;
        internal string Name;
        internal string Description;
        internal int Category;
        internal DateTime StartTime;
        internal int PromCategory;

        internal UInt32 CreatorId;
        internal String CreatorName;
        internal UInt32 RoomId;

        internal RoomEvent(UInt32 eventId, UInt32 RoomId, UInt32 creatorId, String creatorName, string Name, string Description, int Category, List<string> tags, int promcategory)
        {
            this.EventId = eventId;
            this.RoomId = RoomId;
            this.CreatorId = creatorId;
            this.CreatorName = creatorName;
            this.Name = Name;
            this.Description = Description;
            this.Category = Category;
            this.StartTime = DateTime.Now;
            this.PromCategory = promcategory;
        }

        internal ServerMessage Serialize()
        {
            ServerMessage Message = new ServerMessage(Outgoing.RoomEvent);
            Message.AppendUInt(EventId);
            Message.AppendUInt(CreatorId);
            Message.AppendString(CreatorName);
            Message.AppendUInt(RoomId);
            Message.AppendInt32(Category);
            Message.AppendString(Name);
            Message.AppendString(Description);
            Message.AppendInt32(0);
            Message.AppendInt32(120 - (DateTime.Now - (DateTime)StartTime).Minutes);
            Message.AppendInt32(PromCategory);

            return Message;
        }

        internal void EndEvent()
        {
            Room Room = OtanixEnvironment.GetGame().GetRoomManager().LoadRoom(RoomId);
            if (Room == null || Room.RoomData == null)
                return;

            OtanixEnvironment.GetGame().GetRoomManager().GetEventManager().QueueRemoveEvent(Room.RoomData);
            Room.RoomData.Event = null;
        }
    }
}
