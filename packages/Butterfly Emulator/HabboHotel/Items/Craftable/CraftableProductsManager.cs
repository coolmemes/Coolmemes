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

namespace Butterfly.HabboHotel.Items.Craftable
{
    class CraftableProductsManager
    {
        private Dictionary<string, CraftableProduct> craftableProduct;
        private List<uint> reqItems;
        private ServerMessage Message;
        private Dictionary<string, ServerMessage> ItemsMessage;

        internal ServerMessage GetMessage()
        {
            return Message;
        }

        internal ServerMessage GetItemMessage(string ItemName)
        {
            if (ItemsMessage.ContainsKey(ItemName))
                return ItemsMessage[ItemName];
            else
                return null;
        }

        internal CraftableProduct GetCraftableProduct(string ItemName)
        {
            if (craftableProduct.ContainsKey(ItemName))
                return craftableProduct[ItemName];

            return null;
        }

        internal void Initialize(IQueryAdapter dbClient)
        {
            this.craftableProduct = new Dictionary<string, CraftableProduct>();
            this.ItemsMessage = new Dictionary<string, ServerMessage>();
            this.reqItems = new List<uint>();

            dbClient.setQuery("SELECT * FROM items_craftables");
            DataTable dTable = dbClient.getTable();

            if(dTable != null)
            {
                Item Item = null;

                foreach(DataRow dRow in dTable.Rows)
                {
                    uint ItemId = uint.Parse(dRow["item_id"].ToString());
                    string[] reqSpl = dRow["requirements"].ToString().Split(';');
                    if(reqSpl.Length <= 0)
                        continue;

                    uint[] reqSub = new uint[reqSpl.Length];
                    for(int i = 0; i < reqSpl.Length; i++)
                    {
                        uint reqId = uint.Parse(reqSpl[i]);

                        if (!reqItems.Contains(reqId))
                            reqItems.Add(reqId);

                        reqSub[i] = reqId;
                    }

                    Item = OtanixEnvironment.GetGame().GetItemManager().GetItem(ItemId);

                    CraftableProduct cProd = new CraftableProduct(ItemId, Item.Name, reqSub);

                    this.craftableProduct.Add(Item.Name, cProd);
                }
            }

            this.Serialize();
            this.SerializeItems();
        }

        private void Serialize()
        {
            ServerMessage Message = new ServerMessage(Outgoing.CraftableProductsMessageParser);
            Message.AppendInt32(craftableProduct.Count);
            foreach (string itemName in craftableProduct.Keys)
            {
                Message.AppendString(itemName);
                Message.AppendString(itemName);
            }
            Message.AppendInt32(reqItems.Count);
            foreach (uint i in reqItems)
            {
                Item item = OtanixEnvironment.GetGame().GetItemManager().GetItem(i);
                Message.AppendString(item.Name);
            }

            this.Message = Message;
        }

        private void SerializeItems()
        {
            foreach(CraftableProduct cProd in craftableProduct.Values)
            {
                if (ItemsMessage.ContainsKey(cProd.GetItemName()))
                    continue;

                List<uint> dProd = cProd.GetDifferentItems();

                ServerMessage Message = new ServerMessage(Outgoing.CraftingRecipeMessageParser);
                Message.AppendInt32(dProd.Count);
                foreach(uint itemId in dProd)
                {
                    Item item = OtanixEnvironment.GetGame().GetItemManager().GetItem(itemId);

                    Message.AppendUInt(cProd.GetItemsCount(itemId));
                    Message.AppendString(item.Name);
                }

                ItemsMessage.Add(cProd.GetItemName(), Message);

                dProd.Clear();
                dProd = null;
            }
        }

        internal uint GetRandomCraftableId(uint[] myItems)
        {
            List<uint> probableCraftables = new List<uint>();
            uint[] myItemsCopy = new uint[myItems.Length];
            bool valid = false;
            uint count = 0;

            foreach (CraftableProduct craftableProduct in this.craftableProduct.Values)
            {
                count = 0;
                Array.Copy(myItems, myItemsCopy, myItems.Length);
                uint[] craftableItems = craftableProduct.GetReqIds;

                foreach (uint BaseId in craftableItems)
                {
                    valid = false;
                    
                    for (int i = 0; i < myItems.Length; i++)
                    {
                        if (BaseId == myItemsCopy[i])
                        {
                            count++;
                            valid = true;
                            myItemsCopy[i] = 0;

                            if (count == myItems.Length)
                            {
                                if (!probableCraftables.Contains(craftableProduct.GetItemId))
                                    probableCraftables.Add(craftableProduct.GetItemId);
                            }

                            break;
                        }
                    }

                    if (valid == false)
                        break;
                }
            }

            uint probableBaseId = probableCraftables[new Random().Next(probableCraftables.Count - 1)];

            probableCraftables.Clear();
            probableCraftables = null;

            Array.Clear(myItemsCopy, 0, myItemsCopy.Length);
            myItemsCopy = null;

            return probableBaseId;
        }

        internal bool GetSimilarItems(uint[] myItems, ref uint Count)
        {
            bool state = false;

            uint[] myItemsCopy = new uint[myItems.Length];

            foreach (CraftableProduct craftableProduct in this.craftableProduct.Values)
            {
                uint count = 0;
                Array.Copy(myItems, myItemsCopy, myItems.Length);

                uint[] craftableItems = craftableProduct.GetReqIds;

                foreach (uint BaseId in craftableItems)
                {
                    for (int i = 0; i < myItemsCopy.Length; i++)
                    {
                        if (BaseId == myItemsCopy[i])
                        {
                            count++;
                            myItemsCopy[i] = 0;
                            break;
                        }
                    }

                    if (count == myItemsCopy.Length) // ya se ha recorrido el array y todos los elementos se han encontrado
                    {
                        Count++;
                        if (count == craftableItems.Length)
                            state = true;

                        break;
                    }
                }
            }

            Array.Clear(myItemsCopy, 0, myItemsCopy.Length);
            myItemsCopy = null;

            return state;
        }
    }
}
