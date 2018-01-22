using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Butterfly.Core;
using Butterfly.HabboHotel;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Pets;
using Butterfly.HabboHotel.Users;
using Butterfly.HabboHotel.Users.UserDataManagement;
using Butterfly.Messages;
using Butterfly.Messages.ClientMessages;
using Butterfly.Messages.StaticMessageHandlers;
using Butterfly.Net;
using Butterfly.Util;
using Database_Manager.Database;
using HabboEvents;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Misc;
using Butterfly;
using System.Collections;
using ButterStorm.HabboHotel.Misc;
using Azure.Encryption;
using Butterfly.Cache;
using System.Net;
using Butterfly.Scripts;
using Butterfly.HabboHotel.Misc.API;

namespace ButterStorm
{
    internal static class OtanixEnvironment
    {
        #region Fields
        /// <summary>
        /// This Configuration is for Configuration.ini on Settings
        /// </summary>
        private static ConfigurationData Configuration;

        internal static ConfigurationData GetConfig()
        {
            return Configuration;
        }

        /// <summary>
        /// Default Emulator Encoding
        /// </summary>
        private static Encoding DefaultEncoding = Encoding.UTF8;

        internal static Encoding GetDefaultEncoding()
        {
            return DefaultEncoding;
        }

        /// <summary>
        /// Sockets Information
        /// </summary>
        private static ConnectionHandeling ConnectionManager;

        internal static ConnectionHandeling GetConnectionManager()
        {
            return ConnectionManager;
        }

        /// <summary>
        /// Main Brain Process
        /// </summary>
        private static Game Game;

        internal static Game GetGame()
        {
            return Game;
        }

        /// <summary>
        /// Database for users, rooms...
        /// </summary>
        private static DatabaseManager manager;

        internal static DatabaseManager GetDatabaseManager()
        {
            return manager;
        }

        /// <summary>
        /// Shutdown process
        /// </summary>
        private static bool ShutdownInitiated;

        internal static bool ShutdownStarted
        {
            get { return ShutdownInitiated; }
        }

        internal static DateTime ServerStarted;
        internal static CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("en-GB");
        internal static MusSocket MusSystem;
        #endregion

        #region Constructor
        internal static void Initialize()
        {
            ServerStarted = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("\n                                                                 ");
            Console.WriteLine(@"        $$$$$$\    $$\                         $$\                ");
            Console.WriteLine(@"       $$  __$$\   $$ |                        \__|               ");
            Console.WriteLine(@"       $$ /  $$ |$$$$$$\    $$$$$$\  $$$$$$$\  $$\ $$\   $$\      ");
            Console.WriteLine(@"       $$ |  $$ |\_$$  _|   \____$$\ $$  __$$\ $$ |\$$\ $$  |     ");
            Console.WriteLine(@"       $$ |  $$ |  $$ |     $$$$$$$ |$$ |  $$ |$$ | \$$$$  /      ");
            Console.WriteLine(@"       $$ |  $$ |  $$ |$$\ $$  __$$ |$$ |  $$ |$$ | $$  $$<       ");
            Console.WriteLine(@"        $$$$$$  |  \$$$$  |\$$$$$$$ |$$ |  $$ |$$ |$$  /\$$\      ");
            Console.WriteLine(@"        \______/    \____/  \_______|\__|  \__|\__|\__/  \__|     ");
            Console.WriteLine("\n                                                                 ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(@"   © 2016 - 2017 - Todos os direitos reservados ao Thiago Araujo.");
            Console.WriteLine(@"                                                                 ");
            Console.ForegroundColor = ConsoleColor.Gray;

            try
            {
                #region Starting
                Configuration = new ConfigurationData(Path.Combine(Application.StartupPath, @"Settings/configuration.ini"));

                UsersCache.Initialize();
                LowPriorityWorker.Init();
                APIexterna.InitStatus();
                LanguageLocale.Init();
                ChatCommandRegister.Init();
                PetLocale.Init();
                #endregion
                #region MySQL Configuration
                var starts = DateTime.Now;
                Logging.WriteLine("[Otanix] @ Conectando na database...");

                manager = new DatabaseManager(uint.Parse(GetConfig().data["db.pool.maxsize"]),
                    int.Parse(GetConfig().data["db.pool.minsize"]));
                manager.setServerDetails(
                    GetConfig().data["db.hostname"],
                    uint.Parse(GetConfig().data["db.port"]),
                    GetConfig().data["db.username"],
                    GetConfig().data["db.password"],
                    GetConfig().data["db.name"]);
                manager.init();

                var timeUsed2 = DateTime.Now - starts;
                Logging.WriteLine("[Otanix] @ Conectado com sucesso na database! (" + timeUsed2.Seconds + " s, " + timeUsed2.Milliseconds + " ms)");
                #endregion
                #region Cycles Configuration
                starts = DateTime.Now;

                StaticClientMessageHandler.Initialize();
                ClientMessageFactory.Init();

                var timeUsed3 = DateTime.Now - starts;
                Logging.WriteLine("[Otanix] @ Iniciando os ciclos! (" + timeUsed3.Seconds + " s, " + timeUsed3.Milliseconds +
                        " ms)");

                Game = new Game();
                Game.ContinueLoading();
                #endregion
                #region Connections Configuration
                ConnectionManager = new ConnectionHandeling(int.Parse(GetConfig().data["game.tcp.port"]),
                    int.Parse(GetConfig().data["game.tcp.conlimit"]),
                    int.Parse(GetConfig().data["game.tcp.conperip"]),
                    GetConfig().data["game.tcp.enablenagles"].ToLower() == "true");
                ConnectionManager.init();
                ConnectionManager.Start();

                Handler.Initialize(RsaKeyHolder.N, RsaKeyHolder.D, RsaKeyHolder.E);
                Logging.WriteLine("[Otanix] @ RSA Crypto iniciada!");

                if (GetConfig().data["mus.enable"].ToLower() == "true")
                    MusSystem = new MusSocket(int.Parse(GetConfig().data["mus.tcp.port"]));
                #endregion
                #region Last Process
                var TimeUsed = DateTime.Now - ServerStarted;
                Logging.WriteLine("[Otanix] @ ENVIRONMENT -> PRONTO! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");
                Console.Title = "Otanix Emulador ~ Versão privada por Thiago Araujo para " + EmuSettings.HOTEL_LINK;

                if (Debugger.IsAttached)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Logging.WriteLine("[Otanix] @ Alerta: Servidor está em DEBUG, console ativado!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Logging.WriteLine("[Otanix] @ Alerta: Servidor não está em DEBUG, console desativado!");
                    Logging.DisablePrimaryWriting(false);
                }

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("[Otanix] @ Alerta: Pressione alguma tecla para ativar o console de comandos.");
                Console.ForegroundColor = ConsoleColor.White;
                #endregion
            }
            catch (KeyNotFoundException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logging.WriteLine("[Otanix] @ Alerta de erro: Cheque seu arquivo de configurações - Alguns valores parecem estar faltando.");
                Logging.WriteLine("[Otanix] @ Alerta de erro: Pressione alguma tecla para finalizar ...");
                Logging.WriteLine(e.ToString());
                Console.ReadKey(true);
                Destroy();
            }
            catch (InvalidOperationException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logging.WriteLine("[Otanix] @ Alerta de erro: Falha ao iniciar o OtanixEmulator: " + e.Message);
                Logging.WriteLine("[Otanix] @ Alerta de erro: Pressione alguma tecla para finalizar ...");

                Console.ReadKey(true);
                Destroy();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Otanix] @ Alerta de erro: Erro fatal na inicialização: " + e);
                Console.WriteLine("[Otanix] @ Alerta de erro: Pressione alguma tecla para finalizar ...");

                Console.ReadKey();
                Environment.Exit(1);
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void Destroy()
        {
            Logging.WriteLine("[Otanix] @ Alerta: Finalizando Otanix Environment...");

            if (GetGame() != null)
            {
                GetGame().Destroy();
                Game = null;
            }

            if (GetConnectionManager() != null)
            {
                Logging.WriteLine("[Otanix] @ Alerta: Finalizando o controle de conexão.");
                GetConnectionManager().Destroy();
            }

            if (manager != null)
            {
                Logging.WriteLine("[Otanix] @ Alerta: Finalizando o controle da database.");
                manager.destroy();
            }

            Logging.WriteLine("[Otanix] @ Alerta: Processo de desligamento feito com sucesso, desligando...");
        }

        internal static void PreformShutDown()
        {
            if (ShutdownInitiated)
            {
                Console.WriteLine("[Otanix] @ Alerta: Processo de shutdown inicializado. Retornando ...");
                return;
            }

            //LowPriorityWorker.DestroyJson();

            ShutdownInitiated = true;

            var builder = new StringBuilder();
            var ShutdownStart = DateTime.Now;
            var MessaMessage = DateTime.Now;

            AppendTimeStampWithComment(ref builder, MessaMessage, "Hotel pre-warning");

            Game.StopGameLoop();
            Console.Write(@"[Otanix] @ Alerta: Game loop parado");

            var ConnectionClose = DateTime.Now;
            Console.WriteLine("[Otanix] @ Alerta: Servidor sendo finalizado...");

            Console.Title = "[Otanix] @ Alerta: Otanix foi desligado com sucesso!";

            GetConnectionManager().Destroy();
            AppendTimeStampWithComment(ref builder, ConnectionClose, "[Otanix] @ Alerta: Socket fechado");

            var sConnectionClose = DateTime.Now;
            GetGame().GetClientManager().CloseAll();
            AppendTimeStampWithComment(ref builder, sConnectionClose, "[Otanix] @ Alerta: Furni pre-save and connection close");

            var RoomRemove = DateTime.Now;
            Console.WriteLine("[Otanix] @ Alerta: SALVANDO OS QUARTOS");
            Game.GetRoomManager().RemoveAllRooms();
            AppendTimeStampWithComment(ref builder, RoomRemove, "Room destructor");
            var DbSave = DateTime.Now;

            AppendTimeStampWithComment(ref builder, DbSave, "Database pre-save");

            var connectionShutdown = DateTime.Now;
            ConnectionManager.Destroy();
            AppendTimeStampWithComment(ref builder, connectionShutdown, "Connection shutdown");

            var gameDestroy = DateTime.Now;
            Game.Destroy();
            AppendTimeStampWithComment(ref builder, gameDestroy, "Game destroy");

            var databaseDeconstructor = DateTime.Now;

            Console.WriteLine("[Otanix] @ Alerta: Finalizando o controle da database...");
            manager.destroy();

            Game.GetMuteManager().saveToDatabase();

            AppendTimeStampWithComment(ref builder, databaseDeconstructor, "Database shutdown");

            var timeUsedOnShutdown = DateTime.Now - ShutdownStart;
            builder.AppendLine("[Otanix] @ Alerta: Tempo total do processo de fechamento: " + TimeSpanToString(timeUsedOnShutdown));
            builder.AppendLine();

            Logging.LogShutdown(builder);

            Console.WriteLine("[Otanix] @ Alerta: Sistema completamente fechado, até a proxima!");
           
            Environment.Exit(Environment.ExitCode);
        }

        internal static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static InfoModelo getStatusApi()
        {
            return APIexterna.getStatus();
        }

        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (dateTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
        }
        #endregion

        #region Methods
        internal static bool EnumToBool(string Enum)
        {
            return (Enum == "1");
        }

        internal static string BoolToEnum(bool Bool)
        {
            if (Bool)
            {
                return "1";
            }

            return "0";
        }

        internal static int GetUnixTimestamp()
        {
            var ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            var unixTime = ts.TotalSeconds;

            return (int) unixTime;
        }

        internal static double GetUnixTimestampInMili()
        {
            var ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            var unixTime = ts.TotalMilliseconds;

            return (double)unixTime;
        }

        internal static string FilterInjectionChars(string Input)
        {
            return FilterInjectionChars(Input, false);
        }

        internal static string FilterFigure(string figure)
        {
            if (figure.Any(character => !isValid(character)))
            {
                return "lg-3023-1335.hr-828-45.sh-295-1332.hd-180-4.ea-3168-89.ca-1813-62.ch-235-1332";
            }

            return figure;
        }

        private static bool isValid(char character)
        {
            return new List<char>(new[]
            {
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l',
                'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
                'y', 'z', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-', '.'
            }).Contains(character);
        }

        internal static string FilterInjectionChars(string Input, bool AllowLinebreaks)
        {
            if (Input == null)
            {
                return null;
            }

            Input = Input.Replace(Convert.ToChar(1), ' ');
            Input = Input.Replace(Convert.ToChar(2), ' ');
            Input = Input.Replace(Convert.ToChar(9), ' ');

            if (!AllowLinebreaks)
            {
                Input = Input.Replace(Convert.ToChar(13), ' ');
            }

            return Input;
        }

        internal static bool IsValidAlphaNumeric(string inputStr)
        {
            if (string.IsNullOrEmpty(inputStr))
            {
                return false;
            }

            return inputStr.All(t => isValid(t));
        }

        internal static string TimeSpanToString(TimeSpan span)
        {
            return span.Seconds + " s, " + span.Milliseconds + " ms";
        }

        internal static void AppendTimeStampWithComment(ref StringBuilder builder, DateTime time, string text)
        {
            builder.AppendLine(text + " => [" + TimeSpanToString(DateTime.Now - time) + "]");
        }

        internal static string GenerateRandomString()
        {
            Random obj = new Random();
            string posibles = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            int longitud = posibles.Length;
            char letra;
            int longitudnuevacadena = 32;
            string nuevacadena = "";
            for (int i = 0; i < longitudnuevacadena; i++)
            {
                letra = posibles[obj.Next(longitud)];
                nuevacadena += letra.ToString();
            }

            return nuevacadena;
        }

        internal static uint prisaoId()
        {
            return EmuSettings.PRISAOID;
        }

        internal static bool ContainsHTMLCode(string Message)
        {
            if (Message.Contains("<font") || Message.Contains("<i>") || Message.Contains("<b>") || Message.Contains("<u>"))
                return true;

            return false;
        }
        #endregion
    }
}