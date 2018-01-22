using ButterStorm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Web;
using System.IO.Compression;
using System.Web.Script.Serialization;

namespace Butterfly.HabboHotel.Camera
{
    class Camera
    {
        internal static string Decompiler(byte[] input)
        {
            // Primero desofuscar el ZLIB
            return DecompressBytes(input);
        }

        private static string DecompressBytes(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes, 2, bytes.Length - 2))
            using (var inflater = new DeflateStream(stream, CompressionMode.Decompress))
            using (var streamReader = new StreamReader(inflater))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}