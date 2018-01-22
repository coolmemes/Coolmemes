using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Mutes
{
    class MuteManager
    {
        internal Dictionary<uint, MuteUser> UsersMuted;
        internal List<uint> usersToRemove;
        internal MuteManager()
        {
            this.UsersMuted = new Dictionary<uint, MuteUser>();
            this.usersToRemove = new List<uint>();
            load();
        }
        internal void load()
        {
            using(IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT id_user, tempo_fim FROM users_mutado");
                DataTable dTable = dbClient.getTable();

                foreach (DataRow dRow in dTable.Rows)
                {
                    uint iduser = Convert.ToUInt32(dRow["id_user"]);
                    double tempoRestante = Convert.ToDouble(dRow["tempo_fim"]);

                    if (!UserIsMuted(iduser) && tempoRestante > OtanixEnvironment.GetUnixTimestamp())
                        this.UsersMuted.Add(iduser, new MuteUser(iduser, tempoRestante));
                    else
                        this.usersToRemove.Add(iduser);
                }
            }        
        }

        internal bool UserIsMuted(uint pId)
        {
            return this.UsersMuted.ContainsKey(pId);
        }

        internal void AddUserMute(uint pId, double minutes)
        {
            if (!UserIsMuted(pId))
                this.UsersMuted.Add(pId, new MuteUser(pId, OtanixEnvironment.GetUnixTimestamp() + (minutes * 60)));
        }

        internal void RemoveUserMute(uint pId)
        {
            if (UserIsMuted(pId))
            {
                this.UsersMuted.Remove(pId);
                this.usersToRemove.Add(pId);
            }
        }

        internal int HasMuteExpired(uint pId)
        {
            if (!UserIsMuted(pId))
            {
                return 0;
            }
            else if (OtanixEnvironment.GetUnixTimestamp() >= this.UsersMuted[pId].ExpireTime)
            {
                RemoveUserMute(pId);
                return -1;
            }

            return (int)(UsersMuted[pId].ExpireTime - OtanixEnvironment.GetUnixTimestamp());
        }

        internal void ClearMutes()
        {
            UsersMuted.Clear();
        }

        internal void saveToDatabase()
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                foreach (MuteUser user in UsersMuted.Values)
                {
                    dbClient.runFastQuery("REPLACE INTO users_mutado(id_user, tempo_fim) VALUES ('" + user.UserId + "','" + user.ExpireTime + "')");
                }

                foreach (uint userid in usersToRemove)
                {
                    dbClient.runFastQuery("DELETE FROM users_mutado WHERE id_user = '" + userid + "'");
                }
            }

            this.ClearMutes();
            Console.WriteLine("Mutemanager -> usuários salvos");
        }
    }
}
