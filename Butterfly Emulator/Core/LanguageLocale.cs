using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using ButterStorm;
using System.Data;
using System.Text.RegularExpressions;

namespace Butterfly.Core
{
    class LanguageLocale
    {
        private static Dictionary<string, string> values;
        internal static bool welcomeAlertEnabled;
        internal static int welcomeAlertType;
        internal static string welcomeAlertImage;
        internal static string welcomeAlert;

        internal static void Init()
        {
            values = IniReader.ReadFile(Path.Combine(Application.StartupPath, @"System/locale.ini"));
            InitWelcomeMessage();
        }

        private static void InitWelcomeMessage()
        {
            var configFile = IniReader.ReadFile(Path.Combine(Application.StartupPath, @"System/welcome_config.ini"));
            welcomeAlertEnabled = configFile["welcome.alert.enabled"] == "true";
            welcomeAlertType = int.Parse(configFile["welcome.alert.type"]);
            welcomeAlertImage = configFile["welcome.alert.image"];

            if (welcomeAlertEnabled)
            {
                welcomeAlert = File.ReadAllText(Path.Combine(Application.StartupPath, @"System/welcome_message.ini"));
            }
        }

        internal static string GetValue(string value)
        {
            if (values.ContainsKey(value))
                return values[value];
            else
                return "";
                //throw new MissingLocaleException("Missing language locale for [" + value + "]");
        }
    }

    class MissingLocaleException : Exception
    {
        public MissingLocaleException(string message)
            : base(message)
        {

        }
    }
}
