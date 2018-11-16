using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Pets
{
    class PetOrders
    {
        public static Dictionary<uint, List<int>> PetsOrders;

        public static void Init(IQueryAdapter dbClient)
        {

            PetsOrders = new Dictionary<uint, List<int>>();

            dbClient.setQuery("SELECT * FROM pets_commands");
            var Table = dbClient.getTable();

            foreach (DataRow dRow in Table.Rows)
            {
                List<int> comandsIds = new List<int>();
                foreach (string parse in ((string)dRow["command"]).Split(';'))
                {
                    comandsIds.Add(int.Parse(parse));
                }

                PetsOrders.Add(Convert.ToUInt32(dRow["pet_race"]), comandsIds);
            }
        }

        public static bool PetCanDoCommand(uint raceId, int Level, string Command)
        {
            if(PetsOrders.ContainsKey(raceId))
            {
                for (int i = 0; i < PetsOrders[raceId].Count; i++ )
                {
                    if (PetsOrders[raceId][i] == ConvertCommandToInt32(Command))
                    {
                        return ((Level + 3) > i);
                    }
                }
            }

            return false;
        }

        private static Int32 ConvertCommandToInt32(string Command)
        {
            switch(Command)
            {
                case "DESCANSA":
                    return 0;
                case "HABLA":
                    return 10;
                case "JUEGA":
                    return 11;
                case "CALLA":
                    return 12;
                case "A CASA":
                    return 13;
                case "BEBE":
                    return 14;
                case "IZQUIERDA":
                    return 15;
                case "DERECHA":
                    return 16;
                case "FÚTBOL":
                    return 17;
                case "ARRODÍLLATE":
                    return 18;
                case "BOTA":
                    return 19;
                case "SIÉNTATE":
                    return 1;
                case "ESTATUA":
                    return 20;
                case "BAILA":
                    return 21;
                case "GIRA":
                    return 22;
                case "ENCIENDE TV":
                    return 23;
                case "ADELANTE":
                    return 24;
                //case "IZQUIERDA":
                //    return 25;
                //case "DERECHA":
                //    return 26;
                case "RELAX":
                    return 27;
                case "CROA":
                    return 28;
                case "INMERSIÓN":
                    return 29;
                case "TÚMBATE":
                    return 2;
                case "SALUDA":
                    return 30;
                case "MARCHA":
                    return 31;
                case "GRAN SALTO":
                    return 32;
                case "BAILE POLLO":
                    return 33;
                case "TRIPLE SALTO":
                    return 34;
                case "MUESTRA ALAS":
                    return 35;
                case "ECHA FUEGO":
                    return 36;
                case "PLANEA":
                    return 37;
                case "ANTORCHA":
                    return 38;
                case "VEN AQUí":
                    return 3;
                case "CAMBIA VUELO":
                    return 40;
                case "VOLTERETA":
                    return 41;
                case "ANILLO FUEGO":
                    return 42;
                case "COMER":
                    return 43;
                case "MOVER COLA":
                    return 44;
                case "Cuenta":
                    return 45;
                case "Cruzar":
                    return 46;
                case "PIDE":
                    return 4;
                case "HAZ EL MUERTO":
                    return 5;
                case "QUIETO":
                    return 6;
                case "SÍGUEME":
                    return 7;
                case "LEVANTA":
                    return 8;
                case "SALTA":
                    return 9;
            }
            return 0;
        }
    }
}
