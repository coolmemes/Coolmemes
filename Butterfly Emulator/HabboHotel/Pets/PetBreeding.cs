using Butterfly.Messages;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Pets
{
    class PetBreeding
    {
        internal static String GetPetByPetId(uint Id)
        {
            switch(Id)
            {
                case 0:
                    return "dog";
                case 1:
                    return "cat";
                case 2:
                    return "pig";
                case 3:
                    return "terrier";
                case 4:
                    return "bear";
                default:
                    return "";
            }
        }

        internal static Int32 GetPetIdByPet(string Pet)
        {
            switch(Pet)
            {
                case "dog":
                    return 0;
                case "cat":
                    return 1;
                case "pig":
                    return 2;
                case "terrier":
                    return 3;
                case "bear":
                    return 4;
                default:
                    return -1;
            }
        }

        internal static Int32 GetBreedingByPet(int Id)
        {
            switch(Id)
            {
                case 0:
                    return 29;
                case 1:
                    return 28;
                case 2:
                    return 30;
                case 3:
                    return 25;
                case 4:
                    return 24;
                default:
                    return -1;
            }
        }

        internal static ServerMessage GetMessage(uint FurniId, Pet Pet1, Pet Pet2)
        {
            ServerMessage Message = new ServerMessage(Outgoing.PetBreedingPanel);
            Message.AppendUInt(FurniId);
            Message.AppendUInt(Pet1.PetId);
            Message.AppendString(Pet1.Name);
            Message.AppendInt32(Pet1.Level);
            Message.AppendString(Pet1.Look);
            Message.AppendString(Pet1.OwnerName);
            Message.AppendUInt(Pet2.PetId);
            Message.AppendString(Pet2.Name);
            Message.AppendInt32(Pet2.Level);
            Message.AppendString(Pet2.Look);
            Message.AppendString(Pet2.OwnerName);
            Message.AppendInt32(4); // 4 razas (ÉPICO, RARO, NADA COMÚN, COMÚN)

            Message.AppendInt32(1);
            if (Pet1.Type == 0)
            {
                Message.AppendInt32(dogEpicRace.Length);
                foreach (int value in dogEpicRace)
                    Message.AppendInt32(value);
            }
            else if (Pet1.Type == 1)
            {
                Message.AppendInt32(catEpicRace.Length);
                foreach (int value in catEpicRace)
                    Message.AppendInt32(value);
            }
            else if (Pet1.Type == 2)
            {
                Message.AppendInt32(pigEpicRace.Length);
                foreach (int value in pigEpicRace)
                    Message.AppendInt32(value);
            }
            else if (Pet1.Type == 3)
            {
                Message.AppendInt32(terrierEpicRace.Length);
                foreach (int value in terrierEpicRace)
                    Message.AppendInt32(value);
            }
            else if (Pet1.Type == 4)
            {
                Message.AppendInt32(bearEpicRace.Length);
                foreach (int value in bearEpicRace)
                    Message.AppendInt32(value);
            }

            Message.AppendInt32(2);
            if (Pet1.Type == 0)
            {
                Message.AppendInt32(dogRareRace.Length);
                foreach (int value in dogRareRace)
                    Message.AppendInt32(value);
            }
            else if (Pet1.Type == 1)
            {
                Message.AppendInt32(catRareRace.Length);
                foreach (int value in catRareRace)
                    Message.AppendInt32(value);
            }
            else if (Pet1.Type == 2)
            {
                Message.AppendInt32(pigRareRace.Length);
                foreach (int value in pigRareRace)
                    Message.AppendInt32(value);
            }
            else if (Pet1.Type == 3)
            {
                Message.AppendInt32(terrierRareRace.Length);
                foreach (int value in terrierRareRace)
                    Message.AppendInt32(value);
            }
            else if (Pet1.Type == 4)
            {
                Message.AppendInt32(bearRareRace.Length);
                foreach (int value in bearRareRace)
                    Message.AppendInt32(value);
            }

            Message.AppendInt32(3);
            if (Pet1.Type == 0)
            {
                Message.AppendInt32(dogNoRareRace.Length);
                foreach (int value in dogNoRareRace)
                    Message.AppendInt32(value);
            }
            else if (Pet1.Type == 1)
            {
                Message.AppendInt32(catNoRareRace.Length);
                foreach (int value in catNoRareRace)
                    Message.AppendInt32(value);
            }
            else if (Pet1.Type == 2)
            {
                Message.AppendInt32(pigNoRareRace.Length);
                foreach (int value in pigNoRareRace)
                    Message.AppendInt32(value);
            }
            else if (Pet1.Type == 3)
            {
                Message.AppendInt32(terrierNoRareRace.Length);
                foreach (int value in terrierNoRareRace)
                    Message.AppendInt32(value);
            }
            else if (Pet1.Type == 4)
            {
                Message.AppendInt32(bearNoRareRace.Length);
                foreach (int value in bearNoRareRace)
                    Message.AppendInt32(value);
            }

            Message.AppendInt32(94);
            if (Pet1.Type == 0)
            {
                Message.AppendInt32(dogNormalRace.Length);
                foreach (int value in dogNormalRace)
                    Message.AppendInt32(value);
            }
                else if (Pet1.Type == 1)
            {
                Message.AppendInt32(catNormalRace.Length);
                foreach (int value in catNormalRace)
                    Message.AppendInt32(value);
            }
                else if (Pet1.Type == 2)
            {
                Message.AppendInt32(pigNormalRace.Length);
                foreach (int value in pigNormalRace)
                    Message.AppendInt32(value);
            }
            else if (Pet1.Type == 3)
            {
                Message.AppendInt32(terrierNormalRace.Length);
                foreach (int value in terrierNormalRace)
                    Message.AppendInt32(value);
            }
            else if (Pet1.Type == 4)
            {
                Message.AppendInt32(bearNormalRace.Length);
                foreach (int value in bearNormalRace)
                    Message.AppendInt32(value);
            }

            Message.AppendInt32(GetBreedingByPet((int)Pet1.Type));

            return Message;
        }

        internal static int[] dogEpicRace = new int[3] { 18,19,20 };
        internal static int[] dogRareRace = new int[6] { 12,13,14,15,16,17 };
        internal static int[] dogNoRareRace = new int[5] { 7,8,9,10,11 };
        internal static int[] dogNormalRace = new int[6] { 1,2,3,4,5,6 };

        internal static int[] catEpicRace = new int[3] { 18,19,20 };
        internal static int[] catRareRace = new int[6] { 12,13,14,15,16,17 };
        internal static int[] catNoRareRace = new int[5] { 7,8,9,10,11 };
        internal static int[] catNormalRace = new int[6] {1,2,3,4,5,6 };

        internal static int[] pigEpicRace = new int[3] { 18, 19, 20 };
        internal static int[] pigRareRace = new int[6] { 12, 13, 14, 15, 16, 17 };
        internal static int[] pigNoRareRace = new int[5] { 7, 8, 9, 10, 11 };
        internal static int[] pigNormalRace = new int[6] { 1, 2, 3, 4, 5, 6 };

        internal static int[] terrierEpicRace = new int[3] { 17, 18, 19 };
        internal static int[] terrierRareRace = new int[4] { 10, 14, 15, 16 };
        internal static int[] terrierNoRareRace = new int[6] { 11, 12, 13, 4, 5, 6 };
        internal static int[] terrierNormalRace = new int[6] { 0, 1, 2, 7, 8, 9 };

        internal static int[] bearEpicRace = new int[3] { 3, 10, 11 };
        internal static int[] bearRareRace = new int[6] { 12, 13, 15, 16, 17, 18 };
        internal static int[] bearNoRareRace = new int[5] { 7, 8, 9, 14, 19 };
        internal static int[] bearNormalRace = new int[6] { 0, 1, 2, 4, 5, 6 };
    }
}
