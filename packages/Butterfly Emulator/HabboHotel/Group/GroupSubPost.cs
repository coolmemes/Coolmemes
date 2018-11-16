using Butterfly.Messages;
using ButterStorm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Group
{
    class GroupSubPost
    {
        internal uint Id;
        internal uint SubPostId;
        internal string Issue;
        internal string Message;
        internal uint OwnerId;
        private string _username;
        private string _look;
        internal double UnixCreated;
        internal uint GroupId;
        internal int HiddenMode;
        internal string HiddenBy;
        internal int PostOrderID;

        internal string Username
        {
            get
            {
                if (_username == "")
                    _username = UsersCache.getUsernameById(OwnerId);

                return _username;
            }
        }

        internal string Look
        {
            get
            {
                if (_look == "")
                    _look = UsersCache.getHabboCache(OwnerId).Look;

                return _look;
            }
        }

        internal GroupSubPost(uint id, uint subpostid, string issue, string message, uint ownerid, string username, string look, double unixcreated, uint groupid, int hiddenmode, int postorderid, string hiddenby)
        {
            this.Id = id;
            this.SubPostId = subpostid;
            this.Issue = issue;
            this.Message = message;
            this.OwnerId = ownerid;
            this._username = username;
            this._look = look;
            this.UnixCreated = unixcreated;
            this.GroupId = groupid;
            this.HiddenMode = hiddenmode;
            this.HiddenBy = hiddenby;
            this.PostOrderID = postorderid;
        }

        internal void SerializeSubPost(ServerMessage Message)
        {
            Message.AppendUInt(this.Id);
            Message.AppendInt32(this.PostOrderID);
            Message.AppendUInt(this.OwnerId);
            Message.AppendString(this.Username);
            Message.AppendString(this.Look);
            Message.AppendInt32((int)(OtanixEnvironment.GetUnixTimestamp() - this.UnixCreated));
            Message.AppendString(this.Message);
            Message.AppendByted(this.HiddenMode);
            Message.AppendInt32(0);
            Message.AppendString(this.HiddenBy);
            Message.AppendUInt(this.Id);
            Message.AppendUInt(OtanixEnvironment.GetGame().GetGroup().GetTotalPosts(this.OwnerId));
        }
    }
}
