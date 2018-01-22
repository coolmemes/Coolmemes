using Butterfly.HabboHotel.Alfas.Manager;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Alfas
{
    class AlfaManager
    {
        private TourManager TourManager;
        private HelpManager HelpManager;
        private BullyManager BullyManager;

        private Stopwatch moduleWatch;

        internal TourManager GetTourManager()
        {
            return TourManager;
        }

        internal HelpManager GetHelpManager()
        {
            return HelpManager;
        }

        internal BullyManager GetBullyManager()
        {
            return BullyManager;
        }

        internal AlfaManager()
        {
            this.TourManager = new TourManager();
            this.HelpManager = new HelpManager();
            this.BullyManager = new BullyManager();

            this.moduleWatch = new Stopwatch();
            this.moduleWatch.Start();
        }

        internal void OnCycle()
        {
            if (this.moduleWatch.ElapsedMilliseconds > 1000)
            {
                this.HelpManager.OnCycle();
                this.BullyManager.OnCycle();

                this.moduleWatch.Restart();
            }
        }

        internal void LoadAlfaLog(UInt32 UserId, String ReportType, String Text, Int32 VoteValue)
        {
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("INSERT INTO user_alfas_logs VALUES (NULL,'" + UserId + "','" + ReportType + "',@text,'" + VoteValue + "')");
                dbClient.addParameter("text", Text);
                dbClient.runQuery();
            }
        }
    }
}
