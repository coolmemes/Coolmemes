using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Butterfly.HabboHotel.Items;
using System.Drawing;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;

namespace Butterfly.HabboHotel.Rooms.Wired
{
    class ConditionHandler
    {
        private Dictionary<Point, List<IWiredCondition>> roomMatrix;

        public ConditionHandler()
        {
            roomMatrix = new Dictionary<Point, List<IWiredCondition>>();
        }

        public void AddConditionToTile(Point coord, IWiredCondition cond)
        {
            if(!roomMatrix.ContainsKey(coord))
            {
                List<IWiredCondition> iwc = new List<IWiredCondition>(1) { cond };
                roomMatrix.Add(coord, iwc);
            }
            else
            {
                List<IWiredCondition> iwc = roomMatrix[coord];
                if (!iwc.Contains(cond))
                    iwc.Add(cond);
            }
        }

        public void RemoveConditionToTile(Point coord, IWiredCondition cond)
        {
            if (roomMatrix.ContainsKey(coord))
            {
                List<IWiredCondition> iwc = roomMatrix[coord];
                if (iwc.Contains(cond))
                    iwc.Remove(cond);

                if (iwc.Count == 0)
                    roomMatrix.Remove(coord);
            }
        }

        internal bool AllowsHandling(Point coordinate, RoomUser user)
        {
            if (!roomMatrix.ContainsKey(coordinate))
                return true;

            return roomMatrix[coordinate].All(coond => coond.AllowsExecution(user));
        }
    }
}
