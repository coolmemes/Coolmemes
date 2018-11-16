using System;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;

namespace Butterfly.HabboHotel.RoomBots
{
    abstract class BotAI
    {
        internal Int32 BaseId;
        private RoomUser roomUser;
        private Room room;

        internal void Init(Int32 pBaseId, Int32 pRoomUserId, UInt32 pRoomId, RoomUser user, Room room)
        {
            this.BaseId = pBaseId;
            this.roomUser = user;
            this.room = room;
        }

        internal Room GetRoom()
        {
            return room;
        }

        internal RoomUser GetRoomUser()
        {
            return roomUser;
        }

        internal RoomBot GetBotData()
        {
            var User = GetRoomUser();
            if (User == null)
                return null;
            else
                return GetRoomUser().BotData;
        }

        internal abstract void OnSelfEnterRoom();
        internal abstract void OnSelfLeaveRoom(bool Kicked);
        internal abstract void OnUserEnterRoom(RoomUser User);
        internal abstract void OnUserLeaveRoom(GameClient Client);
        internal abstract void OnUserSay(RoomUser User, string Message);
        internal abstract void OnUserShout(RoomUser User, string Message);
        internal abstract void OnTimerTick();
    }
}
