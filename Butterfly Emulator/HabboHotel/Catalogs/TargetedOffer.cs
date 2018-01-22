using Butterfly.HabboHotel.Items;
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

namespace Butterfly.HabboHotel.Catalogs
{
    class TargetedOfferManager
    {
        private static List<TargetedOffer> targetedOffers;

        internal void Initialize(IQueryAdapter dbClient)
        {
            targetedOffers = new List<TargetedOffer>();

            dbClient.setQuery("SELECT * FROM targered_offers WHERE enable = '1'");
            DataTable dTable = dbClient.getTable();

            foreach (DataRow dRow in dTable.Rows)
            {
                targetedOffers.Add(new TargetedOffer(dRow));
            }
        }

        internal TargetedOffer GetTargetedOffer(uint Id)
        {
            foreach (TargetedOffer to in targetedOffers)
            {
                if (to.Id == Id)
                    return to;
            }

            return null;
        }

        internal TargetedOffer GetRoomIdTargetedOffer(uint RoomId)
        {
            foreach (TargetedOffer to in targetedOffers)
            {
                if (to.roomId == RoomId && to.ExpirationTime > OtanixEnvironment.GetUnixTimestamp())
                    return to;
            }

            return null;
        }

        internal TargetedOffer GetRandomStaticTargetedOffer()
        {
            List<TargetedOffer> list = targetedOffers.Where(element => element.roomId == 0 && element.ExpirationTime > OtanixEnvironment.GetUnixTimestamp()).ToList();

            if (list.Count > 0)
                return list[new Random().Next(list.Count)];
            else
                return null;
        }

        internal ServerMessage SerializeTargetedOffer(TargetedOffer to)
        {
            ServerMessage targetedOffer = new ServerMessage(Outgoing.TargetedOfferMessageComposer);
            targetedOffer.AppendInt32(1); // 0,5,6: maximizeMallOffer || 2: return || default: MallOfferMinimizedView
            targetedOffer.AppendUInt(to.Id); // offerId
            targetedOffer.AppendString(to.Identifier); // identifier
            targetedOffer.AppendString(to.Identifier2); // 
            targetedOffer.AppendUInt(to.PriceInCredits); // priceInCredits
            targetedOffer.AppendUInt(to.PriceInDiamonds); // priceInDiamonds
            targetedOffer.AppendInt32(to.ActivityPointType); // activityPointType -1 = moedas, 0 = duckets , 5 = diamantes
            targetedOffer.AppendUInt(to.PurchaseLimit); // purchaseLimit
            targetedOffer.AppendInt32(to.ExpirationTime - OtanixEnvironment.GetUnixTimestamp()); // expirationTime (seconds)
            targetedOffer.AppendString(to.Title); // title
            targetedOffer.AppendString(to.Description); // description
            targetedOffer.AppendString(to.ImageUrl); // imageUrl
            targetedOffer.AppendString(to.BmpIcon); // bmp_icon
            targetedOffer.AppendInt32(0); // type
            targetedOffer.AppendInt32(to.Items.Count); // items
            foreach(uint itemId in to.Items.Keys)
            {
                Item item = OtanixEnvironment.GetGame().GetItemManager().GetItem(itemId);
                targetedOffer.AppendString(item == null ? "" : item.Name); // itemName
            }
            return targetedOffer;
        }
    }

    class TargetedOffer
    {
        internal uint Id;
        internal string Identifier;
        internal string Identifier2;
        internal uint PriceInCredits;
        internal uint PriceInDiamonds;
        internal int ActivityPointType;
        internal uint PurchaseLimit;
        internal int ExpirationTime;
        internal string Title;
        internal string Description;
        internal string ImageUrl;
        internal string BmpIcon;
        internal Dictionary<uint, uint> Items;
        internal uint roomId;
        
        internal TargetedOffer(DataRow dRow)
        {
            this.Items = new Dictionary<uint, uint>();
            this.Id = Convert.ToUInt32(dRow["id"]);
            this.Identifier = (string)dRow["identifier"];
            this.Identifier2 = (string)dRow["identifier2"];
            this.PriceInCredits = Convert.ToUInt32(dRow["priceInCredits"]);
            this.PriceInDiamonds = Convert.ToUInt32(dRow["priceInDiamonds"]);
            this.ActivityPointType = Convert.ToInt32(dRow["activityPointType"]);
            this.PurchaseLimit = Convert.ToUInt32(dRow["purchaseLimit"]);
            this.ExpirationTime = Convert.ToInt32(dRow["expirationTime"]);
            this.Title = (string)dRow["title"];
            this.Description = (string)dRow["description"];
            this.ImageUrl = (string)dRow["imageUrl"];
            this.BmpIcon = (string)dRow["bmp_icon"];
            string items = (string)dRow["items"];
            if(!string.IsNullOrEmpty(items))
            {
                foreach(string item in items.Split(';'))
                {
                    uint itemId = uint.Parse(item);
                    if (!this.Items.ContainsKey(itemId))
                        this.Items.Add(itemId, 1);
                    else
                        this.Items[itemId]++;
                }
            }
            this.roomId = Convert.ToUInt32(dRow["roomId"]);
        }
    }
}
