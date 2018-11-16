using System;
using Butterfly.Core;
using Butterfly.HabboHotel.Catalogs;
using Butterfly.Messages;
using ButterStorm;
using Butterfly.HabboHotel.Camera;
using System.Net;
using System.IO;

namespace Butterfly.HabboHotel.Items
{
    class UserItem
    {
        internal UInt32 Id;
        internal UInt32 BaseItem;
        internal string ExtraData;
        internal Int32 LimitedValue;
        internal bool isWallItem;
        
        internal Item mBaseItem
        {
            get
            {
                return OtanixEnvironment.GetGame().GetItemManager().GetItem(BaseItem);
            }
        }

        internal UserItem(UInt32 Id, UInt32 BaseItem, string ExtraData)
        {
            this.Id = Id;
            this.BaseItem = BaseItem;
            this.ExtraData = ExtraData;
            
            if (mBaseItem == null)
            {
                Console.WriteLine(@"Unknown baseItem ID: " + BaseItem);
                Logging.LogException("Unknown baseItem ID: " + BaseItem);
            }

            if (mBaseItem != null)
            {
                if (mBaseItem.LimitedStack > 0) // osea, si es limitado.
                {
                    try
                    {
                        this.LimitedValue = int.Parse(this.ExtraData.Split(';')[1]);
                        this.ExtraData = this.ExtraData.Split(';')[0];
                    }
                    catch
                    {
                        this.LimitedValue = -1;
                        this.ExtraData = "0";
                    }
                }

                isWallItem = (mBaseItem.Type == 'i');
            }
        }

        internal void SerializeWall(ServerMessage Message, Boolean Inventory)
        {
            try
            {
                Message.AppendUInt(Id);
                Message.AppendString(mBaseItem.Type.ToString().ToUpper());
                Message.AppendUInt(Id);
                Message.AppendInt32(mBaseItem.SpriteId);

                if (mBaseItem.Name.Contains("a2") || mBaseItem.Name.Contains("floor"))
                {
                    Message.AppendInt32(3);
                }
                else if (mBaseItem.Name.Contains("wallpaper") && mBaseItem.Name != "wildwest_wallpaper")
                {
                    Message.AppendInt32(2);
                }
                else if (mBaseItem.Name.Contains("landscape"))
                {
                    Message.AppendInt32(4);
                }
                else
                {
                    Message.AppendInt32(1);
                }
                Message.AppendInt32(0);
                Message.AppendString(ExtraData);
                Message.AppendBoolean(mBaseItem.AllowRecycle);
                Message.AppendBoolean(mBaseItem.AllowTrade);
                Message.AppendBoolean(mBaseItem.AllowInventoryStack);
                Message.AppendBoolean(Marketplace.CanSellItem(this));
                Message.AppendInt32(-1);
                Message.AppendBoolean(false);
                Message.AppendInt32(-1);

                if(mBaseItem.InteractionType == InteractionType.photo)
                {
                    
                }
            }
            catch (Exception e)
            {
                Logging.LogPacketException("Error al serializar un item de pared en el inventario: <" + Id + "> : ", e.ToString());
            }
        }

        internal void SerializeFloor(ServerMessage Message, Boolean Inventory)
        {
            Message.AppendUInt(Id);
            Message.AppendString(mBaseItem.Type.ToString().ToUpper());
            Message.AppendUInt(Id);
            Message.AppendInt32(mBaseItem.SpriteId);
            var result = 1;
            if (mBaseItem.InteractionType == InteractionType.gift)
            {
                result = 9;
            }
            else if(mBaseItem.InteractionType == InteractionType.seed)
            {
                result = 19;
            }

            Message.AppendInt32(result); // category
            if (mBaseItem.IsGroupItem)
            {
                try
                {
                    uint GroupID = uint.Parse(ExtraData.Split(';')[1]);

                    var Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupID);
                    Message.AppendInt32(2);
                    Message.AppendInt32(5);
                    Message.AppendString(ExtraData.Split(';')[0]);
                    Message.AppendString(GroupID.ToString());
                    Message.AppendString(Group == null ? "" : Group.GroupImage);
                    Message.AppendString(ExtraData.Split(';')[2]);
                    Message.AppendString(ExtraData.Split(';')[3]);
                }
                catch
                {
                    Message.AppendInt32(2);
                    Message.AppendInt32(0);
                }
            }
            else if (mBaseItem.LimitedStack > 0)
            {
                Message.AppendString("");
                Message.AppendBoolean(true);
                Message.AppendBoolean(false);
            }
            else if (mBaseItem.InteractionType == InteractionType.badge_display)
                Message.AppendInt32(2);
            else if (mBaseItem.InteractionType == InteractionType.maniqui || mBaseItem.InteractionType == InteractionType.yttv || mBaseItem.InteractionType == InteractionType.seed)
                Message.AppendInt32(1);
            else
                Message.AppendInt32(0);

            if (mBaseItem.InteractionType == InteractionType.maniqui)
            {
                if (ExtraData.Length <= 0 || !ExtraData.Contains(";") || ExtraData.Split(';').Length < 3)
                {
                    Message.AppendInt32(3); // Coun Of Values
                    Message.AppendString("GENDER");
                    Message.AppendString("m");
                    Message.AppendString("FIGURE");
                    Message.AppendString("");
                    Message.AppendString("OUTFIT_NAME");
                    Message.AppendString("");
                }
                else
                {
                    var Extradatas = ExtraData.Split(';');

                    Message.AppendInt32(3); // Count Of Values
                    Message.AppendString("GENDER");
                    Message.AppendString(Extradatas[0]);
                    Message.AppendString("FIGURE");
                    Message.AppendString(Extradatas[1]);
                    Message.AppendString("OUTFIT_NAME");
                    Message.AppendString(Extradatas[2]);
                }
            }
            else if (mBaseItem.InteractionType == InteractionType.yttv)
            {
                Message.AppendInt32(1);
                Message.AppendString("THUMBNAIL_URL");
                Message.AppendString(ExtraData);
            }
            else if (mBaseItem.InteractionType == InteractionType.badge_display)
            {
                Message.AppendInt32(4); // Count of Values
                Message.AppendString("0");
                if (ExtraData.Split(';').Length == 3)
                {
                    Message.AppendString(ExtraData.Split(';')[0]); // BadgeCode
                    Message.AppendString(ExtraData.Split(';')[1]); // OwnerName
                    Message.AppendString(ExtraData.Split(';')[2]); // DateBuyed
                }
                else
                {
                    Message.AppendString(ExtraData); // BadgeCode
                    Message.AppendString(""); // OwnerName
                    Message.AppendString(""); // DateBuyed
                }
            }
            else if (mBaseItem.InteractionType == InteractionType.seed)
            {
                Message.AppendInt32(1);
                Message.AppendString("rarity");
                Message.AppendString(ExtraData);
            }
            else if (mBaseItem.IsGroupItem) { }
            else
                Message.AppendString(ExtraData);

            if (mBaseItem.LimitedStack > 0)
            {
                Message.AppendInt32(LimitedValue);
                Message.AppendInt32(mBaseItem.LimitedStack);
            }

            Message.AppendBoolean(mBaseItem.AllowRecycle);
            Message.AppendBoolean(mBaseItem.AllowTrade);
            Message.AppendBoolean(mBaseItem.AllowInventoryStack);
            Message.AppendBoolean(Marketplace.CanSellItem(this));
            Message.AppendInt32(-1);
            Message.AppendBoolean(false);
            Message.AppendInt32(-1);
            Message.AppendString("");

            int giftValue = 0;
            if (mBaseItem.InteractionType == InteractionType.gift)
            {
                int.TryParse(ExtraData.Split(';')[1], out giftValue);
            }

            Message.AppendInt32(giftValue);
        }
    }
}
