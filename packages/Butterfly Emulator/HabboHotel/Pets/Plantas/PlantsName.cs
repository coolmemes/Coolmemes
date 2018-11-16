using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Pets.Plantas
{
    class PlantsName
    {
        private static Random r = new Random();

        // 0,0,1,1,2,2,3,3,4,4,5
        private static string[] Colores = { "Aeneus", "Fulvus", "Viridulus", "Griseus", "Incarnatus", "Phoenicus", "Amethyst", "Cinereus", "Atamasc", "Azureus", "Cyaneus" };
        private const int MAX_COLOR = 5;
        
        // 0,0,1,1,2,2,3,3,4,4,5,6
        private static string[] Tipo = { "Blungon", "Squarg", "Stumpy", "Wailzor", "Sunspike", "Weggytum", "Shroomer", "Zuchinu", "Hairbullis", "Wystique", "Abysswirl", "Snozzle" };
        private const int MAX_TIPO = 6;

        public static string GenerateRandomName(int Rareza)
        {
            // Colores + Tipo = Rareza
            int MinColor = 0;
            int RandomColor = r.Next(0, Rareza);
            if (Rareza > 6)
            {
                MinColor = Rareza - 6;
                RandomColor = r.Next(MinColor, MAX_COLOR + 1);
            }

            return GetRandomColorByRange(RandomColor) + " " + GetRandomTipeByRange(Rareza - RandomColor);
        }

        private static int GetColorRangeByName(string ColorName)
        {
            switch(ColorName)
            {
                case "Aeneus":
                    return 0;
                case "Fulvus":
                    return 0;
                case "Viridulus":
                    return 1;
                case "Griseus":
                    return 1;
                case "Incarnatus":
                    return 2;
                case "Phoenicus":
                    return 2;
                case "Amethyst":
                    return 3;
                case "Cinereus":
                    return 3;
                case "Atamasc":
                    return 4;
                case "Azureus":
                    return 4;
                case "Cyaneus":
                    return 5;
                default:
                    return 0;
            }
        }

        private static string GetRandomColorByRange(int Rare)
        {
            int RandomNumber = r.Next(0, 2);
            switch(Rare)
            {
                case 0:
                    return RandomNumber == 0 ? "Aeneus" : "Fulvus";
                case 1:
                    return RandomNumber == 0 ? "Viridulus" : "Griseus";
                case 2:
                    return RandomNumber == 0 ? "Incarnatus" : "Phoenicus";
                case 3:
                    return RandomNumber == 0 ? "Amethyst" : "Cinereus";
                case 4:
                    return RandomNumber == 0 ? "Atamasc" : "Azureus";
                case 5:
                    return "Cyaneus";
                default:
                    return "";
            }
        }

        private static int GetTipoRangeByName(string TipoName)
        {
            switch (TipoName)
            {
                case "Blungon":
                    return 0;
                case "Squarg":
                    return 0;
                case "Stumpy":
                    return 1;
                case "Wailzor":
                    return 1;
                case "Sunspike":
                    return 2;
                case "Weggytum":
                    return 2;
                case "Shroomer":
                    return 3;
                case "Zuchinu":
                    return 3;
                case "Hairbullis":
                    return 4;
                case "Wystique":
                    return 4;
                case "Abysswirl":
                    return 5;
                case "Snozzle":
                    return 6;
                default:
                    return 0;
            }
        }

        private static string GetRandomTipeByRange(int Rare)
        {
            int RandomNumber = r.Next(0, 2);
            switch (Rare)
            {
                case 0:
                    return RandomNumber == 0 ? "Blungon" : "Squarg";
                case 1:
                    return RandomNumber == 0 ? "Stumpy" : "Wailzor";
                case 2:
                    return RandomNumber == 0 ? "Sunspike" : "Weggytum";
                case 3:
                    return RandomNumber == 0 ? "Shroomer" : "Zuchinu";
                case 4:
                    return RandomNumber == 0 ? "Hairbullis" : "Wystique";
                case 5:
                    return "Abysswirl";
                case 6:
                default:
                    return "Snozzle";
            }
        }
    }
}
