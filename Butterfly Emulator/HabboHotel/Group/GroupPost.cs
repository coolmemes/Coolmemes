using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Group
{
    class GroupPost
    {
        internal uint Id;
        internal uint SubPostId;
        internal string Issue;
        internal string Message;
        internal uint OwnerId;
        private string _username;
        internal double UnixCreated;
        internal int SubPostsCount;
        private bool SubPostsLoaded;
        internal Dictionary<uint, GroupSubPost> subPosts;
        internal uint GroupId;
        internal bool Closed;
        internal bool Disabled;
        internal int HiddenMode;
        internal string HiddenBy;
        internal uint LastOwnerId;
        internal string LastOwnerName;
        internal double LastUnix;

        internal string Username
        {
            get
            {
                if (_username == "")
                    _username = UsersCache.getUsernameById(OwnerId);

                return _username;
            }
        }

        internal Dictionary<uint, GroupSubPost> SubPosts
        {
            get
            {
                if (!SubPostsLoaded)
                    LoadSubPosts();

                return subPosts;
            }
        }

        internal GroupPost(uint id, uint subpostid, string issue, string message, uint ownerid, string username, double unixcreated, uint groupid, bool closed, bool disabled, int hiddenmode, int subforumcount, string hiddenby, bool loadlastest)
        {
            this.Id = id;
            this.SubPostId = subpostid;
            this.Issue = issue;
            this.Message = message;
            this.OwnerId = ownerid;
            this._username = username;
            this.UnixCreated = unixcreated;
            this.GroupId = groupid;
            this.Closed = closed;
            this.Disabled = disabled;
            this.HiddenMode = hiddenmode;
            this.HiddenBy = hiddenby;
            this.LastOwnerId = 0;
            this.LastOwnerName = "";
            this.LastUnix = OtanixEnvironment.GetUnixTimestamp();

            this.subPosts = new Dictionary<uint, GroupSubPost>();
            this.subPosts.Add(this.Id, new GroupSubPost(this.Id, this.SubPostId, this.Issue, this.Message, this.OwnerId, this.Username, "", this.UnixCreated, this.GroupId, this.HiddenMode, 0, this.HiddenBy));
            this.SubPostsCount = subforumcount;

            if (loadlastest)
                this.LoadLast();
        }

        internal void LoadSubPosts()
        {
            this.SubPostsLoaded = true;

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT id, sub_post_id, message, owner_id, date_created, groupid, hiddenmode, hiddenby FROM groups_forums_boards WHERE groupid = '" + GroupId + "' AND sub_post_id = '" + this.Id + "'");
                DataTable dTable = dbClient.getTable();
                int lastID = 1;

                foreach (DataRow dRow in dTable.Rows)
                {
                    GroupSubPost post = new GroupSubPost(Convert.ToUInt32(dRow["id"]), Convert.ToUInt32(dRow["sub_post_id"]), "", (string)dRow["message"], Convert.ToUInt32(dRow["owner_id"]), "", "", Convert.ToDouble(dRow["date_created"]), Convert.ToUInt32(dRow["groupid"]), Convert.ToInt32(dRow["hiddenmode"]), lastID, (string)dRow["hiddenby"]);
                    if(!this.subPosts.ContainsKey(post.Id))
                        this.subPosts.Add(post.Id, post);
                    lastID++;
                }
            }
        }

        internal void LoadLast()
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT owner_id, date_created FROM groups_forums_boards WHERE groupid = '" + GroupId + "' AND sub_post_id = '" + this.Id + "' OR id = '" + this.Id + "' ORDER BY id DESC LIMIT 1");
                DataRow dRow = dbClient.getRow();

                if (dRow != null)
                {
                    this.LastOwnerId = Convert.ToUInt32(dRow["owner_id"]);
                    this.LastOwnerName = UsersCache.getUsernameById(this.LastOwnerId);
                    this.LastUnix = Convert.ToDouble(dRow["date_created"]);
                }
            }
        }

        internal void Serialize(ServerMessage Message)
        {
            Message.AppendUInt(this.Id);
            Message.AppendUInt(this.OwnerId);
            Message.AppendString(this.Username);
            Message.AppendString(this.Issue);
            Message.AppendBoolean(this.Disabled);
            Message.AppendBoolean(this.Closed);
            Message.AppendInt32((Int32)(OtanixEnvironment.GetUnixTimestamp() - this.UnixCreated)); // tiempo desde creación
            Message.AppendInt32(this.SubPostsCount); // mensajes count
            Message.AppendInt32(0); // no leídos
            Message.AppendInt32(1);
            Message.AppendUInt(this.LastOwnerId); // ultimo mensaje OwnerID
            Message.AppendString(this.LastOwnerName); // último mensaje Username
            Message.AppendInt32((int)(OtanixEnvironment.GetUnixTimestamp() - this.LastUnix)); // último mensaje hace:
            Message.AppendByted(this.HiddenMode);
            Message.AppendInt32(0);
            Message.AppendString(this.HiddenBy);
            Message.AppendUInt(this.Id);
            // Message.AppendUInt(OtanixEnvironment.GetGame().GetGroup().GetTotalPosts(this.OwnerId));
        }
    }
}
