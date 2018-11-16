using Butterfly.HabboHotel.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Navigators.RoomQueue
{
    class RoomQueueManager
    {
        private Dictionary<uint, RoomQueue> RoomsInQueue;

        public RoomQueueManager()
        {
            RoomsInQueue = new Dictionary<uint, RoomQueue>();
        }
    
        public RoomQueue CreateRoomQueue(Room Room)
        {
            if (!RoomsInQueue.ContainsKey(Room.Id))
            {
                RoomQueue queue = new RoomQueue(Room);
                RoomsInQueue.Add(Room.Id, queue);

                return queue;
            }
            else
            {
                return GetRoomQueue(Room.Id);
            }
        }

        public void RemoveRoomQueue(uint RoomId)
        {
            if (RoomsInQueue.ContainsKey(RoomId))
                RoomsInQueue.Remove(RoomId);
        }

        public RoomQueue GetRoomQueue(uint RoomId)
        {
            if (RoomsInQueue.ContainsKey(RoomId))
                return RoomsInQueue[RoomId];

            return null;
        }
    }
}
