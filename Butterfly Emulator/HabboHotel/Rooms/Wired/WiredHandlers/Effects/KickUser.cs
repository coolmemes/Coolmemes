using System;
using System.Collections.Generic;
using System.Linq;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using System.Collections;
using Database_Manager.Database.Session_Details.Interfaces;
using System.Data;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.Messages;
using HabboEvents;
using Butterfly.Util;


namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class KickUser : IWiredTrigger, IWiredCycleable, IWiredEffect
    {
        private readonly WiredHandler handler;
        private RoomItem itemID;
        private int cycles;
        private readonly Queue delayedUsers;
        private bool disposed;

        private string message;

        public KickUser(string message, WiredHandler handler, RoomItem itemID)
        {
            this.itemID = itemID;
            this.handler = handler;
            this.message = message;
            this.cycles = 0;
            this.disposed = false;
            this.delayedUsers = new Queue();
        }

        public string Message
        {
            get
            {
                return message;
            }
        }

        public bool OnCycle()
        {
            cycles++;
            if (cycles > 5)
            {
                if (delayedUsers.Count > 0)
                {
                    lock (delayedUsers.SyncRoot)
                    {
                        while (delayedUsers.Count > 0)
                        {
                            var user = (RoomUser)delayedUsers.Dequeue();
                            onKickUser(user);
                        }
                    }
                }
                return false;
            }

            return true;
        }

        public void Handle(RoomUser user, Team team, RoomItem item)
        {
            //InteractorGenericSwitch.DoAnimation(itemID);

            if (user == null || user.IsBot || user.GetClient() == null || disposed || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().Rank >= 3)
                return;

            var servermsg = new ServerMessage(Outgoing.Whisp);
            servermsg.AppendInt32(user.VirtualId);
            if (user.HabboId == itemID.GetRoom().RoomData.OwnerId)
                servermsg.AppendString("Wired kick exception: Room owner");
            else
                servermsg.AppendString(message);
            servermsg.AppendInt32(0);
            servermsg.AppendInt32(0); // color
            servermsg.AppendInt32(0);
            servermsg.AppendInt32(-1);
            user.GetClient().SendMessage(servermsg);

            if (user.HabboId == itemID.GetRoom().RoomData.OwnerId)
                return;

            user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(4);

            lock (delayedUsers.SyncRoot)
            {
                delayedUsers.Enqueue(user);
            }
            handler.RequestCycle(this);
        }

        private void onKickUser(RoomUser user)
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().CurrentRoom == null || user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent() == null || user.GetClient().GetHabbo().CurrentRoom.GetRoomUserManager() == null)
                return;

            user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(0);
            user.GetClient().GetHabbo().CurrentRoom.GetRoomUserManager().RemoveUserFromRoom(user.GetClient(), true, false);
        }

        public void Dispose()
        {
            disposed = true;
            message = null;
            if (delayedUsers != null)
                delayedUsers.Clear();
        }

        public void ResetTimer()
        {

        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = message.ToString() + ";;false";
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + itemID.Id + "', @data" + itemID.Id + ", @to_item" + itemID.Id + ", @original_location" + itemID.Id + ")");
            wiredInserts.AddParameter("data" + itemID.Id, wired_data);
            wiredInserts.AddParameter("to_item" + itemID.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + itemID.Id, wired_original_location);
        }

        public bool Disposed()
        {
            return disposed;
        }
    }
}