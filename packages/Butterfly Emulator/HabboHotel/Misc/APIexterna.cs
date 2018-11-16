using Butterfly.HabboHotel.Misc.API;
using ButterStorm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Misc
{
    class APIexterna
    {
        private static InfoModelo status;
        public static void enviaAlerta(InfoModelo dados)
        {
            string json = JsonConvert.SerializeObject(dados);

            WebClient wc = new WebClient();
            try
            {
                wc.DownloadString("http://api.kash.habbospirata.in/api/statusServidor.php?dados=" + json);
            }
            catch (WebException e)
            {
                Console.WriteLine("Ganhou Evento erro #1: " + e.Message);
            }
            catch (NotSupportedException e)
            {
                Console.WriteLine("Ganhou Evento erro #2: " + e.Message);
            }

        }

        public static void InitStatus()
        {
            status = new InfoModelo(0, string.Empty, 0, 0);
        }

        public static InfoModelo getStatus()
        {
            return status;
        }
    }
}
