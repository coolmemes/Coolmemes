using System;
using Butterfly.Core;
using ButterStorm;
using Butterfly.HabboHotel.Users;
using Butterfly.Messages;
using HabboEvents;

namespace Butterfly.HabboHotel.Misc
{
    static class AntiMutant
    {
        internal static bool ValidateLook(string Look, string Gender, Habbo Habbo)
        {
            if (string.IsNullOrEmpty(Look))
                return false;

            // Si no es chico ni chica.
            if (Gender != "M" && Gender != "F")
                return false;

            // Partes imprescindibles.
            if (!Look.Contains("hd") || !Look.Contains("ch") || !Look.Contains("lg"))
                return false;

            string[] Sets = Look.Split('.');

            foreach (string Set in Sets)
            {
                string[] Parts = Set.Split('-');

                // Pocas/Demasiadas partes.
                if (Parts.Length < 2 || Parts.Length > 4)
                {
                    return false;
                }

                string Name = Parts[0];
                int Type = int.Parse(Parts[1]);
                // var Color = int.Parse(Parts[1]);

                if (Type <= 0)
                    return false;

                if (Name.Length != 2)
                    return false;

                if (OtanixEnvironment.GetGame().GetClothingManager().IsPremiumPart(Type))
                {
                    //La ropa es premium y no está comprada.
                    if (!Habbo.GetUserClothingManager().ContainsPart(Type))
                    {
                        ServerMessage Alert = new ServerMessage(Outgoing.CustomAlert);
                        Alert.AppendString("furni_placement_error");
                        Alert.AppendInt32(2);
                        Alert.AppendString("message");
                        Alert.AppendString("Está roupa é VIP, você não pode usar ela! adquira VIP e desfrute dessas e outras vantagens!");
                        Alert.AppendString("image");
                        Alert.AppendString("${image.library.url}notifications/" + EmuSettings.EVENTHA_ICON + ".png");
                        Habbo.GetClient().SendMessage(Alert);

                        return false;
                    }
                }
            }

            return true;
        }

        internal static string ReFixeateLook(string Look)
        {
            var fixedLook = "";
            if (Look.Contains("wa-"))
            {
                var parts = Look.Split('.');
                foreach (var _part in parts)
                {
                    if (_part.StartsWith("wa"))
                    {
                        fixedLook += "wa-" + _part.Split('-')[1] + ".";
                        continue;
                    }
                    fixedLook += _part + ".";
                }
            }
            else
            {
                return Look;
            }
            return fixedLook.Substring(0, fixedLook.Length - 1);
        }
    }
}
