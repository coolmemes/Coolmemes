using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Users.Prisao
{
    class PrisaoManager
    {
        private Dictionary<uint, PrisaoUsuario> presos;

        internal void Init(IQueryAdapter dbClient)
        {
            presos = new Dictionary<uint, PrisaoUsuario>();
            dbClient.setQuery("SELECT userid, tempoRestante FROM users_presos WHERE tempoRestante > '" + OtanixEnvironment.GetUnixTimestamp() + "'");
            DataTable table = dbClient.getTable();

            if (table == null)
                return;

            foreach (DataRow row in table.Rows)
            {
                uint userid = (uint)row["userid"];
                double tempoRestante = (double)row["tempoRestante"];

                presos.Add(userid, new PrisaoUsuario(userid, tempoRestante));
            }
        }

        internal bool estaPreso(uint userid)
        {
            return (presos.ContainsKey(userid) && tempoRestantePreso(userid) > OtanixEnvironment.GetUnixTimestamp());
        }

        internal double tempoRestantePreso(uint userid)
        {
            if (!presos.ContainsKey(userid))
                return 0;

            return presos[userid].tempoRestante;
        }

        private void removeUser(uint userid)
        {
            try
            {
                presos.Remove(userid);
            }catch(Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        private void addUser(uint userid, double tempoPrisao)
        {
            try
            {
                presos.Add(userid, new PrisaoUsuario(userid, tempoPrisao));
            }catch(Exception e)
            {
                throw new Exception(e.ToString());
            }
}

        internal void removePrisao(uint userid)
        {
            if (!estaPreso(userid))
                return;

            removeUser(userid);

            using(IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM users_presos WHERE userid = '" + userid + "'");
            }
        }

        internal void prenderUsuario(uint userid, double tempoPrisao)
        {
            if (estaPreso(userid))
                return;

            addUser(userid, tempoPrisao);

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("INSERT INTO users_presos (userid, tempoRestante) VALUES ("+userid+", @tempoRestante)");
                dbClient.addParameter("tempoRestante", tempoPrisao);
                dbClient.runQuery();
            }
        }
    }
}
