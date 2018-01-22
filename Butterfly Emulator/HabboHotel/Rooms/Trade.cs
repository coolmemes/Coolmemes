using System;
using System.Collections.Generic;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Items;
using Butterfly.Messages;
using HabboEvents;
using Database_Manager.Database.Session_Details.Interfaces;
using ButterStorm;

namespace Butterfly.HabboHotel.Rooms
{
    class Trade
    {
        public static readonly Dictionary<uint, Trade> tradeMap = new Dictionary<uint, Trade>();
        public readonly TradeUser ownerUser;
        public readonly TradeUser guestUser;

        internal Trade(GameClient owner, GameClient guest)
        {
            ownerUser = new TradeUser(owner);
            tradeMap.Add(ownerUser.userId, this);

            guestUser = new TradeUser(guest);
            tradeMap.Add(guestUser.userId, this);
        }

        internal void Accept(uint UserId)
        {
            var user = guestUser;
            if (ownerUser.userId == UserId)
                user = ownerUser;

            if (user.status != 0)
                return;

            user.status = 1;

            var Message = new ServerMessage(Outgoing.TradeAcceptUpdate);
            Message.AppendUInt(UserId);
            Message.AppendInt32(1);
            SendMessageToUsers(Message);

            if (guestUser.status == ownerUser.status)
                SendMessageToUsers(new ServerMessage(Outgoing.TradeComplete));
        }

        internal void Unaccept(uint UserId)
        {
            var user = guestUser;
            if (ownerUser.userId == UserId)
                user = ownerUser;

            if (user.status != 1)
                return;

            user.status = 0;

            var Message = new ServerMessage(Outgoing.TradeAcceptUpdate);
            Message.AppendUInt(UserId);
            Message.AppendInt32(0);
            SendMessageToUsers(Message);
        }

        internal void OfferItem(UInt32 UserId, UserItem Item)
        {
            var user = guestUser;
            if (ownerUser.userId == UserId)
                user = ownerUser;

            if (user.status != 0)
                return;

            if (user.furnis.ContainsKey(Item.Id))
                return;

            user.furnis.Add(Item.Id, Item);
            UpdateTradeWindow(ownerUser, guestUser);
            
            if (ownerUser.status != 0)
            {
                ownerUser.status = 0;

                var Message = new ServerMessage(Outgoing.TradeAcceptUpdate);
                Message.AppendUInt(UserId);
                Message.AppendInt32(0);
                SendMessageToUsers(Message);
            }

            if (guestUser.status != 0)
            {
                guestUser.status = 0;

                var Message = new ServerMessage(Outgoing.TradeAcceptUpdate);
                Message.AppendUInt(UserId);
                Message.AppendInt32(0);
                SendMessageToUsers(Message);
            }
        }

        internal void TakeBackItem(UInt32 UserId, UserItem Item)
        {
            var user = guestUser;
            if (ownerUser.userId == UserId)
                user = ownerUser;

            user.furnis.Remove(Item.Id);
            UpdateTradeWindow(ownerUser, guestUser);
        }

        internal void CompleteTrade(UInt32 UserId)
        {
            var user = guestUser;
            if (ownerUser.userId == UserId)
                user = ownerUser;

            if (user.status != 1)
                return;

            user.status = 2;

            var Message = new ServerMessage(Outgoing.TradeAcceptUpdate);
            Message.AppendUInt(UserId);
            Message.AppendInt32(1);
            SendMessageToUsers(Message);

            if (guestUser.status == ownerUser.status)
            {
                if (ownerUser.furnis.Count > 0)
                {
                    foreach (var I in ownerUser.furnis.Values)
                    {
                        ownerUser.connection.GetHabbo().GetInventoryComponent().RemoveItem(I.Id, false);
                        guestUser.connection.GetHabbo().GetInventoryComponent().AddNewItem(I.Id, I.BaseItem, I.ExtraData, false, false, false, I.mBaseItem.Name, ownerUser.userId, 0);

                        LogTradeItem(I.Id, I.mBaseItem.ItemId, ownerUser.userId, guestUser.userId);

                        ownerUser.connection.GetHabbo().GetInventoryComponent().RunDBUpdate();
                        guestUser.connection.GetHabbo().GetInventoryComponent().RunDBUpdate();
                    }
                    guestUser.connection.GetHabbo().GetInventoryComponent().UpdateItems(false);
                    ownerUser.furnis.Clear();
                }

                if (guestUser.furnis.Count > 0)
                {
                    foreach (var I in guestUser.furnis.Values)
                    {
                        guestUser.connection.GetHabbo().GetInventoryComponent().RemoveItem(I.Id, false);
                        ownerUser.connection.GetHabbo().GetInventoryComponent().AddNewItem(I.Id, I.BaseItem, I.ExtraData, false, false, false, I.mBaseItem.Name, guestUser.userId, 0);

                        LogTradeItem(I.Id, I.mBaseItem.ItemId, guestUser.userId, ownerUser.userId);

                        guestUser.connection.GetHabbo().GetInventoryComponent().RunDBUpdate();
                        ownerUser.connection.GetHabbo().GetInventoryComponent().RunDBUpdate();
                    }
                    ownerUser.connection.GetHabbo().GetInventoryComponent().UpdateItems(false);
                    guestUser.furnis.Clear();
                }

                SendMessageToUsers(new ServerMessage(Outgoing.TradeCloseClean));
                CloseTradeClean();
            }
        }

        internal void LogTradeItem(UInt32 ItemId, UInt32 SpriteId, UInt32 UserId, UInt32 ReceiverId)
        {
            string Date = DateTime.Now.ToShortDateString();

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("INSERT INTO items_traded_logs VALUES (NULL,'" + ItemId + "','" + SpriteId + "','" + Date + "','" + UserId + "','" + ReceiverId + "')");
            }
        }

        internal void UpdateTradeWindow(TradeUser owner, TradeUser guest)
        {
            var Message = new ServerMessage(Outgoing.TradeUpdate);
            
            Message.AppendUInt(owner.userId);
            Message.AppendInt32(owner.furnis.Count);
            foreach (var Item in owner.furnis.Values)
            {
                Message.AppendUInt(Item.Id);
                Message.AppendString(Item.mBaseItem.Type.ToString().ToLower());
                Message.AppendUInt(Item.Id);
                Message.AppendInt32(Item.mBaseItem.SpriteId);
                Message.AppendInt32(0); // undef
                Message.AppendBoolean(true);
                Message.AppendInt32(0);
                Message.AppendString("");
                Message.AppendInt32(0); // xmas 09 furni had a special furni tag here, with wired day (wat?)
                Message.AppendInt32(0); // xmas 09 furni had a special furni tag here, wired month (wat?)
                Message.AppendInt32(0); // xmas 09 furni had a special furni tag here, wired year (wat?)

                if (Item.mBaseItem.Type == 's')
                {
                    Message.AppendInt32(0);
                }
            }
            Message.AppendInt32(0); // ??
            Message.AppendInt32(0); // ??

            Message.AppendUInt(guest.userId);
            Message.AppendInt32(guest.furnis.Count);
            foreach (var Item in guest.furnis.Values)
            {
                Message.AppendUInt(Item.Id);
                Message.AppendString(Item.mBaseItem.Type.ToString().ToLower());
                Message.AppendUInt(Item.Id);
                Message.AppendInt32(Item.mBaseItem.SpriteId);
                Message.AppendInt32(0); // undef
                Message.AppendBoolean(true);
                Message.AppendInt32(0);
                Message.AppendString("");
                Message.AppendInt32(0); // xmas 09 furni had a special furni tag here, with wired day (wat?)
                Message.AppendInt32(0); // xmas 09 furni had a special furni tag here, wired month (wat?)
                Message.AppendInt32(0); // xmas 09 furni had a special furni tag here, wired year (wat?)

                if (Item.mBaseItem.Type == 's')
                {
                    Message.AppendInt32(0);
                }
            }
            Message.AppendInt32(0); // ??
            Message.AppendInt32(0); // ??

            SendMessageToUsers(Message);
        }

        internal void CloseTradeClean()
        {
            if (ownerUser == null || guestUser == null)
                return;

            tradeMap.Remove(ownerUser.userId);
            tradeMap.Remove(guestUser.userId);

            if (ownerUser.GetRoomUser() == null || guestUser.GetRoomUser() == null)
                return;

            if (ownerUser.GetRoomUser().IsTrading)
            {
                ownerUser.GetRoomUser().RemoveStatus("trd");
                ownerUser.GetRoomUser().UpdateNeeded = true;
            }

            if (guestUser.GetRoomUser().IsTrading)
            {
                guestUser.GetRoomUser().RemoveStatus("trd");
                guestUser.GetRoomUser().UpdateNeeded = true;
            }

            if (ownerUser.furnis.Count > 0)
                ownerUser.furnis.Clear();

            if (guestUser.furnis.Count > 0)
                guestUser.furnis.Clear();
        }

        internal void CloseTrade(UInt32 UserId)
        {
            var Message = new ServerMessage(Outgoing.TradeClose);
            Message.AppendUInt(UserId);
            Message.AppendInt32(2);
            SendMessageToUsers(Message);

            CloseTradeClean();
        }

        internal void SendMessageToUsers(ServerMessage Message)
        {
            ownerUser.connection.SendMessage(Message);
            guestUser.connection.SendMessage(Message);
        }

        internal static Trade getContainsTrade(uint UserId)
        {
            if (tradeMap.ContainsKey(UserId))
                return tradeMap[UserId];
            return null;
        }

        internal static ServerMessage messageTradeError(int Value, string ToUsername)
        {
            // 1: Ahora mismo el tradeo no está permitido en Habbo Hotel.
            // 2: Tu cuenta tiene el tradeo desactivado.
            // 3: message
            // 4: <username> no quiere o no puede tradear un cajero automático.
            // 5: message
            // 6: El tradeo no está permitido en esta sala.
            // 7: Ya tienes un tradeo en marcha. Debes finalizarlo antes de abrir otro.
            // 8: <username> ya está tradeando.

            var message = new ServerMessage(Outgoing.TradeError);
            message.AppendInt32(Value);
            message.AppendString(ToUsername);
            return message;
        }
    }

    class TradeUser
    {
        public uint userId;
        public GameClient connection;
        public Dictionary<uint, UserItem> furnis;
        public int status;

        internal TradeUser(GameClient user)
        {
            userId = user.GetHabbo().Id;
            connection = user;
            furnis = new Dictionary<uint, UserItem>();
        }

        internal RoomUser GetRoomUser()
        {
            return connection.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(userId);
        }
    }
}
