using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Net;
using System.Net.Sockets;

using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;
using Butterfly.Messages;
using Butterfly.Core;

using Database_Manager.Database.Session_Details.Interfaces;
using ButterStorm;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Users;
using HabboEvents;

namespace Butterfly.Net
{
    class MusSocket
    {
        /// <summary>
        /// Socket de la conexión MUS
        /// </summary>
        internal Socket msSocket;

        /// <summary>
        /// Creamos la conexión MUS
        /// </summary>
        /// <param name="_musPort">Puerto de conexión</param>
        internal MusSocket(int musPort)
        {
            try
            {
                msSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                msSocket.Bind(new IPEndPoint(IPAddress.Any, musPort));
                msSocket.Listen(0);
                msSocket.BeginAccept(OnEvent_NewConnection, msSocket);

                Logging.WriteLine("[Otanix] @ Soquete MUS -> PRONTO!");
            }
            catch (SocketException SE)
            {
                throw new Exception("[Otanix] @ Alerta de erro: Não foi possível configurar o soquete MUS:\n" + SE.Message);
            }
        }

        internal void OnEvent_NewConnection(IAsyncResult iAr)
        {
            try
            {
                Socket socket = ((Socket)iAr.AsyncState).EndAccept(iAr);
                MusConnection nC = new MusConnection(socket);
            }
            catch { }

            msSocket.BeginAccept(OnEvent_NewConnection, msSocket);
        }
    }

    class MusConnection
    {
        private Socket socket;
        private byte[] buffer = new byte[1024];

        internal MusConnection(Socket _socket)
        {
            socket = _socket;

            try
            {
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnEvent_RecieveData, socket);
            }
            catch
            {
                tryClose();
            }
        }

        internal void tryClose()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket.Dispose();
            }
            catch { }

            socket = null;
            buffer = null;
        }

        internal void OnEvent_RecieveData(IAsyncResult iAr)
        {
            try
            {
                int bytes = 0;

                try
                {
                    bytes = socket.EndReceive(iAr);
                }
                catch { tryClose(); return; }

                String data = Encoding.Default.GetString(buffer, 0, bytes);

                if (data.Length > 0)
                    processCommand(data);
            }
            catch { }

            tryClose();
        }

        private void processCommand(String data)
        {
            String header = data.Split(';')[0];

            switch(header.ToLower())
            {
                case "enterroom":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);
                        uint RoomID = uint.Parse(data.Split(';')[2]);

                        UsersCache.AddUserProvisionalRoom(HabboID, RoomID);

                        break;
                    }

                case "goroom":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);
                        uint RoomID = uint.Parse(data.Split(';')[2]);

                        GameClient client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(HabboID);
                        if (client == null || client.GetMessageHandler() == null)
                            return;

                        Room room = OtanixEnvironment.GetGame().GetRoomManager().LoadRoom(RoomID);
                        if (room == null)
                            return;

                        client.GetMessageHandler().enterOnRoom3(room);

                        break;
                    }

                case "getevents":
                    {
                        int count = int.Parse(data.Split(';')[1]);
                        StringBuilder strBuilder = new StringBuilder();
                        List<RoomData> rooms = OtanixEnvironment.GetGame().GetRoomManager().GetEventManager().GetFourRecentEvents(count);

                        foreach (RoomData room in rooms)
                        {
                            if (room != null && room.Event != null)
                                strBuilder.Append(room.Id + "" + (char)10 + "" + room.Event.Name + (char)10 + "" + room.UsersNow + "" + (char)13);
                        }

                        if (strBuilder.Length > 0)
                            strBuilder.Remove(strBuilder.Length - 1, 1);

                        sendCommand(strBuilder.ToString());

                        break;
                    }

                case "getactiverooms":
                    {
                        int count = int.Parse(data.Split(';')[1]);
                        StringBuilder strBuilder = new StringBuilder();
                        List<RoomData> rooms = OtanixEnvironment.GetGame().GetRoomManager().GetMostActiveRooms(count);

                        foreach (RoomData room in rooms)
                        {
                            if (room != null)
                                strBuilder.Append(room.Id + "" + (char)10 + "" + room.UsersNow + "" + (char)13);
                        }

                        if (strBuilder.Length > 0)
                            strBuilder.Remove(strBuilder.Length - 1, 1);

                        sendCommand(strBuilder.ToString());

                        break;
                    }

                case "getmotto":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);

                        Habbo habbo = UsersCache.getHabboCache(HabboID);
                        
                        if(habbo != null)
                            sendCommand(habbo.Motto.ToString());

                        break;
                    }

                case "updatemotto":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);
                        string Motto = data.Split(';')[2];

                        Habbo habbo = UsersCache.getHabboCache(HabboID);
                        if (habbo != null && habbo.GetClient() != null)
                        {
                            habbo.Motto = Motto;

                            ServerMessage UpdateMotto = new ServerMessage(Outgoing.UpdateUserInformation);
                            UpdateMotto.AppendInt32(-1);
                            UpdateMotto.AppendString(habbo.Look);
                            UpdateMotto.AppendString(habbo.Gender.ToLower());
                            UpdateMotto.AppendString(habbo.Motto);
                            UpdateMotto.AppendUInt(habbo.AchievementPoints);
                            habbo.GetClient().SendMessage(UpdateMotto);

                            if (habbo.CurrentRoom != null)
                            {
                                ServerMessage UpdateMottoInRoom = new ServerMessage(Outgoing.UpdateUserInformation);
                                UpdateMottoInRoom.AppendInt32(habbo.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id).VirtualId);
                                UpdateMottoInRoom.AppendString(habbo.Look);
                                UpdateMottoInRoom.AppendString(habbo.Gender.ToLower());
                                UpdateMottoInRoom.AppendString(habbo.Motto);
                                UpdateMottoInRoom.AppendUInt(habbo.AchievementPoints);
                                habbo.CurrentRoom.SendMessage(UpdateMottoInRoom);
                            }
                        }

                        break;
                    }

                case "getlook":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);

                        Habbo habbo = UsersCache.getHabboCache(HabboID);

                        if (habbo != null)
                            sendCommand(habbo.Look.ToString());

                        break;
                    }

                case "getrespects":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);

                        Habbo habbo = UsersCache.getHabboCache(HabboID);

                        if (habbo != null)
                            sendCommand(habbo.Respect.ToString());

                        break;
                    }

                case "getpetitionsdisable":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);

                        Habbo habbo = UsersCache.getHabboCache(HabboID);

                        if (habbo != null)
                            sendCommand(habbo.HasFriendRequestsDisabled.ToString());

                        break;
                    }

                case "updatepetitionsdisable":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);
                        bool petitionsDisable = OtanixEnvironment.EnumToBool(data.Split(';')[2]);

                        Habbo habbo = UsersCache.getHabboCache(HabboID);
                        if (habbo != null)
                        {
                            habbo.HasFriendRequestsDisabled = petitionsDisable;
                        }

                        break;
                    }

                case "gettradesdisable":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);

                        Habbo habbo = UsersCache.getHabboCache(HabboID);

                        if (habbo != null)
                            sendCommand(habbo.BlockTrade.ToString());

                        break;
                    }

                case "updatetradesdisable":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);
                        bool tradesDisable = OtanixEnvironment.EnumToBool(data.Split(';')[2]);

                        Habbo habbo = UsersCache.getHabboCache(HabboID);
                        if (habbo != null)
                        {
                            habbo.BlockTrade = tradesDisable;
                        }

                        break;
                    }

                case "getignoreroominvitations":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);

                        Habbo habbo = UsersCache.getHabboCache(HabboID);

                        if (habbo != null)
                            sendCommand(habbo.IgnoreRoomInvitations.ToString());

                        break;
                    }

                case "updateignoreroominvitations":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);
                        bool ignoreRoomInvitations = OtanixEnvironment.EnumToBool(data.Split(';')[2]);

                        Habbo habbo = UsersCache.getHabboCache(HabboID);
                        if (habbo != null)
                        {
                            habbo.IgnoreRoomInvitations = ignoreRoomInvitations;
                        }

                        break;
                    }

                case "getdontfocususers":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);

                        Habbo habbo = UsersCache.getHabboCache(HabboID);

                        if (habbo != null)
                            sendCommand(habbo.DontFocusUser.ToString());

                        break;
                    }

                case "updatedontfocususers":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);
                        bool dontFocusUsers = OtanixEnvironment.EnumToBool(data.Split(';')[2]);

                        Habbo habbo = UsersCache.getHabboCache(HabboID);
                        if (habbo != null)
                        {
                            habbo.DontFocusUser = dontFocusUsers;
                        }

                        break;
                    }

                case "getprefoldchat":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);

                        Habbo habbo = UsersCache.getHabboCache(HabboID);

                        if (habbo != null)
                            sendCommand(habbo.preferOldChat.ToString());

                        break;
                    }

                case "updateprefoldchat":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);
                        bool prefOldChat = OtanixEnvironment.EnumToBool(data.Split(';')[2]);

                        Habbo habbo = UsersCache.getHabboCache(HabboID);
                        if (habbo != null)
                        {
                            habbo.preferOldChat = prefOldChat;
                        }

                        break;
                    }

                case "getdiamonds":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);

                        Habbo habbo = UsersCache.getHabboCache(HabboID);

                        if (habbo != null)
                            sendCommand(habbo.Diamonds.ToString());

                        break;
                    }

                case "updatediamonds":
                    {
                        uint HabboID = uint.Parse(data.Split(';')[1]);
                        int Diamonds = int.Parse(data.Split(';')[2]);

                        Habbo habbo = UsersCache.getHabboCache(HabboID);
                        if (habbo != null)
                        {
                            if (Diamonds < 0)
                                habbo.Diamonds -= (uint)Math.Abs(Diamonds);
                            else
                                habbo.Diamonds += (uint)Diamonds;

                            habbo.UpdateExtraMoneyBalance();
                        }

                        break;
                    }

                case "updatechatsettings":
                    {
                        uint HabboId = uint.Parse(data.Split(';')[1]);

                        GameClient client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(HabboId);
                        if (client != null && client.GetHabbo() != null)
                        {
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                dbClient.setQuery("SELECT chat_color FROM users WHERE id = '" + HabboId + "'");
                                client.GetHabbo().ChatColor = dbClient.getString();
                            }
                        }

                        break;
                    }
                case "updatecoins":
                    {
                        uint HabboId = uint.Parse(data.Split(';')[1]);
                        uint Coins = uint.Parse(data.Split(';')[2]);

                        GameClient client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(HabboId);
                        if (client != null && client.GetHabbo() != null)
                        {
                            client.GetHabbo().CoinsPurchased += Coins;
                        }

                        break;
                    }
            }
        }

        internal void clientCommand(String data)
        {
            String header = data.Split(';')[0];

            switch(header)
            {
                case "actualRoom":
                    {
                        uint roomId = uint.Parse(data.Split(';')[1]);
                        sendCommand(header + ";" + roomId.ToString());

                        break;
                    }
            }
        }

        internal void sendCommand(String data)
        {
            try
            {
                socket.Send(OtanixEnvironment.GetDefaultEncoding().GetBytes(data));
            }
            catch
            {
                tryClose();
            }
        }
    }
}
