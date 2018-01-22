using System;
using System.Net.NetworkInformation;
using System.Linq;
using System.Security.Permissions;
using Butterfly.Core;
using ButterStorm;
using System.IO;
using System.Net;

namespace Butterfly
{
    internal class Program
    {
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        [STAThread]
        internal static void Main()
        {
            InitEnvironment();

            while (true)
            {
                Console.CursorVisible = true;

                if (Logging.DisabledState)
                    Console.Write(@"[Otanix] @ Digite o comando: ");

                ConsoleCommandHandeling.InvokeCommand(Console.ReadLine());
            }
        }

        [MTAThread]
        internal static void InitEnvironment()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.CursorVisible = false;
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += MyHandler;

            OtanixEnvironment.Initialize();
   
        }
            static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Logging.DisablePrimaryWriting(true);
            var e = (Exception)args.ExceptionObject;
            Logging.LogCriticalException("[Otanix] @ Alerta de erro: EXCEÇÃO CRÍTICA DO SISTEMA: " + e);
            OtanixEnvironment.PreformShutDown();
        }
    }
}
