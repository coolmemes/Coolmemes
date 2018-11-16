using System;
using Butterfly.HabboHotel;
using Butterfly.HabboHotel.Pets;
using Butterfly.Messages;
using Butterfly.Messages.ClientMessages;
using ButterStorm;
using System.Text;
using System.IO;
using System.Threading;
using System.Runtime;
using ConnectionManager;
using Butterfly.HabboHotel.Rooms;
using Database_Manager.Database;
using HabboEvents;
using Butterfly.HabboHotel.SoundMachine;
using Database_Manager.Database.Session_Details;
using Database_Manager.Database.Session_Details.Interfaces;
using System.Data;
using ButterStorm.HabboHotel.Misc;
using Butterfly.HabboHotel.Users.Messenger;
using Butterfly.HabboHotel.Filter;
using Butterfly.HabboHotel.Misc;
using Butterfly.HabboHotel.GameClients;
using Butterfly.Scripts;

namespace Butterfly.Core
{
    class ConsoleCommandHandeling
    {
        internal static void InvokeCommand(string inputData)
        {
            if (string.IsNullOrEmpty(inputData) && Logging.DisabledState)
                return;

            if (Logging.DisabledState == false)
            {
                Logging.DisabledState = true;
                return;
            }

            try
            {
                #region Command parsing

                if (inputData != null)
                {
                    var parameters = inputData.Split(' ');

                    switch (parameters[0])
                    {
                        case "fecha":
                        case "fechar":
                        case "desligar":
                        case "stop":
                        case "desliga":
                        case "shutdown":
                            {
                                Logging.DisablePrimaryWriting(true);
                                Console.WriteLine("Shutdown process started successfully at " + DateTime.Now.ToShortTimeString());
                                OtanixEnvironment.PreformShutDown();

                                break;
                            }
                        case "alert":
                            {
                                var Notice = inputData.Substring(6);

                                var HotelAlert = new ServerMessage(Outgoing.SendNotif);
                                HotelAlert.AppendString(LanguageLocale.GetValue("console.noticefromadmin") + "\n\n" +
                                                                 Notice);
                                HotelAlert.AppendString("");
                                getGame().GetClientManager().QueueBroadcaseMessage(HotelAlert);
                                Console.WriteLine("[" + Notice + "] sent");

                                break;
                            }

                        case "help":
                        case "ajuda":
                            {
                                Console.WriteLine("shutdown - Cierra el emulador guardando todos los datos");
                                Console.WriteLine("alert (message) - Envía una alerta al hotel");
                                Console.WriteLine("flush");
                                Console.WriteLine("     cache - Refresca la caché del emulador.");
                                Console.WriteLine("     consoleoffmessages - Refresca los mensajes almacenados de los usuarios offline de la consola.");
                                Console.WriteLine("     emusettings - Refresca el archivo values.ini");
                                Console.WriteLine("     commands - Refresca el archivo commands.ini y locale.pets.ini");
                                Console.WriteLine("     language - Refresca el archivo locale.ini y los welcome.ini");
                                Console.WriteLine("     settings");
                                Console.WriteLine("          ranks - Coge el número de rangos de la tabla ranks.");
                                Console.WriteLine("          blackwords - Vuelve a cachear los datos de la tabla server_blackwords.");
                                Console.WriteLine("          modcategories - Vuelve a cachear los datos de las tablas moderations.");
                                Console.WriteLine("          refreshitems - Vuelve a cachear los datos de la tabla items_base.");
                                Console.WriteLine("          bans - Vuelve a cachear los datos de la tabla bans.");
                                Console.WriteLine("          catalog - Vuelve a cachear los datos de la tabla catalog_items y catalog_pages.");
                                Console.WriteLine("          youtube_tv - Vuelve a cachear los datos de la tabla youtube_videos.");
                                Console.WriteLine("          modeldata - Vuelve a cachear los datos de la tabla room_models.");
                                Console.WriteLine("     console - Limpia el aspecto visual de la consola.");
                                Console.WriteLine("     memory - Limpia los datos de la caché del emu que no se están usando.");
                                break;
                            }

                        case "flush":
                            {
                                if (parameters.Length < 2)
                                {
                                    Console.WriteLine("You need to specify a parameter within your command. Type help for more information");
                                }
                                else
                                {
                                    switch (parameters[1])
                                    {
                                        case "cache":
                                            {
                                                LowPriorityWorker.FlushCache();
                                                break;
                                            }

                                        case "consoleoffmessages":
                                            {
                                                Console.WriteLine("Se han borrado un total de " + MessengerChat.MessagesCount + " mensajes");
                                                MessengerChat.ClearMessages();

                                                break;
                                            }

                                        case "emusettings":
                                            {
                                                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                                                {
                                                    EmuSettings.Initialize(dbClient);
                                                    StaffChat.Initialize(dbClient);
                                                }

                                                Console.WriteLine("Emu Settings reloaded.");
                                                break;
                                            }

                                        case "commands":
                                            {
                                                Console.WriteLine("Flushing commands");
                                                ChatCommandRegister.Init();
                                                PetLocale.Init();
                                                Console.WriteLine("Commands flushed");

                                                break;
                                            }

                                        case "language":
                                            {
                                                Console.WriteLine("Flushing language files");
                                                LanguageLocale.Init();
                                                Console.WriteLine("Language files flushed");

                                                break;
                                            }

                                        case "settings":
                                            {
                                                if (parameters.Length < 3)
                                                    Console.WriteLine("You need to specify a parameter within your command. Type help for more information");
                                                else
                                                {
                                                    switch (parameters[2])
                                                    {
                                                        case "ranks":
                                                            {
                                                                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                                                                {
                                                                    Ranks.LoadMaxRankId(dbClient);
                                                                }

                                                                Console.WriteLine("Rangos actualizados con éxito.");

                                                                break;
                                                            }

                                                        case "blackwords":
                                                            {
                                                                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                                                                {
                                                                    BlackWordsManager.Load(dbClient);
                                                                }

                                                                Console.WriteLine("BlackWords actualizados con éxito.");

                                                                break;
                                                            }

                                                        case "modcategories":
                                                            {
                                                                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                                                                {
                                                                    OtanixEnvironment.GetGame().GetModerationTool().LoadMessagePresets(dbClient);
                                                                    OtanixEnvironment.GetGame().GetModerationTool().LoadModActions(dbClient);
                                                                }

                                                                break;
                                                            }

                                                        case "refreshitems":
                                                            {
                                                                getGame().GetItemManager().reloaditems();
                                                                Console.WriteLine("Item definition reloaded");
                                                                break;
                                                            }

                                                        case "bans":
                                                            {
                                                                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                                                                {
                                                                    OtanixEnvironment.GetGame().GetBanManager().LoadBans(dbClient);
                                                                }

                                                                Console.WriteLine("Bans flushed");

                                                                break;
                                                            }

                                                        case "catalog":
                                                            {
                                                                Console.WriteLine("Flushing catalog settings");

                                                                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                                                                {
                                                                    getGame().GetCatalog().Initialize(dbClient);
                                                                    getGame().GetCatalogPremium().Initialize(dbClient);
                                                                }
                                                                getGame().GetCatalog().InitCache();

                                                                ServerMessage Message = new ServerMessage(Outgoing.UpdateShop);
                                                                Message.AppendBoolean(false); // timer?
                                                                OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(Message);

                                                                Console.WriteLine("Catalog flushed");

                                                                break;
                                                            }

                                                        case "youtube_tv":
                                                            {
                                                                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                                                                {
                                                                    getGame().GetYoutubeManager().Initialize(dbClient);
                                                                }

                                                                break;
                                                            }

                                                        case "modeldata":
                                                            {
                                                                Console.WriteLine("Flushing modeldata");
                                                                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                                                                {
                                                                    getGame().GetRoomManager().LoadModels(dbClient);
                                                                }
                                                                Console.WriteLine("Models flushed");

                                                                break;
                                                            }
                                                    }
                                                }
                                                break;
                                            }

                                        case "console":
                                            {
                                                Console.Clear();
                                                break;
                                            }

                                        case "memory":
                                            {
                                                GC.Collect();
                                                Console.WriteLine("Memory flushed");

                                                break;
                                            }

                                        default:
                                            {
                                                unknownCommand(inputData);
                                                break;
                                            }
                                    }
                                }

                                break;
                            }

                        case "hacks":
                            {
                                switch (parameters[1])
                                {
                                    case "dice":
                                        {
                                            string Username = parameters[2];
                                            uint Number = uint.Parse(parameters[3]);

                                            GameClient Session = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
                                            if (Session == null)
                                            {
                                                Console.WriteLine("Invalid Username");
                                                break;
                                            }

                                            if (Number < 1 || Number > 6)
                                            {
                                                Console.WriteLine("Invalid Number");
                                                break;
                                            }

                                            Session.GetHabbo().DiceNumber = Number;

                                            break;
                                        }

                                    case "packet":
                                        {
                                            string Username = parameters[2];

                                            GameClient Session = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
                                            if (Session == null)
                                            {
                                                Console.WriteLine("Invalid Username");
                                                break;
                                            }

                                            Session.PacketSaverEnable = !Session.PacketSaverEnable;
                                            Console.WriteLine("Actual State " + Session.PacketSaverEnable + " for user " + Username);

                                            break;
                                        }
                                }

                                break;
                            }

                        default:
                            {
                                unknownCommand(inputData);
                                break;
                            }

                    }
                }

                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in command [" + inputData + "]: " + e);
            }

            Console.WriteLine();
        }

        private static void unknownCommand(string command)
        {
            Console.WriteLine(command + " is an unknown or unsupported command. Type help for more information");
        }

        internal static Game getGame()
        {
            return OtanixEnvironment.GetGame();
        }
    }
}
