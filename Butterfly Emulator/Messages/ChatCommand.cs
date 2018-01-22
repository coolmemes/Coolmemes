using System.Linq;
using Butterfly.HabboHotel.GameClients;
using System;
using System.Collections.Generic;

namespace Butterfly.Messages
{
    struct ChatCommand
    {
        internal readonly string Command;
        internal readonly int CommandId;
        internal readonly int AuthorizedRanks;
        internal readonly string Description;

        internal ChatCommand(string _command, int _commandId, int _ranks, string _description)
        {
            this.Command = _command;
            this.CommandId = _commandId;
            this.AuthorizedRanks = _ranks;
            this.Description = _description;
        }

        internal bool UserGotAuthorization(GameClient session)
        {
            if (this.AuthorizedRanks == -1)
            {
                if (session.GetHabbo().CurrentRoom.CheckRights(session, false))
                    return true;
            }
            else if (this.AuthorizedRanks == -2)
            {
                if (session.GetHabbo().CurrentRoom.CheckRights(session, true))
                    return true;
            }
            else if (this.AuthorizedRanks <= (int)session.GetHabbo().Rank)
            {
                return true;
            }

            return false;
        }
    }
}
