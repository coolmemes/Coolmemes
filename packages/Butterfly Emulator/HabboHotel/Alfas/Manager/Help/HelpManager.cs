using Butterfly.HabboHotel.GameClients;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Alfas.Manager
{
    class HelpManager
    {
        public List<uint> Alfas;
        internal Dictionary<UInt32, Help> Helps;

        private Queue helpsAddQueue;
        private Queue helpsRemoveQueue;

        internal HelpManager()
        {
            this.Alfas = new List<UInt32>();
            this.Helps = new Dictionary<UInt32, Help>();

            this.helpsAddQueue = new Queue();
            this.helpsRemoveQueue = new Queue();
        }

        internal bool UserStartedHelp(UInt32 UserId)
        {
            return this.Helps.ContainsKey(UserId);
        }

        internal void OnCycle()
        {
            AddHelps();
            UpdateHelps();
            RemoveHelps();
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

        internal void AddHelp(Help help)
        {
            lock (helpsAddQueue.SyncRoot)
            {
                helpsAddQueue.Enqueue(help);
            }
        }

        internal void RemoveHelp(UInt32 userId)
        {
            lock (helpsRemoveQueue.SyncRoot)
            {
                helpsRemoveQueue.Enqueue(userId);
            }
        }

        private void AddHelps()
        {
            if (helpsAddQueue.Count > 0)
            {
                lock (helpsAddQueue.SyncRoot)
                {
                    while (helpsAddQueue.Count > 0)
                    {
                        var help = (Help)helpsAddQueue.Dequeue();
                        if (help != null)
                        {
                            if (!Helps.ContainsKey(help.ReporterId))
                                Helps.Add(help.ReporterId, help);
                        }
                    }
                }
            }
        }

        private void UpdateHelps()
        {
            if (this.Helps.Count > 0)
            {
                foreach (Help help in this.Helps.Values)
                {
                    help.OnCycle();
                }
            }
        }

        private void RemoveHelps()
        {
            if (helpsRemoveQueue.Count > 0)
            {
                lock (helpsRemoveQueue.SyncRoot)
                {
                    while (helpsRemoveQueue.Count > 0)
                    {
                        var userId = (UInt32)helpsRemoveQueue.Dequeue();
                        if (Helps.ContainsKey(userId))
                            Helps.Remove(userId);
                    }
                }
            }
        }
    }
}
