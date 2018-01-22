using System.Collections.Generic;
using Butterfly.HabboHotel.Items;

namespace Butterfly.HabboHotel.Rooms.Games
{
    public enum Team
    {
        none = 0,
        red = 1,
        green = 2,
        blue = 3,
        yellow = 4
    }

    public class TeamManager
    {
        private List<RoomUser>[] Teams;

        public TeamManager()
        {
            Teams = new List<RoomUser>[5];
            for (int i = 0; i < 5; i++)
                Teams[i] = new List<RoomUser>();
        }

        public bool EmptyTeam(int TeamId)
        {
            return Teams[TeamId].Count == 0;
        }

        public bool CanEnterOnTeam(Team t)
        {
            int TeamId = (int)t;
            return Teams[TeamId].Count < 5;
        }

        public void AddUser(RoomUser user)
        {
            int TeamId = (int)user.team;
            Teams[TeamId].Add(user);

            Room room = user.GetClient().GetHabbo().CurrentRoom;
            List<RoomItem> Items = room.GetGameManager().GetTeamItems(user.team);
            foreach (RoomItem Item in Items)
            {
                if (room.GetGameManager().IsGate(Item.GetBaseItem().InteractionType))
                {
                    Item.ExtraData = Teams[TeamId].Count.ToString();
                    Item.UpdateState();
                }
            }
        }

        public void OnUserLeave(RoomUser user)
        {
            int TeamId = (int)user.team;
            Teams[TeamId].Remove(user);

            Room room = user.GetClient().GetHabbo().CurrentRoom;
            List<RoomItem> Items = room.GetGameManager().GetTeamItems(user.team);
            foreach (RoomItem Item in Items)
            {
                if (room.GetGameManager().IsGate(Item.GetBaseItem().InteractionType))
                {
                    Item.ExtraData = Teams[TeamId].Count.ToString();
                    Item.UpdateState();
                }
            }
        }
    }
}