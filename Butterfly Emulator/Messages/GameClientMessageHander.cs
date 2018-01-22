using Butterfly.HabboHotel.GameClients;
using System;
using Butterfly.HabboHotel.Rooms;
using Butterfly.Messages.StaticMessageHandlers;
using ButterStorm;
using Butterfly.Core;

namespace Butterfly.Messages
{
    partial class GameClientMessageHandler
    {
        private GameClient Session;
        private ClientMessage Request;
        private PacketsUserLogs packetsuserlogs;
        private readonly ServerMessage Response;

        internal GameClientMessageHandler(GameClient Session)
        {
            this.Session = Session;
            this.packetsuserlogs = new PacketsUserLogs();
            this.Response = new ServerMessage();
        }

        internal ServerMessage GetResponse()
        {
            return Response;
        }

        internal PacketsUserLogs GetPacketsUserLogs()
        {
            return packetsuserlogs;
        }

        internal void Destroy()
        {
            Session = null;
            packetsuserlogs.Destroy();
        }

        internal void HandleRequest(ClientMessage request)
        {
            if (Session == null)
                return;

            Request = request;
            StaticClientMessageHandler.HandlePacket(this, request);
        }

        internal void SendResponse()
        {
            if (Response != null && Response.Id > 0 && Session != null && Session.GetConnection() != null)
            {
                Session.GetConnection().SendData(Response.GetBytes());

                if (Session.PacketSaverEnable)
                    Logging.LogPacketData("UserName: " + Session.GetHabbo().Username + ": " + Response.ToString());
            }
        }

        internal void SendResponseWithOwnerParam()
        {
            if (Response != null && Response.Id > 0 && Session.GetConnection() != null)
            {
                Response.AppendBoolean(Session.GetHabbo().CurrentRoom.CheckRights(Session, true));
                Session.GetConnection().SendData(Response.GetBytes());
            }
        }
    }
}
