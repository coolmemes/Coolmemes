using Butterfly.HabboHotel.Misc;
using Butterfly.HabboHotel.Misc.API;
using Butterfly.Messages.ClientMessages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ButterStorm.HabboHotel.Misc
{
    class LowPriorityWorker
    {
        private static Stopwatch lowPriorityProcessWatch;
        private static Int32 integerClear;
        private static Int32 tempoJson;

        internal static void Init()
        {
            lowPriorityProcessWatch = new Stopwatch();
            lowPriorityProcessWatch.Start();
        }

        internal static void Process()
        {
            if (lowPriorityProcessWatch.ElapsedMilliseconds >= 30000)
            {
                lowPriorityProcessWatch.Restart();
                integerClear++;
                tempoJson++;

                TimeSpan Uptime = DateTime.Now - OtanixEnvironment.ServerStarted;
                Console.Title = (Debugger.IsAttached ? "[DEBUG] " : "") + "Otanix para " + EmuSettings.HOTEL_LINK + " | Tempo online: " + Uptime.Days + " dias " + Uptime.Hours + " horas " + Uptime.Minutes + " minutos | Usuários onlines: " + OtanixEnvironment.GetGame().GetClientManager().clients.Count + " | Quartos Carregados: " + OtanixEnvironment.GetGame().GetRoomManager().LoadedRoomsCount;

                if (integerClear >= 120) // 1h para atualiza o cache
                {
                    FlushCache();
                }

            }
        }

        internal static void FlushCache()
        {
            try
            {
                int usersCacheCount = UsersCache.ClearCache();
                ClientMessageFactory.flushCache();
                int totalRoomDataCount = OtanixEnvironment.GetGame().GetRoomManager().ClearRoomDataCache();
                int groups = OtanixEnvironment.GetGame().GetGroup().ClearGroupsCache();

                OtanixEnvironment.GetGame().GetLandingTopUsersManager().Load();

                GC.Collect();
                GC.WaitForPendingFinalizers();

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("[" + DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString() + "] finalizado " + usersCacheCount + " usuários, " + totalRoomDataCount + " quartos e " + groups + " grupos <" + integerClear + ">.");
                Console.ResetColor();
            }
            finally
            {
                integerClear = 0;
            }
        }

        internal static void atualizaJson(TimeSpan Uptime)
        {
            try
            {
                InfoModelo dados = OtanixEnvironment.getStatusApi();
                dados.hotel = 0;
                dados.onlines = OtanixEnvironment.GetGame().GetClientManager().clients.Count;
                dados.loadRooms = OtanixEnvironment.GetGame().GetRoomManager().loadedRooms.Count;
                dados.uptime = Uptime.Days + " dias " + Uptime.Hours + " horas " + Uptime.Minutes + " minutos";

                APIexterna.enviaAlerta(dados);
            }
            finally
            {
                tempoJson = 0;
            }
        }

        internal static void DestroyJson()
        {
            InfoModelo dados = OtanixEnvironment.getStatusApi();
            dados.hotel = 0;
            dados.onlines = 0;
            dados.loadRooms = 0;
            dados.uptime = "Servidor desligado";

            APIexterna.enviaAlerta(dados);
        }
    }
}
