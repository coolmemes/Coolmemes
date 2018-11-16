using Butterfly.Core;
using Butterfly.HabboHotel.GameClients;
using Butterfly.Messages;
using ButterStorm;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Misc
{
    class NotificaStaff
    {
        internal static void NotificaUser(string imagem, string mensagem, GameClient Session)
        {
            ServerMessage Alert = new ServerMessage(Outgoing.CustomAlert);
            Alert.AppendString("furni_placement_error");
            Alert.AppendInt32(2);
            Alert.AppendString("message");
            Alert.AppendString(mensagem);
            Alert.AppendString("image");
            Alert.AppendString("${image.library.url}notifications/" + imagem + ".png");
            Session.SendMessage(Alert);
        }
        internal static void Notifica(string imagem, string mensagem)
        {
            ServerMessage Alert = new ServerMessage(Outgoing.CustomAlert);
            Alert.AppendString("furni_placement_error");
            Alert.AppendInt32(2);
            Alert.AppendString("message");
            Alert.AppendString(mensagem);
            Alert.AppendString("image");
            Alert.AppendString("${image.library.url}notifications/" + imagem + ".png");
            OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(Alert, "fuse_sa", 0);
        }
        internal static void Notifica(GameClient Session, bool outroQuarto = false)
        {
            ServerMessage newm = new ServerMessage(Outgoing.GeneratingNotification);
            newm.AppendString("info." + EmuSettings.HOTEL_LINK);
            newm.AppendInt32(5);
            newm.AppendString("image");
            newm.AppendString(LanguageLocale.GetValue("bc.imagem"));
            newm.AppendString("title");
            newm.AppendString(LanguageLocale.GetValue("bc.titulo.notificacao"));
            newm.AppendString("message");
            newm.AppendString("<i>" + (outroQuarto ? LanguageLocale.GetValue("bc.mensagem1") : LanguageLocale.GetValue("bc.mensagem")) + "<br><br>• <b>" + Session.GetHabbo().Username + "</b></i>");
            newm.AppendString("linkTitle");
            newm.AppendString("OK");
            newm.AppendString("linkUrl");
            newm.AppendString("event:");
            Session.SendMessage(newm);
        }
    }
}
