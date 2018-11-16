using System.Collections.Generic;
using System.Data;
using System.Linq;
using Database_Manager.Database.Session_Details.Interfaces;
using System;

namespace Butterfly.HabboHotel.Pets
{
    public class PetRace
    {
        public int RaceId;
        public int Color1;
        public int Color2;
        public bool Has1Color;
        public bool Has2Color;

        public static List<PetRace> Races;

        public static void Init(IQueryAdapter dbClient)
        {
            dbClient.setQuery("SELECT * FROM pets_racesoncatalogue");
            var Table = dbClient.getTable();

            Races = new List<PetRace>();
            foreach (DataRow Race in Table.Rows)
            {
                var R = new PetRace
                {
                    RaceId = (int) Race["raceid"],
                    Color1 = (int) Race["color1"],
                    Color2 = (int) Race["color2"],
                    Has1Color = ((string) Race["has1color"] == "1"),
                    Has2Color = ((string) Race["has2color"] == "1")
                };
                Races.Add(R);
            }
        }

        public static List<PetRace> GetRacesForRaceId(int sRaceId)
        {
            return Races.Where(R => R.RaceId == sRaceId).ToList();
        }

        public static bool RaceGotRaces(int sRaceId)
        {
            if (GetRacesForRaceId(sRaceId).Count > 0)
                return true;
            else
                return false;
        }

        public static int RandomRace(int PetType)
        {
            int randRace = 0;
            List<PetRace> pRaceList = GetRacesForRaceId(PetType);

            if (pRaceList.Count > 0)
                randRace = pRaceList[new Random().Next(pRaceList.Count)].Color1;

            return randRace;
        }

        // private string[] petRaces = new String[] { "FF7B3A", "FF9763", "FFCDB3", "F59500", "FBBD5C", "FEE4B2", "EDD400", "F5E759", "FBF8B1", "84A95F", "B0C993", "DBEFC7", "65B197", "91C7B5", "C5EDDE", "7F89B2", "98A1C5", "CAD2EC", "A47FB8", "C09ED5", "DBC7E9", "BD7E9D", "DA9DBD", "ECC6DB", "DD7B7D", "F08B90", "F9BABF", "ABABAB", "D4D4D4", "FFFFFF", "D98961", "DFA281", "F1D2C2", "D5B35F", "DAC480", "FCFAD3", "EAA7AF", "86BC40", "E8CE25", "8E8839", "888F67", "5E9414", "84CE84", "96E75A", "88E70D", "B99105", "C8D71D", "838851", "C08337", "83A785", "E6AF26", "ECFF99", "94FFF9", "ABC8E5", "F2E5CC", "D2FF00" };

        public static string[] PetIdByName = new string[] { "perro", "gato", "cocodrilo", "terrier", "oso", "jabali", "leon", "rinoceronte", "araña", "tortuga", "pollito", "rana", "dragon", "", "mono", "caballo", "planta", "conejo", "conejosiniestro", "conejoaburrido", "conejomaniaco", "palomablanca", "palomanegra", "monorojo", "minioso", "miniterrier", "gnomo", "", "minigato", "miniperro", "minicerdo", "haloompa", "piedra", "terodactilo", "dinosaurio" };
    }
}
