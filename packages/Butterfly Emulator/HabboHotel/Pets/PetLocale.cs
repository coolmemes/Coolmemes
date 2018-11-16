using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Butterfly.Core;

namespace Butterfly.HabboHotel.Pets
{
    class PetLocale
    {
        private static Dictionary<string, string[]> values;

        internal static void Init()
        {
            var unparsedValues = IniReader.ReadFile(Path.Combine(Application.StartupPath, @"System/locale.pets.ini"));
            values = new Dictionary<string, string[]>();

            foreach (var pair in unparsedValues)
            {
                values.Add(pair.Key, pair.Value.Split(','));
            }
        }

        internal static string[] GetValue(string key)
        {
            string[] value;
            if (values.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return new string[] { "NO KEY FOUND FOR " + key };
            }
        }
    }
}
