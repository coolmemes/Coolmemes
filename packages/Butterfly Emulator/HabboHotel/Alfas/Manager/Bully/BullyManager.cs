using Butterfly.HabboHotel.GameClients;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Alfas.Manager
{
    class BullyManager
    {
        public List<uint> Guardians;
        internal Dictionary<UInt32, Bully> Bullies;

        private Queue bulliesAddQueue;
        private Queue bulliesRemoveQueue;

        internal BullyManager()
        {
            this.Guardians = new List<UInt32>();
            this.Bullies = new Dictionary<uint, Bully>();

            this.bulliesAddQueue = new Queue();
            this.bulliesRemoveQueue = new Queue();
        }

        internal void OnCycle()
        {
            AddBullies();
            UpdateBullies();
            RemoveBullies();
        }

        internal bool UserStartedBully(UInt32 UserId)
        {
            return Bullies.ContainsKey(UserId);
        }

        public int GuardianCount()
        {
            return Guardians.Count;
        }

        public bool ContainsUser(uint UserId)
        {
            return Guardians.Contains(UserId);
        }

        public void AddGuardian(uint userId)
        {
            if (!Guardians.Contains(userId))
                Guardians.Add(userId);
        }

        public void RemoveGuardian(uint userId)
        {
            if (Guardians.Contains(userId))
                Guardians.Remove(userId);
        }

        internal void AddBullie(Bully bully)
        {
            lock (bulliesAddQueue.SyncRoot)
            {
                bulliesAddQueue.Enqueue(bully);
            }
        }

        internal void RemoveBullie(UInt32 reporterId)
        {
            lock (bulliesRemoveQueue.SyncRoot)
            {
                bulliesRemoveQueue.Enqueue(reporterId);
            }
        }

        private void AddBullies()
        {
            if (bulliesAddQueue.Count > 0)
            {
                lock (bulliesAddQueue.SyncRoot)
                {
                    while (bulliesAddQueue.Count > 0)
                    {
                        var bully = (Bully)bulliesAddQueue.Dequeue();
                        if (bully != null)
                        {
                            if (!Bullies.ContainsKey(bully.ReporterId))
                                Bullies.Add(bully.ReporterId, bully);
                        }
                    }
                }
            }
        }

        private void UpdateBullies()
        {
            if (this.Bullies.Count > 0)
            {
                foreach (Bully bully in this.Bullies.Values)
                {
                    bully.OnCycle();
                }
            }
        }

        private void RemoveBullies()
        {
            if (bulliesRemoveQueue.Count > 0)
            {
                lock (bulliesRemoveQueue.SyncRoot)
                {
                    while (bulliesRemoveQueue.Count > 0)
                    {
                        var reporterId = (UInt32)bulliesRemoveQueue.Dequeue();
                        if (Bullies.ContainsKey(reporterId))
                            Bullies.Remove(reporterId);
                    }
                }
            }
        }
    }
}
