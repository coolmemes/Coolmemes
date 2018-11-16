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

namespace Butterfly.HabboHotel.Premiums.Users
{
    class Premium
    {
        private uint UserId;
        private double UnixStart;
        private double UnixEnd;
        private uint ActualItems;
        private uint MaxItems;
        private bool[] PremiumIds;

        public int GetRemainingTime()
        {
            return (int)(UnixEnd - OtanixEnvironment.GetUnixTimestamp());
        }

        public uint GetActualItems()
        {
            return ActualItems;
        }

        public void IncreaseItems()
        {
            ActualItems++;
        }

        public void DecreaseItems()
        {
            ActualItems--;
        }

        public uint GetMaxItems()
        {
            return MaxItems;
        }

        public Premium(uint UserId, double UnixStart, double UnixEnd, uint MaxItems)
        {
            this.UserId = UserId;
            this.UnixStart = UnixStart;
            this.UnixEnd = UnixEnd;
            this.MaxItems = MaxItems;
            this.PremiumIds = new bool[this.MaxItems];
            for(int i = 0; i < this.MaxItems; i++)
            {
                this.PremiumIds[i] = false;
            }

            this.GeneratePremiumItemsInfo();
        }

        private void GeneratePremiumItemsInfo()
        {
            DataTable dTable = null;
            using(IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT premium_id FROM items_premium WHERE user_id = " + UserId);
                dTable = dbClient.getTable();
            }

            if (dTable == null)
                return;

            foreach(DataRow dRow in dTable.Rows)
            {
                int premiumId = (int)dRow["premium_id"];

                IncreaseItems();
                ModifyItemPosition(premiumId, true);
            }
        }

        public void ModifyItemPosition(int Position, bool Value)
        {
            this.PremiumIds[Position] = Value;
        }

        public uint GetValidPosition()
        {
            for (uint i = 0; i < this.MaxItems; i++)
            {
                if (this.PremiumIds[i] == false)
                {
                    ModifyItemPosition((int)i, true);
                    return i;
                }
            }

            return 0;
        }
        public void Destroy()
        {
            Array.Clear(this.PremiumIds, 0, this.PremiumIds.Length);
            this.PremiumIds = null;
        }
    }
}
