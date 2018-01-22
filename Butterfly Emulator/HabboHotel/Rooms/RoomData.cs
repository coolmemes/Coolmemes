using System;
using System.Collections.Generic;
using System.Data;
using Butterfly.Core;
using Butterfly.HabboHotel.Group;
using Butterfly.Messages;
using ButterStorm;
using Database_Manager.Database.Session_Details.Interfaces;
using System.Text;

namespace Butterfly.HabboHotel.Rooms
{
    class RoomData
    {
        internal UInt32 Id;
        internal string Type;
        internal string Name;
        internal string Owner;
        internal uint OwnerId;
        internal string Description;
        internal int Category;
        internal int State;
        internal int UsersNow;
        internal uint UsersMax;
        internal string LastModelName;
        internal string ModelName;
        internal int Score;
        internal List<string> Tags;
        internal string Password;
        internal string Wallpaper;
        internal string Floor;
        internal string Landscape;
        internal bool AllowPets;
        internal bool AllowPetsEating;
        internal bool AllowWalkthrough;
        internal bool Hidewall;
        internal bool AllowRightsOverride;
        internal bool AllowDiagonalEnabled;
        internal int FloorThickness;
        internal int WallThickness;
        internal int MuteFuse;
        internal int KickFuse;
        internal int BanFuse;
        internal uint GroupId;
        internal int BubbleMode;
        internal int BubbleType;
        internal int BubbleScroll;
        internal int TradeSettings;
        internal int AntiFloodSettings;
        internal int ChatDistance;
        internal int WallHeight;
        internal uint RollerSpeed;
        internal string temEmblema;
        internal List<int> DisabledCommands;

        internal bool roomNeedSqlUpdate;

        internal RoomEvent Event;
        private RoomModel mModel;

        internal Boolean IsPublicRoom
        {
            get
            {
                if (Type.ToLower() == "public")
                {
                    return true;
                }

                return false;
            }
        }

        internal RoomModel Model
        {
            get
            {
                if (mModel == null)
                    mModel = OtanixEnvironment.GetGame().GetRoomManager().GetModel(ModelName, Id);

                return mModel;
            }
        }

        internal void Fill(DataRow Row, uint modelId = 0)
        {
            Id = modelId != 0 ? modelId : Convert.ToUInt32(Row["id"]);
            Name = (string)Row["caption"];
            Description = (string)Row["description"];
            Type = (string)Row["roomtype"];
            Owner = (string)Row["owner"];
            OwnerId = UsersCache.getIdByUsername(Owner);
            State = (int)Row["state"];
            Category = (int)Row["category"];
            UsersNow = 0;
            UsersMax = Convert.ToUInt32(Row["users_max"]);
            ModelName = (string)Row["model_name"];
            LastModelName = ModelName;
            Score = (int)Row["score"];
            Tags = new List<string>();
            TradeSettings = (int)Row["trade_settings"];
            AllowPets = OtanixEnvironment.EnumToBool(Row["allow_pets"].ToString());
            AllowPetsEating = OtanixEnvironment.EnumToBool(Row["allow_pets_eat"].ToString());
            AllowWalkthrough = OtanixEnvironment.EnumToBool(Row["allow_walkthrough"].ToString());
            AllowRightsOverride = OtanixEnvironment.EnumToBool(Row["allow_rightsoverride"].ToString());
            AllowDiagonalEnabled = OtanixEnvironment.EnumToBool(Row["allow_diagonals"].ToString());
            AntiFloodSettings = (int)Row["antiflood_settings"];
            ChatDistance = (int)Row["chat_distance"];
            Hidewall = OtanixEnvironment.EnumToBool(Row["allow_hidewall"].ToString());
            Password = (string)Row["password"];
            Wallpaper = (string)Row["wallpaper"];
            Floor = (string)Row["floor"];
            Landscape = (string)Row["landscape"];
            FloorThickness = (int)Row["floorthickness"];
            WallThickness = (int)Row["wallthickness"];
            MuteFuse = Convert.ToInt32((string)Row["moderation_mute_fuse"]);
            KickFuse = Convert.ToInt32((string)Row["moderation_kick_fuse"]);
            BanFuse = Convert.ToInt32((string)Row["moderation_ban_fuse"]);
            GroupId = Convert.ToUInt32(Row["groupId"]);
            BubbleMode = Convert.ToInt32((string)Row["bubble_mode"]);
            BubbleType = Convert.ToInt32((string)Row["bubble_type"]);
            BubbleScroll = Convert.ToInt32((string)Row["bubble_scroll"]);
            WallHeight = Convert.ToInt32(Row["wall_height"]);
            RollerSpeed = Convert.ToUInt32(Row["roller_speed"]);
            temEmblema = (string)Row["temEmblema"];

            DisabledCommands = new List<int>();
            foreach (string StrCommandId in Row["disable_commands"].ToString().Split(','))
            {
                int CommandId = -1;
                if (!int.TryParse(StrCommandId, out CommandId))
                    continue;

                if(!DisabledCommands.Contains(CommandId))
                    DisabledCommands.Add(CommandId);
            }

            foreach (var Tag in Row["tags"].ToString().Split(','))
            {
                Tags.Add(Tag);
            }

            mModel = OtanixEnvironment.GetGame().GetRoomManager().GetModel(ModelName, Id);
        }

        internal void SaveRoomDataSettings()
        {
            if (roomNeedSqlUpdate)
            {
                string DisableCommands = "";
                foreach(int CommandId in DisabledCommands)
                {
                    DisableCommands += CommandId + ",";
                }

                if (!string.IsNullOrEmpty(DisableCommands))
                    DisableCommands = DisableCommands.Substring(0, DisableCommands.Length - 1);

                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("UPDATE rooms SET model_name = @modelname, roomtype = @type, floor = @floor, wallpaper = @wall, landscape = @landscape," +
                        " score = '" + Score + "', users_max = '" + UsersMax + "', state = '" + State + "', category = '" + Category + "',  caption = @caption," +
                        " description = @description, tags = @tags, password = @password, allow_pets = '" + TextHandling.BooleanToInt(AllowPets) + "'," +
                        " allow_pets_eat = '" + TextHandling.BooleanToInt(AllowPetsEating) + "', allow_walkthrough = '" + TextHandling.BooleanToInt(AllowWalkthrough) + "'," +
                        " allow_hidewall = '" + TextHandling.BooleanToInt(Hidewall) + "', allow_diagonals = '" + TextHandling.BooleanToInt(AllowDiagonalEnabled) + "', floorthickness = '" + FloorThickness + "', wallthickness = '" + WallThickness + "'," +
                        " moderation_mute_fuse = '" + MuteFuse + "', moderation_kick_fuse = '" + KickFuse + "', moderation_ban_fuse = '" + BanFuse + "'," +
                        " bubble_mode = '" + BubbleMode + "', bubble_type = '" + BubbleType + "', bubble_scroll = '" + BubbleScroll + "', trade_settings = '" + TradeSettings + "'," +
                        " antiflood_settings = '" + AntiFloodSettings + "', chat_distance = '" + ChatDistance + "', wall_height = '" + WallHeight + "', roller_speed = '" + RollerSpeed + "'," +
                        " disable_commands = '" + DisableCommands + "' WHERE id = " + Id);
                    dbClient.addParameter("modelname", ModelName);
                    dbClient.addParameter("type", Type);
                    dbClient.addParameter("floor", Floor);
                    dbClient.addParameter("wall", Wallpaper);
                    dbClient.addParameter("landscape", Landscape);
                    dbClient.addParameter("caption", Name);
                    dbClient.addParameter("description", Description);
                    dbClient.addParameter("tags", GenerateTags());
                    dbClient.addParameter("password", Password);
                    dbClient.runQuery();
                }

                roomNeedSqlUpdate = false;
            }
        }

        private string GenerateTags()
        {
            StringBuilder formattedTags = new StringBuilder();

            for (int i = 0; i < Tags.Count; i++)
            {
                if (i > 0)
                {
                    formattedTags.Append(",");
                }

                formattedTags.Append(Tags[i]);
            }

            return formattedTags.ToString();
        }

        internal void Serialize(ServerMessage Message, int OtherState = -1)
        {
            Message.AppendUInt(Id);
            Message.AppendString(Name);
            Message.AppendUInt(OwnerId);
            Message.AppendString(Owner);
            Message.AppendInt32(OtherState != -1 ? OtherState : State); // room state
            Message.AppendInt32(UsersNow);
            Message.AppendUInt(UsersMax);
            Message.AppendString(Description);
            Message.AppendInt32(TradeSettings);
            Message.AppendInt32(Score);
            Message.AppendInt32(0); // Ranking
            Message.AppendInt32(Category);
            Message.AppendInt32(Tags.Count);
            foreach (var Tag in Tags)
            {
                Message.AppendString(Tag);
            }
            
            Message.AppendInt32(getEnumFlags());

            if (GroupId > 0) // (getEnumFlags() & 2 > 0)
            {
                GroupItem grupo = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupId);
                if (grupo != null)
                {
                    Message.AppendUInt(GroupId); // groupdId
                    Message.AppendString(grupo.Name); // groupName
                    Message.AppendString(grupo.GroupImage); // groupBadge
                }
            }

            if (Event != null) // (getEnumFlags() & 4 > 0)
            {
                Message.AppendString(Event.Name);
                Message.AppendString(Event.Description);
                int min = 120 - (DateTime.Now - Event.StartTime).Minutes;
                if (min <= 0)
                {
                    Event.EndEvent();
                    min = 0;
                }
                Message.AppendInt32(min);
            }
        }

        private Int32 getEnumFlags()
        {
            int value = 0;

            // contiene imagen 1:

            // continee grupo 2:
            if (GroupId > 0)
                value += 2;

            // contiene grupo 4:
            if (Event != null)
                value += 4;

            // contiene privado 8:
            if (!IsPublicRoom)
                value += 8;

            // contiene mascotas 16:
            if (AllowPets)
                value += 16;

            // contiene anuncio 32:
            value += 32;

            return value;
        }
    }
}
