using Butterfly.Messages;
using ButterStorm;

namespace Butterfly.HabboHotel.Users.Messenger
{
    struct SearchResult
    {
        internal uint userID;
        internal string username;
        internal string motto;
        internal string look;
        internal string last_online;

        public SearchResult(uint userID, string username, string motto, string look, string last_online)
        {
            this.userID = userID;
            this.username = username;
            this.motto = motto;
            this.look = look;
            this.last_online = last_online;
        }

        internal void Searialize(ServerMessage reply)
        {
            reply.AppendUInt(userID);
            reply.AppendString(username);
            reply.AppendString(motto);

            var Online = (OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(userID) != null);

            reply.AppendBoolean(Online);
          
            reply.AppendBoolean(false);

            reply.AppendString(string.Empty);
            reply.AppendInt32(0);
            reply.AppendString(look);
            reply.AppendString(last_online);
        }
    }
}
