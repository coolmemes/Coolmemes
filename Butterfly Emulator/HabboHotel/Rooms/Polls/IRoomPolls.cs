using Butterfly.HabboHotel.GameClients;
using Butterfly.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Polls
{
    interface IRoomPolls
    {
        PollType GetPollType();
        bool LoadQuestionary(uint RoomId);
        ServerMessage SerializePoll();
        ServerMessage SaveInformation(uint UserId, string Answers);
        ServerMessage ClearInformation();
    }
}