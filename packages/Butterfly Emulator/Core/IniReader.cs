using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Butterfly.Core
{
    class IniReader
    {
        internal static Dictionary<string, string> ReadFile(string path)
        {
            var texts = File.ReadAllLines(path);

            return (from text in texts where text.Length != 0 && text.Contains("=") && text.Substring(0, 1) != "#" && text.Substring(0, 1) != "[" select text.Split('=')).ToDictionary(parsedText => parsedText[0], parsedText => parsedText[1]);
        }
    }
}
