using Butterfly.Util;
using Database_Manager.Database.Session_Details.Interfaces;

namespace Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces
{
    interface IWiredTrigger
    {
        void Dispose();
        void SaveToDatabase(QueryChunk wiredInserts);
    }
}
