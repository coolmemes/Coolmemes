using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Classifications
{
    class Puntuations
    {
        private RoomItem Item;
        private Boolean enabled;
        private Double lastUpdate;

        internal List<PuntuationRow> puntuationRows;

        internal string EnableValue
        {
            get
            {
                return enabled ? "1" : "0";
            }
        }

        internal void ChangeEnable()
        {
            this.enabled = !this.enabled;
        }

        internal Puntuations(RoomItem _Item)
        {
            this.Item = _Item;
            this.puntuationRows = new List<PuntuationRow>();
            this.enabled = OtanixEnvironment.EnumToBool(Item.ExtraData.Split(';')[0]);
            if (Item.ExtraData.Split(';').Length > 1)
                this.lastUpdate = Convert.ToDouble(Item.ExtraData.Split(';')[1]);
            else
                this.lastUpdate = OtanixEnvironment.GetUnixTimestamp();

            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT names, puntuation FROM items_classifications WHERE itemid = '" + Item.Id + "' ORDER BY puntuation DESC LIMIT 15");
                foreach (DataRow dRow in dbClient.getTable().Rows)
                {
                    this.puntuationRows.Add(new PuntuationRow((string)dRow["names"], Convert.ToUInt32(dRow["puntuation"])));
                }
            }
        }

        internal void Reestructure()
        {
            List<PuntuationRow> backupRows = new List<PuntuationRow>();
            backupRows.AddRange(puntuationRows);

            puntuationRows.Clear();
            puntuationRows = backupRows.OrderByDescending(x => x.puntuation).Take(15).ToList();
            Item.UpdateState();
        }

        internal void AddPointsToClass()
        {
            switch (Item.GetBaseItem().Name)
            {
                case "highscore_classic*1": // Máximas Puntuaciones Clásicas: Globales
                case "highscore_classic*2": // Máximas Puntuaciones Clásicas: Diarias
                case "highscore_classic*3": // Máximas Puntuaciones Clásicas: Semanales
                case "highscore_classic*4": // Máximas Puntuaciones Clásicas: Mensuales
                    {
                        foreach (RoomUser user in Item.GetRoom().GetRoomUserManager().GetRoomUsers())
                        {
                            if (user.team == Team.none || user.classPoints <= 0)
                                continue;

                            PuntuationRow pRow = new PuntuationRow(user.GetUsername(), user.classPoints);
                            this.puntuationRows.Add(pRow);
                        }

                        this.Reestructure();

                        break;
                    }

                case "highscore_perteam*1": // Máximas Puntuaciones por Equipo: Globales
                case "highscore_perteam*2": // Máximas Puntuaciones por Equipo: Diarias
                case "highscore_perteam*3": // Máximas Puntuaciones por Equipo: Semanales
                case "highscore_perteam*4": // Máximas Puntuaciones por Equipo: Mensuales
                    {
                        string[] names = new string[4] { "", "", "", "" };
                        uint[] puntuations = new uint[4] { 0, 0, 0, 0 };

                        foreach (RoomUser user in Item.GetRoom().GetRoomUserManager().GetRoomUsers())
                        {
                            // red, green, blue, yellow
                            if (user.team == Team.none || user.classPoints <= 0)
                                continue;

                            names[(int)user.team - 1] += user.GetUsername() + ",";
                            puntuations[(int)user.team - 1] += user.classPoints;
                        }

                        for (int i = 0; i < 4; i++)
                        {
                            if (names[i].Length <= 0)
                                continue;

                            string newName = names[i].Substring(0, names[i].Length - 1);

                            for (int ii = 0; ii < this.puntuationRows.Count; ii++)
                            {
                                if (this.puntuationRows[ii].names == newName)
                                {
                                    this.puntuationRows[ii].puntuation += puntuations[i];
                                    this.puntuationRows[ii].needUpdate = true;
                                    this.Reestructure();
                                    return;
                                }
                            }

                            PuntuationRow pRow = new PuntuationRow(newName, puntuations[i]);
                            this.puntuationRows.Add(pRow);
                        }

                        this.Reestructure();

                        break;
                    }

                case "highscore_mostwin*1": // Máximas Puntuaciones (Más Victorias): Globales
                case "highscore_mostwin*2": // Máximas Puntuaciones (Más Victorias): Diarias
                case "highscore_mostwin*3": // Máximas Puntuaciones (Más Victorias): Semanales
                case "highscore_mostwin*4": // Máximas Puntuaciones (Más Victorias): Mensuales
                    {
                        foreach (RoomUser user in Item.GetRoom().GetRoomUserManager().GetRoomUsers())
                        {
                            if (user.team == Team.none || user.classPoints <= 0)
                                continue;

                            for (int ii = 0; ii < this.puntuationRows.Count; ii++)
                            {
                                if (this.puntuationRows[ii].names == user.GetUsername())
                                {
                                    this.puntuationRows[ii].puntuation++;
                                    this.puntuationRows[ii].needUpdate = true;
                                    this.Reestructure();
                                    return;
                                }
                            }

                            PuntuationRow pRow = new PuntuationRow(user.GetUsername(), 1);
                            this.puntuationRows.Add(pRow);
                        }

                        this.Reestructure();

                        break;
                    }
            }
        }

        internal void CheckTimeOnline()
        {
            switch (Item.GetBaseItem().Name)
            {
                // globales:
                case "highscore_classic*1": // Máximas Puntuaciones Clásicas: Globales
                case "highscore_perteam*1": // Máximas Puntuaciones por Equipo: Globales
                case "highscore_mostwin*1": // Máximas Puntuaciones (Más Victorias): Globales
                    {
                        break;
                    }

                // diarias:
                case "highscore_classic*2": // Máximas Puntuaciones Clásicas: Diarias
                case "highscore_perteam*2": // Máximas Puntuaciones por Equipo: Diarias
                case "highscore_mostwin*2": // Máximas Puntuaciones (Más Victorias): Diarias
                    {
                        Double totalTime = OtanixEnvironment.GetUnixTimestamp() - this.lastUpdate;
                        if (totalTime > 86400)
                        {
                            this.lastUpdate = OtanixEnvironment.GetUnixTimestamp();
                            this.puntuationRows.Clear();
                            Item.UpdateState();

                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                dbClient.runFastQuery("DELETE FROM items_classifications WHERE itemid = '" + Item.Id + "'");
                            }
                        }
                        break;
                    }

                // semanales:
                case "highscore_classic*3": // Máximas Puntuaciones Clásicas: Semanales
                case "highscore_perteam*3": // Máximas Puntuaciones por Equipo: Semanales
                case "highscore_mostwin*3": // Máximas Puntuaciones (Más Victorias): Semanales
                    {
                        Double totalTime = OtanixEnvironment.GetUnixTimestamp() - this.lastUpdate;
                        if (totalTime > 604800)
                        {
                            this.lastUpdate = OtanixEnvironment.GetUnixTimestamp();
                            this.puntuationRows.Clear();
                            Item.UpdateState();

                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                dbClient.runFastQuery("DELETE FROM items_classifications WHERE itemid = '" + Item.Id + "'");
                            }
                        }
                        break;
                    }

                // mensuales:
                case "highscore_classic*4": // Máximas Puntuaciones Clásicas: Mensuales
                case "highscore_perteam*4": // Máximas Puntuaciones por Equipo: Mensuales
                case "highscore_mostwin*4": // Máximas Puntuaciones (Más Victorias): Mensuales
                    {
                        Double totalTime = OtanixEnvironment.GetUnixTimestamp() - this.lastUpdate;
                        if (totalTime > 2419200)
                        {
                            this.lastUpdate = OtanixEnvironment.GetUnixTimestamp();
                            this.puntuationRows.Clear();
                            Item.UpdateState();

                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                dbClient.runFastQuery("DELETE FROM items_classifications WHERE itemid = '" + Item.Id + "'");
                            }
                        }
                        break;
                    }
            }
        }

        internal string SavePuntuations()
        {
            string extradata = this.EnableValue + ";" + this.lastUpdate;

            if (this.puntuationRows.Count > 0)
            {
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.runFastQuery("DELETE FROM items_classifications WHERE itemid = '" + Item.Id + "'");

                    foreach (PuntuationRow pRows in this.puntuationRows)
                    {
                        dbClient.setQuery((pRows.needUpdate ? "REPLACE" : "INSERT") + " INTO items_classifications (itemid, names, puntuation) VALUES (" + Item.Id + ",@names," + pRows.puntuation + ")");
                        dbClient.addParameter("names", pRows.names);
                        dbClient.runQuery();
                    }
                }
            }
            return extradata;
        }

        internal int getMainInt()
        {
            switch (Item.GetBaseItem().Name)
            {
                case "highscore_classic*1": // Máximas Puntuaciones Clásicas: Globales
                case "highscore_classic*2": // Máximas Puntuaciones Clásicas: Diarias
                case "highscore_classic*3": // Máximas Puntuaciones Clásicas: Semanales
                case "highscore_classic*4": // Máximas Puntuaciones Clásicas: Mensuales
                    {
                        return 2;
                    }

                case "highscore_perteam*1": // Máximas Puntuaciones por Equipo: Globales
                case "highscore_perteam*2": // Máximas Puntuaciones por Equipo: Diarias
                case "highscore_perteam*3": // Máximas Puntuaciones por Equipo: Semanales
                case "highscore_perteam*4": // Máximas Puntuaciones por Equipo: Mensuales
                    {
                        return 0;
                    }

                case "highscore_mostwin*1": // Máximas Puntuaciones (Más Victorias): Globales
                case "highscore_mostwin*2": // Máximas Puntuaciones (Más Victorias): Diarias
                case "highscore_mostwin*3": // Máximas Puntuaciones (Más Victorias): Semanales
                case "highscore_mostwin*4": // Máximas Puntuaciones (Más Victorias): Mensuales
                    {
                        return 1;
                    }
            }
            return 0;
        }

        internal int getSecondInt()
        {
            int lastInt = Convert.ToInt32(Item.GetBaseItem().Name.Substring(Item.GetBaseItem().Name.Length - 1, 1));
            return (lastInt - 1);
        }
    }
}