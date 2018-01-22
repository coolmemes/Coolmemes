using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;
using Butterfly.Messages;
using ButterStorm;
using HabboEvents;

namespace Butterfly.HabboHotel.Users.Messenger
{
    class MessengerBuddy
    {
        #region Fields
        private readonly uint UserId;
        private string mUsername;
        private string mLook;
        private string mMotto;
        private bool mIsOnline;
        private Room mRoom;
        #endregion

        #region Return values
        internal uint Id
        {
            get
            {
                return UserId;
            }
        }

        internal string Username
        {
            get
            {
                return mUsername;
            }
            set
            {
                mUsername = value;
            }
        }

        internal string Look
        {
            get
            {
                return mLook;
            }
            set
            {
                mLook = value;
            }
        }

        internal string Motto
        {
            get
            {
                return mMotto;
            }
        }

        internal bool IsOnline
        {
            get
            {
                return mIsOnline;
            }
        }

        internal bool InRoom
        {
            get
            {
                return (mRoom != null);
            }
        }

        internal Room currentRoom
        {
            get
            {
                return mRoom;
            }
        }

        internal void UpdateUserSettings()
        {
            GameClient client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (client != null && client.GetHabbo() != null && client.GetHabbo().GetMessenger() != null && !client.GetHabbo().GetMessenger().AppearOffline)
            {
                mIsOnline = true;
                mLook = client.GetHabbo().Look;
                mMotto = client.GetHabbo().Motto;
                mRoom = client.GetHabbo().CurrentRoom;
            }
            else if (UserId == EmuSettings.CHAT_USER_ID)
            {
                mIsOnline = true;
                mRoom = null;
            }
            else
            {
                mIsOnline = false;
                mLook = "";
                mMotto = "";
                mRoom = null;
            }
        }

        internal void FriendConnectAlert(string Username)
        {
            /*GameClient client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (client == null)
                return;

            ServerMessage messageError = new ServerMessage(Outgoing.CustomAlert);
            messageError.AppendString("furni_placement_error");
            messageError.AppendInt32(1);
            messageError.AppendString(Username);
            messageError.AppendString("${notifications.text.friend.online}");
            client.SendMessage(messageError);*/
        }
        #endregion

        #region Constructor
        internal MessengerBuddy(uint UserId, string pUsername, string pLook, string pMotto)
        {
            this.UserId = UserId;
            this.mUsername = pUsername;
            this.mLook = pLook;
            this.mMotto = pMotto;
        }
        #endregion

        #region Methods
        internal void Serialize(ServerMessage reply, Habbo Habbo)
        {
            reply.AppendUInt(UserId); // avatar = int32 > 0, senão <= 0 (chat grupo)
            reply.AppendString(mUsername); // do grupo ou avatar
            reply.AppendInt32(1); // ??
            reply.AppendBoolean(IsOnline); // showLook
            reply.AppendBoolean(Habbo.FollowEnable ? InRoom : false); // canFollow
            reply.AppendString(Look); // se for grupo, badgecode, senão, o avatara do usuario
            reply.AppendInt32(0); // categoryid
            reply.AppendString(Motto);
            reply.AppendString(string.Empty); // avatar real name, idk
            reply.AppendString(string.Empty); // sem uso
            reply.AppendBoolean(true); // offline messaging enabled //Neri
            reply.AppendBoolean(false); // idk
            reply.AppendBoolean(false); // Has Pocket Habbo That failed app that you can out-game talk with your friend in-game
            if (Habbo.GetRelationshipComposer().LoveRelation.ContainsKey(UserId))
                reply.AppendShort(1);
            else if (Habbo.GetRelationshipComposer().FriendRelation.ContainsKey(UserId))
                reply.AppendShort(2);
            else if (Habbo.GetRelationshipComposer().DieRelation.ContainsKey(UserId))
                reply.AppendShort(3);
            else
                reply.AppendShort(0);
        }
        #endregion
    }
}
