using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Group
{
    class GroupForum
    {
        internal uint GroupId;
        internal int CanReadForum;
        internal int CanWriteForum;
        internal int CanCreateForum;
        internal int CanModerateForum;
        private bool PostsLoaded;
        private Dictionary<uint, GroupPost> _posts;
        internal Int32 totalmessages;
        private Boolean totalMessagesLoaded;
        internal uint LastOwnerId;
        internal string LastOwnerName;
        internal double LastUnix;

        internal GroupForum(uint groupid, int canreadforum, int canwriteforum, int cancreateforum, int canmoderateforum, bool loadlast)
        {
            this.GroupId = groupid;
            this.CanReadForum = canreadforum;
            this.CanWriteForum = canwriteforum;
            this.CanCreateForum = cancreateforum;
            this.CanModerateForum = canmoderateforum;
            this.PostsLoaded = false;
            this.LastOwnerId = 0;
            this.LastOwnerName = "";
            this.LastUnix = 0;

            if(loadlast)
                this.LoadLast();
        }

        internal Dictionary<uint, GroupPost> Posts
        {
            get
            {
                if (!PostsLoaded)
                    LoadPosts();

                return _posts;
            }
        }

        internal Int32 TotalMessages
        {
            get
            {
                if (!totalMessagesLoaded)
                {
                    totalMessagesLoaded = true;
                    using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        dbClient.setQuery("SELECT COUNT(*) FROM groups_forums_boards WHERE groupid = '" + this.GroupId + "'");
                        totalmessages = dbClient.getInteger();
                    }
                }

                return totalmessages;
            }
        }

        internal void LoadPosts()
        {
            this._posts = new Dictionary<uint, GroupPost>();
            this.PostsLoaded = true;

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT * FROM groups_forums_boards WHERE groupid = '" + GroupId + "' AND sub_post_id = '0'");
                DataTable dTable = dbClient.getTable();

                foreach (DataRow dRow in dTable.Rows)
                {
                    dbClient.setQuery("SELECT COUNT(*) FROM groups_forums_boards WHERE groupid = '" + GroupId + "' AND sub_post_id = '" + Convert.ToUInt32(dRow["id"]) + "'");
                    int subpostscount = dbClient.getInteger() + 1;

                    GroupPost post = new GroupPost(Convert.ToUInt32(dRow["id"]), Convert.ToUInt32(dRow["sub_post_id"]), (string)dRow["issue"], (string)dRow["message"], Convert.ToUInt32(dRow["owner_id"]), "", Convert.ToDouble(dRow["date_created"]), Convert.ToUInt32(dRow["groupid"]), OtanixEnvironment.EnumToBool((string)dRow["closed"]), OtanixEnvironment.EnumToBool((string)dRow["disabled"]), Convert.ToInt32(dRow["hiddenmode"]), subpostscount, (string)dRow["hiddenby"], true);
                    if(!this._posts.ContainsKey(post.Id))
                        this._posts.Add(post.Id, post);
                }
            }
        }

        internal void LoadLast()
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT owner_id, date_created FROM groups_forums_boards WHERE groupid = '" + GroupId + "' ORDER BY id DESC LIMIT 1");
                DataRow dRow = dbClient.getRow();

                if (dRow != null)
                {
                    this.LastOwnerId = Convert.ToUInt32(dRow["owner_id"]);
                    this.LastOwnerName = UsersCache.getUsernameById(this.LastOwnerId);
                    this.LastUnix = Convert.ToDouble(dRow["date_created"]);
                }
            }
        }

        internal ServerMessage SerializeForum(GroupItem Group, Boolean NullForum, UInt32 UserID)
        {
            var ForumInformation = new ServerMessage(Outgoing.ForumInformation);
            ForumInformation.AppendUInt(GroupId);

            if (NullForum == false)
            {
                ForumInformation.AppendString(Group.Name);
                ForumInformation.AppendString(Group.Description);
                ForumInformation.AppendString(Group.GroupImage);
                ForumInformation.AppendInt32(Group.Forum.Posts.Count);
                ForumInformation.AppendInt32(0);
                ForumInformation.AppendInt32(0);
                ForumInformation.AppendInt32(0);
                ForumInformation.AppendInt32(0);
                ForumInformation.AppendInt32(0);
                ForumInformation.AppendString("unknown");
                ForumInformation.AppendInt32(0); // forum ID
                ForumInformation.AppendInt32(CanReadForum);
                ForumInformation.AppendInt32(CanWriteForum);
                ForumInformation.AppendInt32(CanCreateForum);
                ForumInformation.AppendInt32(CanModerateForum);
                ForumInformation.AppendString("");
                ForumInformation.AppendString("");
                ForumInformation.AppendString("");
                ForumInformation.AppendString((Group.Id != UserID && !Group.Admins.ContainsKey(UserID)) ? "not_admin" : "");
                ForumInformation.AppendString("");
                ForumInformation.AppendBoolean(true);
                ForumInformation.AppendBoolean(false);
            }
            else
            {
                ForumInformation.AppendString("");
                ForumInformation.AppendString("");
                ForumInformation.AppendString("");
                ForumInformation.AppendInt32(0);
                ForumInformation.AppendInt32(0);
                ForumInformation.AppendInt32(0);
                ForumInformation.AppendInt32(0);
                ForumInformation.AppendInt32(0);
                ForumInformation.AppendInt32(0);
                ForumInformation.AppendString("");
                ForumInformation.AppendInt32(0); // forum ID
                ForumInformation.AppendInt32(0);
                ForumInformation.AppendInt32(0);
                ForumInformation.AppendInt32(0);
                ForumInformation.AppendInt32(0);
                ForumInformation.AppendString("not_admin");
                ForumInformation.AppendString("not_admin");
                ForumInformation.AppendString("not_admin");
                ForumInformation.AppendString("not_admin");
                ForumInformation.AppendString("not_admin");
                ForumInformation.AppendBoolean(false);
                ForumInformation.AppendBoolean(false);

            }

            return ForumInformation;
        }

        internal void Destroy()
        {
            _posts.Clear();
            _posts = null;
        }
    }
}
