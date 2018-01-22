using Butterfly.HabboHotel.GameClients;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Alfas.Manager
{
    class TourManager
    {
        public List<uint> Alfas;

        public TourManager()
        {
            this.Alfas = new List<uint>();
        }

        public int AlfasCount()
        {
            return Alfas.Count;
        }

        public bool ContainsUser(uint UserId)
        {
            return Alfas.Contains(UserId);
        }

        public void AddAlfa(uint userId)
        {
            if (!Alfas.Contains(userId))
                Alfas.Add(userId);
        }

        public void RemoveAlfa(uint userId)
        {
            if (Alfas.Contains(userId))
                Alfas.Remove(userId);
        }
    }
}
