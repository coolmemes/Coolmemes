using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;
using Butterfly.HabboHotel.Users;
using Butterfly.Messages;
using ButterStorm;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Navigators.RoomQueue
{
    class RoomQueue
    {
        public Room Room;
        public List<uint> UserQueue;

        public RoomQueue(Room Room)
        {
            this.Room = Room;
            this.UserQueue = new List<uint>();
        }

        public void AddUserToQueue(GameClient Session)
        {
            if (UserQueue.Contains(Session.GetHabbo().Id))
                return;
            
            UserQueue.Add(Session.GetHabbo().Id);
            Session.GetHabbo().roomIdQueue = this.Room.Id;

            ServerMessage Message = new ServerMessage(Outgoing.RoomQueueMessageComposer);
            Message.AppendUInt(2); // foreach
            Message.AppendString("visitors");
            Message.AppendInt32(2); // First is Main Id
            Message.AppendInt32(1); // foreach
            Message.AppendString("visitors");
            Message.AppendInt32(UserQueue.Count - 1);
            Message.AppendString("spectators");
            Message.AppendInt32(1);
            Message.AppendInt32(1);
            Message.AppendString("spectators");
            Message.AppendInt32(0);
            Session.SendMessage(Message);
        }

        public void RemoveUserToQueue(uint UserId)
        {
            if (!UserQueue.Contains(UserId))
                return;

            UserQueue.Remove(UserId);

            if (UserQueue.Count == 0)
            {
                OtanixEnvironment.GetGame().GetRoomQueueManager().RemoveRoomQueue(Room.Id);
                return;
            }

            UpdateQueue();
        }

        public void FirstUserOnRoom()
        {
            if (UserQueue.Count <= 0)
            {
                OtanixEnvironment.GetGame().GetRoomQueueManager().RemoveRoomQueue(Room.Id);
                return;
            }

            uint UserId = UserQueue.First();
            UserQueue.RemoveAt(0);

            GameClient Session = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (Session != null)
            {
                Session.GetHabbo().roomIdQueue = 0;
                Session.GetHabbo().goToQueuedRoom = Room.Id;
                Session.GetMessageHandler().enterOnRoom3(Room);
                UpdateQueue();
            }
            else
            {
                FirstUserOnRoom();
            }
        }

        public void UpdateQueue()
        {
            uint Position = 0;
            foreach (uint UserId in UserQueue)
            {
                GameClient Session = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
                if (Session != null)
                {
                    ServerMessage Message = new ServerMessage(Outgoing.RoomQueueMessageComposer);
                    Message.AppendUInt(2);
                    Message.AppendString("visitors");
                    Message.AppendInt32(2);
                    Message.AppendInt32(1);
                    Message.AppendString("visitors");
                    Message.AppendUInt(Position);
                    Message.AppendString("spectators");
                    Message.AppendInt32(1);
                    Message.AppendInt32(1);
                    Message.AppendString("spectators");
                    Message.AppendInt32(0);
                    Session.SendMessage(Message);
                    Session.SendMessage(Message);
                }
                else
                {
                    RemoveUserToQueue(UserId);
                    continue;
                }

                Position++;
            }
        }
    }
}
