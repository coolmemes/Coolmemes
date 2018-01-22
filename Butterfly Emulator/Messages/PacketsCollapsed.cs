using ButterStorm;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.Messages
{
    class PacketsCollapsed
    {
        public static Dictionary<int, double> PacketsList;

        public static void Initialize()
        {
            PacketsList = new Dictionary<int, double>();

            PacketsList.Add(Incoming.LoadFirstRoomData, 1);
            PacketsList.Add(Incoming.LoadHeightMap, 1);
            PacketsList.Add(Incoming.AddUserToRoom, 1);
        }
    }

    class PacketsUserLogs
    {
        // Variable que logueará el packetID y el tiempo de su último envío.
        public Dictionary<int, double> PacketsList;

        public PacketsUserLogs()
        {
            // Iniciamos la variable que se utilizará como caché.
            PacketsList = new Dictionary<int, double>();

            // Calculamos el tiempo actual.
            double actualUnix = OtanixEnvironment.GetUnixTimestamp();

            // Establecemos valores.
            foreach (int IncomingId in PacketsCollapsed.PacketsList.Keys)
            {
                PacketsList.Add(IncomingId, actualUnix);
            }
        }

        public bool CanReceivePacket(int PacketId)
        {
            // Si el packet no pertenece a la prioridad, se puede recibir sin problemas.
            if (!PacketsList.ContainsKey(PacketId))
                return true;

            // Calculamos el tiempo actual.
            double actualUnix = OtanixEnvironment.GetUnixTimestamp();

            // Si ha pasado más de X segundos desde que recibimos este packet.
            bool value = (actualUnix - PacketsList[PacketId]) > PacketsCollapsed.PacketsList[PacketId];

            // Si ha pasado, actualizamos el valor.
            if (value)
                PacketsList[PacketId] = actualUnix;

            // Devolvemos V/F.
            return value;
        }

        public void Destroy()
        {
            PacketsList.Clear();
            PacketsList = null;
        }
    }
}
