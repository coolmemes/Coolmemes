using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Butterfly.HabboHotel.Items;
using Butterfly.Messages;
using ButterStorm;

namespace Butterfly.HabboHotel.Catalogs
{
    class CatalogItem
    {
        internal readonly uint Id;
        internal readonly string ItemIdString;
        internal readonly Dictionary<uint, uint> Items;
        internal readonly string Name;
        internal readonly uint CreditsCost;
        internal readonly uint PiruletasCost;
        internal readonly uint DucketsCost;
        internal readonly uint DiamondsCost;
        internal readonly uint Amount;
        internal readonly int PageID;
        internal int LimitedSelled;
        internal readonly bool HaveOffer;
        internal readonly int ClubLevel;
        internal readonly uint songID = 0;
        internal readonly string BadgeName = "";
        internal readonly string BotLook = "";
        internal readonly string corString = "";
        internal readonly bool AllowGift;
        internal readonly int PredesignedId;

        internal CatalogItem(DataRow Row)
        {
            Id = Convert.ToUInt32(Row["id"]);
            Name = (string)Row["catalog_name"];
            ItemIdString = (string)Row["item_ids"];
            Items = new Dictionary<uint, uint>();
            if (ItemIdString.Contains(";"))
            {
                var splitted = ItemIdString.Split(';');
                foreach (var s in splitted)
                {
                    if (s != "")
                    {
                        if (s.Contains("-"))
                        {
                            if (!Items.ContainsKey(uint.Parse(s.Split('-')[0])))
                                Items.Add(uint.Parse(s.Split('-')[0]), uint.Parse(s.Split('-')[1]));
                        }
                        else
                        {
                            if (!Items.ContainsKey(uint.Parse(s)))
                                Items.Add(uint.Parse(s), 1);
                        }
                    }
                }
            }
            else
            {
                uint value = 0;
                if (uint.TryParse(ItemIdString, out value))
                {
                    Items.Add(value, 1);
                }
            }
            PageID = (int)Row["page_id"];
            CreditsCost = Convert.ToUInt32(Row["cost_credits"]);
            DucketsCost = Convert.ToUInt32(Row["cost_duckets"]);
            PiruletasCost = Convert.ToUInt32(Row["cost_piruletas"]);
            DiamondsCost = Convert.ToUInt32(Row["cost_diamonds"]);
            Amount = Convert.ToUInt32(Row["amount"]);
            LimitedSelled = (int)Row["limited_sells"];
            HaveOffer = ((string)Row["offer_active"] == "1");
            ClubLevel = (int)Row["club_level"];

            PredesignedId = Convert.ToInt32(Row["predesigned_id"]);

            if (Name.StartsWith("SONG"))
                songID = Convert.ToUInt32(Row["extra_info"]);
            else if (Name.StartsWith("bot_"))
                BotLook = ((string)Row["extra_info"]);
            else
                BadgeName = ((string)Row["extra_info"]);

            AllowGift = ((string)Row["allow_gift"] == "1") ? true : false;
        }

        internal Item GetBaseItem(uint ItemIds)
        {
            var Return = OtanixEnvironment.GetGame().GetItemManager().GetItem(ItemIds);
            if (Return == null)
            {
                if (ItemIds != 0)
                    Console.WriteLine(@"UNKNOWN ItemIds: " + ItemIds);
            }

            return Return;
        }

        internal void Serialize(ServerMessage Message)
        {
            try
            {
                bool IsLimited = false;

                Message.AppendUInt(Id);
                Message.AppendString(Name);
                Message.AppendBoolean(false);
                Message.AppendUInt(CreditsCost);
                Message.AppendUInt((DiamondsCost > 0) ? DiamondsCost : (DucketsCost > 0) ? DucketsCost : PiruletasCost);
                Message.AppendInt32((DiamondsCost > 0) ? 5 : (DucketsCost > 0) ? 0 : 103);
                Message.AppendBoolean(AllowGift);
                Message.AppendInt32((BadgeName.Length > 0 && Name != "room_ad_plus_badge") ? (Items.Count + 1) : Items.Count); // items on pack
                
                if (BadgeName.Length > 0)
                {
                    Message.AppendString("b");
                    Message.AppendString(BadgeName);
                }

                if (Name != "room_ad_plus_badge")
                {
                    foreach (var i in Items)
                    {
                        Message.AppendString(GetBaseItem(i.Key).Type.ToString());
                        Message.AppendInt32(GetBaseItem(i.Key).SpriteId);

                        if (Name.Contains("wallpaper_single") || Name.Contains("floor_single") || Name.Contains("landscape_single"))
                        {
                            var Analyze = Name.Split('_');
                            Message.AppendString(Analyze[2]);
                        }
                        else if (GetBaseItem(i.Key).InteractionType == InteractionType.bot)
                        {
                            Message.AppendString(BotLook);
                        }
                        else if (songID > 0 && GetBaseItem(i.Key).InteractionType == InteractionType.musicdisc)
                        {
                            Message.AppendString(songID.ToString());
                        }
                        else
                        {
                            Message.AppendString(string.Empty);
                        }

                        Message.AppendUInt((Items.Count > 1) ? i.Value : Amount);
                        Message.AppendBoolean(GetBaseItem(i.Key).LimitedStack > 0); // IsLimited

                        if (GetBaseItem(i.Key).LimitedStack > 0)
                        {
                            IsLimited = true;
                            Message.AppendInt32(GetBaseItem(i.Key).LimitedStack);
                            Message.AppendInt32(GetBaseItem(i.Key).LimitedStack - LimitedSelled);
                        }
                    }
                }

                Message.AppendInt32(ClubLevel); // club_level
                Message.AppendBoolean(IsLimited ? false : HaveOffer); // IsOffer
                Message.AppendBoolean(false); // aun nada
                Message.AppendString(""); // previewImage
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to load furniture item " + Id + ": " + Name + ": " + e);
            }

       
        }
        internal void SerializeClub(ServerMessage Message, GameClients.GameClient Session)
        {
            Message.AppendInt32((int)Id);
            Message.AppendString(Name);
            Message.AppendBoolean(false);
            Message.AppendInt32((int)CreditsCost);
            Message.AppendInt32(((int)DiamondsCost > 0) ? (int)DiamondsCost : (int)DiamondsCost);
            Message.AppendInt32(((int)DiamondsCost > 0) ? 105 : 0);
            Message.AppendBoolean(true);

            int Days = 0;
            int Months = 0;

            if (GetBaseItem(Id).InteractionType == InteractionType.club_1_month || GetBaseItem(Id).InteractionType == InteractionType.club_3_month || GetBaseItem(Id).InteractionType == InteractionType.club_6_month)
            {

                switch (GetBaseItem(Id).InteractionType)
                {
                    case InteractionType.club_1_month:
                        Months = 1;
                        break;

                    case InteractionType.CLUB_VIP:
                        Months = 1;
                        break;

                    case InteractionType.club_3_month:
                        Months = 3;
                        break;

                    case InteractionType.CLUB_VIP2:
                        Months = 3;
                        break;

                    case InteractionType.club_6_month:
                        Months = 6;
                        break;
                }

                Days = 31 * Months;
            }

            DateTime future = DateTime.Now;
            if (PageID == EmuSettings.CLUB_PAGE_ID && Session.GetHabbo().GetClubManager().UserHasSubscription("club_habbo"))
            {
                double Expire = Session.GetHabbo().GetClubManager().GetSubscription("club_habbo").TimestampExpire;
                double TimeLeft = Expire - OtanixEnvironment.GetUnixTimestamp();
                int TotalDaysLeft = (int)Math.Ceiling(TimeLeft / 86400);
                future = DateTime.Now.AddDays(TotalDaysLeft);
            }

            future = future.AddDays(Days);

            Message.AppendInt32(Months);
            Message.AppendInt32(Days);
            Message.AppendBoolean(true); // gift
            Message.AppendInt32(Days);
            Message.AppendInt32(future.Year);
            Message.AppendInt32(future.Month);
            Message.AppendInt32(future.Day);
        }

        //internal void SerializeDiscountClub(ServerMessage Message, GameClients.GameClient Session)
        //{
        //    Message.AppendInt32(11093);
        //    Message.AppendString("HABBO_CLUB_VIP_EXTEND_1_MONTH");
        //    Message.AppendBoolean(false);
        //    Message.AppendInt32(45);
        //    Message.AppendInt32(45);
        //    Message.AppendInt32(5);
        //    Message.AppendBoolean(true);
        //    Message.AppendInt32(1);
        //    Message.AppendInt32(31);
        //    Message.AppendBoolean(false);
        //    Message.AppendInt32(32);
        //    Message.AppendInt32(2018);
        //    Message.AppendInt32(11);
        //    Message.AppendInt32(6);
        //    Message.AppendInt32(50);
        //    Message.AppendInt32(50);
        //    Message.AppendInt32(5);
        //    Message.AppendInt32(1);
        //    SendMessage(Message);
        //}
    }
}
