using Butterfly.HabboHotel.GameClients;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Misc
{
    class MensagensAutomaticas
    {
        internal static void MostraNotificacaoUser(GameClient Session)
        {
            int UltimaNoti = (OtanixEnvironment.GetUnixTimestamp() - Session.GetHabbo().UltimaNotificacao);
            if (UltimaNoti > 1800)
            {
                Session.GetHabbo().exibeNotifi = true;
                Session.GetHabbo().UltimaNotificacao = OtanixEnvironment.GetUnixTimestamp();
                concluiNotificacao(Session);
            }
            else
                Session.GetHabbo().exibeNotifi = false;
        }

        internal static void concluiNotificacao(GameClient Session)
        {
            if (Session.GetHabbo().exibeNotifi == true)
            {
                string frase = "";
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("SELECT * FROM otanix_emulador_mensagens_automaticas ORDER BY rand() LIMIT 1");
                    var Data = dbClient.getRow();
                    if (Data == null)
                        return;
                    frase = (string)Data["frase"];
                }

                var imgTexto = frase.Split('/');         

                ServerMessage Alert = new ServerMessage(Outgoing.CustomAlert);
                Alert.AppendString("furni_placement_error");
                Alert.AppendInt32(2);
                Alert.AppendString("message");
                Alert.AppendString(imgTexto[1]);
                Alert.AppendString("image");
                Alert.AppendString("${image.library.url}notifications/" + imgTexto[0] + ".png");
                Session.GetHabbo().GetClient().SendMessage(Alert);
                Session.GetHabbo().exibeNotifi = false;
            }
        }
    }
}
