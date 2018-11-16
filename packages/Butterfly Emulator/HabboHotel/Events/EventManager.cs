using System.Collections.Generic;
using Butterfly.HabboHotel.Rooms;
using System.Collections;
using System.Linq;
using System;
using System.Diagnostics;
using ButterStorm;

namespace Butterfly.HabboHotel.Events
{
    class EventManager
    {
        #region Fields
        private readonly List<RoomData> events;
        private IOrderedEnumerable<RoomData> orderedEventRooms;
        private UInt32 lastEventId;

        private readonly Queue addQueue;
        private readonly Queue removeQueue;
        private readonly Queue updateQueue;
        #endregion

        #region Return values
        internal UInt32 eventsCount
        {
            get
            {
                return lastEventId++;
            }
        }

        internal List<RoomData> GetEventRooms()
        {
            return orderedEventRooms.ToList();
        }

        internal Boolean ContainsEventRoomId(RoomData data)
        {
            if (events.Contains(data))
                return true;
            return false;
        }
        #endregion

        #region Constructor
        public EventManager()
        {
            events = new List<RoomData>();
            orderedEventRooms = events.OrderByDescending(t => t.UsersNow);
            addQueue = new Queue();
            removeQueue = new Queue();
            updateQueue = new Queue();
            moduleWatch = new Stopwatch();
            moduleWatch.Start();
        }
        #endregion

        #region Threading

        internal void onCycle()
        {
            workRemoveQueue();
            workAddQueue();
            CleanEvents();

            SortCollection();
        }

        private Stopwatch moduleWatch;
        private void CleanEvents()
        {
            if (moduleWatch.ElapsedMilliseconds > 60000) // mayor que 1 minuto
            {
                moduleWatch.Restart();

                foreach (RoomData data in events)
                {
                    if (data.Event != null)
                    {
                        if ((DateTime.Now - data.Event.StartTime).Minutes > 120)
                            data.Event.EndEvent();
                    }
                    else
                    {
                        QueueRemoveEvent(data);
                    }
                }
            }
        }

        private void SortCollection()
        {
            orderedEventRooms = events.Take(40).OrderByDescending(t => t.UsersNow);
        }

        private void workAddQueue()
        {
            if (addQueue.Count > 0)
            {
                lock (addQueue.SyncRoot)
                {
                    while (addQueue.Count > 0)
                    {
                        var data = (RoomData)addQueue.Dequeue();
                        if (!events.Contains(data))
                            events.Add(data);
                    }
                }
            }
        }

        private void workRemoveQueue()
        {
            if (removeQueue.Count > 0)
            {
                lock (removeQueue.SyncRoot)
                {
                    while (removeQueue.Count > 0)
                    {
                        var data = (RoomData)removeQueue.Dequeue();
                        events.Remove(data);
                    }
                }
            }
        }
        #endregion

        #region Methods
        internal List<RoomData> GetFourRecentEvents(int count)
        {
            try
            {
                return events.OrderByDescending(t => t.Event.StartTime).Take(count).ToList();
            }
            catch
            {
                return null;
            }
        }

        internal void QueueAddEvent(RoomData data)
        {
            lock (addQueue.SyncRoot)
            {
                addQueue.Enqueue(data);
            }
        }

        internal void QueueRemoveEvent(RoomData data)
        {
            lock (removeQueue.SyncRoot)
            {
                removeQueue.Enqueue(data);
            }
        }
        #endregion
    }
}
