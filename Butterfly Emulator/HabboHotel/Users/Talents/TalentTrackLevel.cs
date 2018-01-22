using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Items;
using Butterfly.Messages;
using ButterStorm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Users.Talents
{
    class TalentTrackLevel
    {
        internal Dictionary<uint, TalentTrackSubLevel> TalentTrackSubLevelDict;
        internal List<string> TalentActionGift;
        internal List<Item> TalentFurniGift;

        internal TalentTrackLevel(string ach_ids, string gifts)
        {
            this.TalentTrackSubLevelDict = new Dictionary<uint, TalentTrackSubLevel>();
            this.TalentActionGift = new List<string>();
            this.TalentFurniGift = new List<Item>();

            if (ach_ids.Length > 0)
            {
                foreach (string value in ach_ids.Split(';'))
                {
                    uint achId = uint.Parse(value.Split('-')[0]);
                    int achLevel = int.Parse(value.Split('-')[1]);

                    this.TalentTrackSubLevelDict.Add(achId, new TalentTrackSubLevel(achId, achLevel));
                }
            }

            if (gifts.Length > 0)
            {
                if (gifts.Split(';')[0].Length > 0)
                {
                    foreach (string value in gifts.Split(';')[0].Split('-'))
                    {
                        this.TalentActionGift.Add(value);
                    }
                }

                if (gifts.Split(';')[1].Length > 0)
                {
                    foreach (string value in gifts.Split(';')[1].Split('-'))
                    {
                        this.TalentFurniGift.Add(OtanixEnvironment.GetGame().GetItemManager().GetItem(uint.Parse(value)));
                    }
                }
            }
        }

        internal void Serialize(ServerMessage Message, GameClient Session, bool ValidLevel)
        {
            Message.AppendInt32(this.TalentTrackSubLevelDict.Count);
            foreach (TalentTrackSubLevel talenttracksublevel in this.TalentTrackSubLevelDict.Values)
            {
                talenttracksublevel.Serialize(Message, Session, ValidLevel);
            }

            Message.AppendInt32(this.TalentActionGift.Count);
            foreach (string value in this.TalentActionGift)
            {
                Message.AppendString(value);
            }
            Message.AppendInt32(this.TalentFurniGift.Count);
            foreach (Item item in this.TalentFurniGift)
            {
                Message.AppendString(item.Name);
                Message.AppendInt32(0);
            }
        }
    }
}
