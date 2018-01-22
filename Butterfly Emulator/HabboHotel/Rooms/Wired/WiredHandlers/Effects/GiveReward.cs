using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Messages;
using Butterfly.Util;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using System;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class GiveReward : IWiredEffect, IWiredTrigger
    {
        private RoomItem item;

        private String ExtraInfo;
        private Int32 Amount;
        private Int32 Type;
        private Int32 AllUsers;
        private Int32 nInt;

        public GiveReward(string _ExtraInfo, int _Amount, int _Type, int _AllUsers, int _nInt, RoomItem _Id)
        {
            item = _Id;
            ExtraInfo = _ExtraInfo;
            Amount = _Amount;
            Type = _Type;
            AllUsers = _AllUsers;
            nInt = _nInt;
        }

        public string Message
        {
            get
            {
                return ExtraInfo;
            }
        }

        public int _Amount
        {
            get
            {
                return Amount;
            }
        }

        public int _Type
        {
            get
            {
                return Type;
            }
        }

        public int _AllUsers
        {
            get
            {
                return AllUsers;
            }
        }

        public int _nInt
        {
            get
            {
                return nInt;
            }
        }
        
        private Boolean isCorrectInt(RoomUser user)
        {
            if (user.GetClient().GetHabbo().WiredRewards.ContainsKey(item.Id))
            {
                if(Type == 0)
                {
                    return false;
                }
                else if(Type == 1 || Type == 2)
                {
                    WiredActReward wiredAct = user.GetClient().GetHabbo().WiredRewards[item.Id];
                    if ((OtanixEnvironment.GetUnixTimestamp() > (wiredAct.LastUpdate + ((Type == 1) ? 86400 : 3600))) || wiredAct.OriginalInt != nInt)
                    {
                        wiredAct.OriginalInt = nInt;
                        wiredAct.ActualRewards = nInt;
                        wiredAct.LastUpdate = OtanixEnvironment.GetUnixTimestamp();
                    }

                    if (wiredAct.ActualRewards > 0)
                    {
                        wiredAct.ActualRewards--;
                        return true;
                    }
                }
            }
            else
            {
                WiredActReward wiredAct = new WiredActReward(item.Id, OtanixEnvironment.GetUnixTimestamp(), (nInt - 1), nInt);
                user.GetClient().GetHabbo().WiredRewards.Add(item.Id, wiredAct);
                
                return true;
            }

            return false;
        }

        public void Handle(RoomUser user, Team team, RoomItem item)
        {
            try
            {
                /*
                // 0: Lo sentimos. Los premios disponibles son limitados y ya han sido todos repartidos.
                // 1: Ya has obtenido este premio. Cada usuari@ sólo puede ganar el mismo premio una vez.
                // 2: Hoy ya has sido premiada, prueba suerte mañana!
                // 3: Ya has sido premiad@ durante la última hora. Prueba de nuevo dentro de una hora.
                // 4: No ha habido suerte esta vez. Prueba de nuevo para hacerte con el premio.
                // 5: Ya has reunido todos los premios que podías obtener.
                // 6: Has recibido un premio. Mira en Mis Cosas o en Mis Efectos para ver qué es.
                // 7: Acabas de recibir una Placa. Mira en Mis Cosas.
                */

                // InteractorGenericSwitch.DoAnimation(item);

                if (user != null && !user.IsBot && user.GetClient() != null && isCorrectInt(user))
                {
                    var data = ExtraInfo;
                    foreach (var Datas in data.Split(';'))
                    {
                        var isbadge = int.Parse(Datas.Split(',')[0]);
                        var code = Datas.Split(',')[1];
                        var percentage = int.Parse(Datas.Split(',')[2]);

                        if (Amount > 0 || AllUsers == 1)
                        {
                            if (isbadge == 0) // placa
                            {
                                #region BadgeCode
                                if (AllUsers == 1) // no porcentaje:
                                {
                                    if (user.GetClient().GetHabbo().GetBadgeComponent().HasBadge(code))
                                    {
                                        var Message = new ServerMessage(Outgoing.NoRewardRoom);
                                        Message.AppendInt32(1);
                                        user.GetClient().SendMessage(Message);
                                    }
                                    else
                                    {
                                        user.GetClient().GetHabbo().GetBadgeComponent().GiveBadge(code);

                                        var Message = new ServerMessage(Outgoing.NoRewardRoom);
                                        Message.AppendInt32(7);
                                        user.GetClient().SendMessage(Message);
                                    }
                                }
                                else
                                {
                                    var randomnumber = new Random().Next(0, 100);
                                    if (randomnumber <= percentage) // premiado
                                    {
                                        if (user.GetClient().GetHabbo().GetBadgeComponent().HasBadge(code))
                                        {
                                            var Message = new ServerMessage(Outgoing.NoRewardRoom);
                                            Message.AppendInt32(1);
                                            user.GetClient().SendMessage(Message);
                                        }
                                        else
                                        {
                                            user.GetClient().GetHabbo().GetBadgeComponent().GiveBadge(code);

                                            var Message = new ServerMessage(Outgoing.NoRewardRoom);
                                            Message.AppendInt32(7);
                                            user.GetClient().SendMessage(Message);
                                        }
                                    }
                                    else
                                    {
                                        var Message = new ServerMessage(Outgoing.NoRewardRoom);
                                        Message.AppendInt32(4);
                                        user.GetClient().SendMessage(Message);
                                    }
                                }
                                #endregion
                            }
                            else // item && Effect
                            {
                                #region Item Or Effect
                                if (AllUsers == 1) // no porcentaje:
                                {
                                    if (code.StartsWith("diamonds:"))
                                    {
                                        int amount = int.Parse(code.Substring(9));
                                        user.GetClient().GetHabbo().GiveUserDiamonds(amount);
                                        user.GetClient().SendNotif("Acaba de receber " + amount + " diamantes.");
                                    }
                                    else if (code.StartsWith("alert:"))
                                    {
                                        string message = code.Substring(6);
                                        user.GetClient().SendNotif(message);
                                    }
                                    else
                                    {
                                        var Item = OtanixEnvironment.GetGame().GetItemManager().GetItem(uint.Parse(code));
                                        if (Item != null)
                                        {
                                            if (Item.Type == 'e') // is effect
                                            {
                                                user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().AddEffect(Item.SpriteId, 3600);
                                            }
                                            else
                                            {
                                                user.GetClient().GetHabbo().GetInventoryComponent().AddNewItem(0, Item.ItemId, "", true, false, false, Item.Name, user.GetClient().GetHabbo().Id, 0);
                                                user.GetClient().GetHabbo().GetInventoryComponent().UpdateItems(false);
                                            }
                                            var Message = new ServerMessage(Outgoing.NoRewardRoom);
                                            Message.AppendInt32(6);
                                            user.GetClient().SendMessage(Message);
                                        }
                                    }
                                }
                                else
                                {
                                    var randomnumber = new Random().Next(0, 100);
                                    if (randomnumber <= percentage) // premiado
                                    {
                                        if (code.StartsWith("diamonds:"))
                                        {
                                            int amount = int.Parse(code.Substring(9));
                                            user.GetClient().GetHabbo().GiveUserDiamonds(amount);
                                            user.GetClient().SendNotif("Acaba de reciber " + amount + " diamantes.");
                                        }
                                        else if (code.StartsWith("alert:"))
                                        {
                                            string message = code.Substring(6);
                                            user.GetClient().SendNotif(message);
                                        }
                                        else
                                        {
                                            var Item = OtanixEnvironment.GetGame().GetItemManager().GetItem(uint.Parse(code));
                                            if (Item != null)
                                            {
                                                if (Item.Type == 'e') // is effect
                                                {
                                                    user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().AddEffect(Item.SpriteId, 3600);
                                                }
                                                else
                                                {
                                                    user.GetClient().GetHabbo().GetInventoryComponent().AddNewItem(0, Item.ItemId, "", true, false, false, Item.Name, user.GetClient().GetHabbo().Id, 0);
                                                    user.GetClient().GetHabbo().GetInventoryComponent().UpdateItems(false);
                                                }
                                                var Message = new ServerMessage(Outgoing.NoRewardRoom);
                                                Message.AppendInt32(6);
                                                user.GetClient().SendMessage(Message);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var Message = new ServerMessage(Outgoing.NoRewardRoom);
                                        Message.AppendInt32(4);
                                        user.GetClient().SendMessage(Message);
                                    }
                                }
                                #endregion
                            }

                            if (Amount > 0)
                            {
                                Amount--;

                                /*using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                                {
                                    var newGenerate = ExtraInfo.ToString() + ";" + Amount + "," + Type + "," + AllUsers + ";false";
                                    dbClient.setQuery("REPLACE INTO items_wired (item_id, wired_data) VALUES (@id, @nG)");
                                    dbClient.addParameter("id", (int)item.Id);
                                    dbClient.addParameter("nG", newGenerate);
                                    dbClient.runQuery();
                                }*/
                            }
                        }
                        else
                        {
                            var Message = new ServerMessage(Outgoing.NoRewardRoom);
                            Message.AppendInt32(0);
                            user.GetClient().SendMessage(Message);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public void Dispose()
        {
        }

        public void SaveToDatabase(QueryChunk wiredInserts)
        {
            string wired_data = ExtraInfo.ToString() + ";" + Amount + "," + Type + "," + AllUsers + "," + nInt + ";false";
            string wired_to_item = "";
            string wired_original_location = "";

            wiredInserts.AddQuery("('" + item.Id + "', @data" + item.Id + ", @to_item" + item.Id + ", @original_location" + item.Id + ")");
            wiredInserts.AddParameter("data" + item.Id, wired_data);
            wiredInserts.AddParameter("to_item" + item.Id, wired_to_item);
            wiredInserts.AddParameter("original_location" + item.Id, wired_original_location);
        }
    }
}
