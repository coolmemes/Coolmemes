using System;

namespace Butterfly.HabboHotel.RoomBots
{
    class RandomSpeech
    {
        internal string Message;
        internal UInt32 BotID;

        internal RandomSpeech(string Message, UInt32 BotID)
        {
            this.BotID = BotID;
            this.Message = Message;
        }
    }
}
