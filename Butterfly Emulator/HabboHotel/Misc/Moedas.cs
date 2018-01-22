using Butterfly.HabboHotel.GameClients;
using ButterStorm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Misc
{
    class Moedas
    {
        internal static void GiveCycleMoedas(GameClient Session)
        {
            if (EmuSettings.HOTEL_LUCRATIVO == false || EmuSettings.HOTEL_LUCRATIVO_DARMOEDAS == false)
                return;

            if ((OtanixEnvironment.GetUnixTimestamp() - Session.GetHabbo().MoedasCycleUpdate) > EmuSettings.HOTEL_LUCRATIVO_MOEDAS_TEMPO * 60)
            {
                int qtdMoedas = (int)(Session.GetHabbo().IsPremium() ? EmuSettings.HOTEL_LUCRATIVO_QUANTIDADE_MOEDAS * 2 : EmuSettings.HOTEL_LUCRATIVO_QUANTIDADE_MOEDAS);

                Session.GetHabbo().MoedasCycleUpdate = OtanixEnvironment.GetUnixTimestamp();
                Session.GetHabbo().darMoedas(qtdMoedas);
            }
        }
    }
}
