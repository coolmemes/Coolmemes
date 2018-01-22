using Butterfly.HabboHotel.Premiums.Clothes;
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

namespace Butterfly.HabboHotel.Users.Clothing
{
    class UserClothing
    {
        private uint UserId;

        private List<int> Parts;
        private List<string> Clothes;

        public UserClothing(uint userID)
        {
            this.UserId = userID;
            this.Parts = new List<int>();
            this.Clothes = new List<string>();

            DataTable dTable = null;
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT * FROM user_clothing WHERE user_id = " + UserId);
                dTable = dbClient.getTable();
            }

            if (dTable != null)
            {
                foreach (DataRow dRow in dTable.Rows)
                {
                    string Clothe = (string)dRow["clothes_name"];
                    AddClothes(Clothe);
                }
            }
        }

        public bool ContainsPart(int Part)
        {
            return this.Parts.Contains(Part);
        }

        public bool ContainsClothes(string ClothesName)
        {
            return this.Clothes.Contains(ClothesName);
        }

        public void AddClothesToSQL(string ClothesName)
        {
            AddClothes(ClothesName);

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("INSERT IGNORE INTO user_clothing VALUES ('" + UserId + "','" + ClothesName + "')");
            }
        }

        private void AddClothes(string ClothesName)
        {
            if (!this.Clothes.Contains(ClothesName))
            {
                this.Clothes.Add(ClothesName);

                ClothingItem cl = OtanixEnvironment.GetGame().GetClothingManager().GetClothingItemByName(ClothesName);
                if (cl != null)
                {
                    foreach (int Part in cl.GetClothes())
                    {
                        if (!this.Parts.Contains(Part))
                            this.Parts.Add(Part);
                    }
                }
            }
        }

        public ServerMessage SerializeClothes()
        {
            ServerMessage Message = new ServerMessage(Outgoing.FigureSetIdsMessageParser);
            Message.AppendInt32(this.Parts.Count);
            foreach (int Part in this.Parts)
            {
                Message.AppendInt32(Part);
            }
            Message.AppendInt32(this.Clothes.Count);
            foreach (string Clothe in this.Clothes)
            {
                Message.AppendString(Clothe);
            }

            return Message;
        }
    }
}
