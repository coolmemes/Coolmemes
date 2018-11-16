using Butterfly.HabboHotel.Users.UserDataManagement;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otanix.HabboHotel.Sanctions
{
    class SanctionManager
    {
        private readonly uint UserId;
        private readonly Dictionary<uint, Sanction> Sanctions;
        internal int ReasonId;

        public SanctionManager(uint UserId, UserData userData)
        {
            this.UserId = UserId;
            Sanctions = userData.sanctions;
        }

        internal void Clear()
        {
            Sanctions.Clear();
        }

        internal Sanction GetSanction(uint UserId)
        {
            if (Sanctions.ContainsKey(UserId))
            {
                return Sanctions[UserId];
            }

            else
            {
                return null;
            }
        }

        internal void AddSanction(uint UserId, int RemainingTime, string NextSanction)
        {
            if (Sanctions.ContainsKey(UserId))
            {
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.runFastQuery("DELETE FROM moderation_sanctions WHERE user_id = " + UserId);

                    dbClient.setQuery("INSERT INTO moderation_sanctions (user_id, reason, start_time, remaining_time, next_sanction) VALUES (@user_id, @reason, @start_time, @remaining_time, @next_sanction)");
                    dbClient.addParameter("user_id", UserId);
                    dbClient.addParameter("reason", ReasonId);
                    dbClient.addParameter("start_time", OtanixEnvironment.GetUnixTimestamp());
                    dbClient.addParameter("remaining_time", OtanixEnvironment.GetUnixTimestamp() + RemainingTime);
                    dbClient.addParameter("next_sanction", NextSanction);

                    dbClient.runQuery();
                }
                Sanctions.Remove(UserId);
                Sanctions.Add(UserId, new Sanction(UserId, ReasonId, OtanixEnvironment.GetUnixTimestamp(), OtanixEnvironment.GetUnixTimestamp() + RemainingTime, NextSanction));
            }

            else
            {
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("INSERT INTO moderation_sanctions (user_id, reason, start_time, remaining_time, next_sanction) VALUES (@user_id, @reason, @start_time, @remaining_time, @next_sanction)");
                    dbClient.addParameter("user_id", UserId);
                    dbClient.addParameter("reason", ReasonId);
                    dbClient.addParameter("start_time", OtanixEnvironment.GetUnixTimestamp());
                    dbClient.addParameter("remaining_time", OtanixEnvironment.GetUnixTimestamp() + RemainingTime);
                    dbClient.addParameter("next_sanction", NextSanction);

                    dbClient.runQuery();
                }
                Sanctions.Add(UserId, new Sanction(UserId, ReasonId, OtanixEnvironment.GetUnixTimestamp(), OtanixEnvironment.GetUnixTimestamp() + RemainingTime, NextSanction));
            }
        }


        internal string Reason
        {
            get
            {
                string r = "";
                switch (GetSanction(UserId).ReasonId)
                {
                    case 1:
                        r = "Conversazione sessuale esplicita";
                        break;

                    case 2:
                        r = "Chiedere/offrire cybersex";
                        break;

                    case 3:
                        r = "Per avere chiesto/offerto webcam o immagini a sfondo sessuale";
                        break;

                    case 36:
                        r = "Sex Links";
                        break;

                    case 31:
                        r = "Comportamento sessuale inappropriato";
                        break;

                    case 6:
                        r = "Tentativo di contatto nella vita reale";
                        break;

                    case 8:
                        r = "Per aver chiesto o condiviso informazioni personali";
                        break;

                    case 9:
                        r = "Promozione di sito scam o retroserver";
                        break;

                    case 10:
                        r = "Per aver venduto o acquistato furni/crediti/dati di accesso di un account";
                        break;

                    case 11:
                        r = "Per aver rubato furni/crediti/dati di accesso di un account";
                        break;

                    case 32:
                        r = "Hacking/scamming";
                        break;

                    case 33:
                        r = "Comportamento sospetto di frode";
                        break;

                    case 12:
                        r = "Bullismo/linguaggio offensivo";
                        break;

                    case 13:
                        r = "Nome Habbo inappropriato";
                        break;

                    case 34:
                        r = "Stanza/Gruppo/Evento Inappropriato";
                        break;

                    case 14:
                        r = "Linguaggio inappropriato";
                        break;

                    case 15:
                        r = "Promozione di droghe";
                        break;

                    case 16:
                        r = "Gioco d'azzardo";
                        break;

                    case 17:
                        r = "Impersonificazione di un membro dello Staff";
                        break;

                    case 18:
                        r = "Utente con età non consentita";
                        break;

                    case 19:
                        r = "Incitazione all'odio";
                        break;

                    case 20:
                        r = "Comportamento violento";
                        break;

                    case 21:
                        r = "Qualcuno sta minacciando di autolesionarsi";
                        break;

                    case 22:
                        r = "Flooding e interruzione di attività";
                        break;

                    case 23:
                        r = "Blocco porte";
                        break;

                    case 29:
                        r = "Raids";
                        break;

                    case 35:
                        r = "Scripting";
                        break;
                }
                return r;
            }
        }
    }
}
