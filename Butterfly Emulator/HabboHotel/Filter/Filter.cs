using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Butterfly.HabboHotel.Filter
{
    class Filter
    {
        private static Dictionary<string, Dictionary<string, string>> charsDictionary;

        public static string Default { get; private set; }

        public static void Initialize()
        {
            charsDictionary = new Dictionary<string, Dictionary<string, string>>();

            foreach (string line in File.ReadAllLines(@"Settings/BlackWords/filter.ini", Encoding.Default))
            {
                string[] array = line.Split('=');
                string mode = array[0];
                string jsonStr = string.Join("=", array.Skip(1));

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                dynamic items = serializer.Deserialize<object[]>(jsonStr);

                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (object[] item in items)
                {
                    string key = item[0].ToString();
                    string value = "";

                    if (item.Length > 1)
                        value = item[1].ToString();

                    if(!dic.ContainsKey(key))
                        dic.Add(key, value);
                }

                if (charsDictionary.ContainsKey(mode))
                    continue;

                if (Default == null)
                    Default = mode;

                charsDictionary.Add(mode, dic);
            }
        }

        public static string Replace(string mode, string str)
        {
            str = RemoveDiacritics(str).ToLower();

            if (!charsDictionary.ContainsKey(mode) || string.IsNullOrEmpty(str))
                return str;

            return charsDictionary[mode].Aggregate(str, (current, array) => current.Replace(array.Key, array.Value));
        }

        private static String RemoveDiacritics(String s)
        {
            string normalizedString = s.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)) 
                stringBuilder.Append(c);

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
