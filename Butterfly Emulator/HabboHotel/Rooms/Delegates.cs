using System;

namespace Butterfly.HabboHotel.Rooms
{
    public delegate void RoomEventDelegate(object sender, EventArgs e);
    public delegate bool RoomUserSaysDelegate(object sender, UserSaysArgs e);
    public delegate void TeamScoreChangedDelegate(object sender, TeamScoreChangedArgs e);
    public delegate void UserWalksFurniDelegate(object sender, UserWalksOnArgs e);
}
