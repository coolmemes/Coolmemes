using System.Collections.Generic;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Triggers;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Conditions;
using Otanix.HabboHotel.Rooms.Wired;
using System;
using Butterfly.HabboHotel.Rooms.Games;

namespace Butterfly.HabboHotel.Rooms.Wired
{
    class WiredLoader
    {
        internal static void LoadWiredItem(RoomItem item, Room room, IQueryAdapter dbClient)
        {
            WiredLoaderSQL wired = new WiredLoaderSQL(item, room, dbClient);
            switch (item.GetBaseItem().InteractionType)
            {
                #region Cargar Causantes
                case InteractionType.triggerroomenter:
                    {
                        IWiredTrigger handler = new EntersRoom(item, room.GetWiredHandler(), room.GetRoomUserManager(), !string.IsNullOrEmpty(wired.StringSettings[0]), wired.StringSettings[0]);
                        HandleItemLoad(handler, item);
                        break;
                    }

                case InteractionType.triggerwalkonfurni:
                    {
                        IWiredTrigger handler = new WalksOnFurni(item, room.GetWiredHandler(), wired.wiredItems);
                        HandleItemLoad(handler, item);
                        break;
                    }

                case InteractionType.triggerwalkofffurni:
                    {
                        IWiredTrigger handler = new WalksOffFurni(item, room.GetWiredHandler(), wired.wiredItems);
                        HandleItemLoad(handler, item);
                        break;
                    }

                case InteractionType.triggergameend:
                    {
                        IWiredTrigger handler = new GameEnds(item, room.GetWiredHandler(), room.GetGameManager());
                        HandleItemLoad(handler, item);
                        break;
                    }

                case InteractionType.triggergamestart:
                    {
                        IWiredTrigger handler = new GameStarts(item, room.GetWiredHandler(), room.GetGameManager());
                        HandleItemLoad(handler, item);
                        break;
                    }

                case InteractionType.triggertimer:
                    {
                        int time = 50;
                        int.TryParse(wired.StringSettings[0], out time);

                        IWiredTrigger handler = new Timer(item, room.GetWiredHandler(), time, room.GetGameManager());
                        HandleItemLoad(handler, item);
                        break;
                    }

                case InteractionType.triggerrepeater:
                    {
                        int time = 50;
                        int.TryParse(wired.StringSettings[0], out time);

                        IWiredTrigger handler = new Repeater(room.GetWiredHandler(), item, time);
                        HandleItemLoad(handler, item);
                        break;
                    }

                case InteractionType.triggeronusersay:
                    {
                        IWiredTrigger handler = new UserSays(item, room.GetWiredHandler(), ((wired.StringSettings[2] == "true") ? true : false), wired.StringSettings[0], room);
                        HandleItemLoad(handler, item);
                        break;
                    }

                case InteractionType.triggerscoreachieved:
                    {
                        int score = 0;
                        int.TryParse(wired.StringSettings[0], out score);

                        IWiredTrigger handler = new ScoreAchieved(item, room.GetWiredHandler(), score, room.GetGameManager());
                        HandleItemLoad(handler, item);
                        break;
                    }

                case InteractionType.triggerstatechanged:
                    {
                        IWiredTrigger handler = new StateChanged(room.GetWiredHandler(), item, wired.wiredItems);
                        HandleItemLoad(handler, item);
                        break;
                    }

                case InteractionType.triggercollision:
                    {
                        IWiredTrigger handler = new Collision(item, room.GetWiredHandler(), room.GetRoomUserManager());
                        HandleItemLoad(handler, item);
                        break;
                    }

                case InteractionType.triggerlongperiodic:
                    {
                        int time = 10;
                        int.TryParse(wired.StringSettings[0], out time);

                        IWiredTrigger handler = new LongRepeater(room.GetWiredHandler(), item, time);
                        HandleItemLoad(handler, item);
                        break;
                    }

                case InteractionType.triggerbotreachedavtr:
                    {
                        IWiredTrigger handler = new BotAlcanzaUsuario(item, room.GetWiredHandler(), room.GetRoomUserManager(), wired.StringSettings[0]);
                        HandleItemLoad(handler, item);
                        break;
                    }

                case InteractionType.triggerbotreachedstf:
                    {
                        IWiredTrigger handler = new BotAlcanzaFurni(item, room.GetWiredHandler(), room.GetRoomUserManager(), wired.wiredItems, wired.StringSettings[0]);
                        HandleItemLoad(handler, item);
                        break;
                    }
                #endregion

                #region Cargar Efectos
                case InteractionType.actiongivescore:
                    {
                        int maxCountPerGame = 1;
                        int.TryParse(wired.StringSettings[1], out maxCountPerGame);
                        int scoreToGive = 10;
                        int.TryParse(wired.StringSettings[0], out scoreToGive);

                        IWiredTrigger action = new GiveScore(maxCountPerGame, scoreToGive, room.GetGameManager(), item);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionposreset:
                    {
                        int time = 5;
                        int.TryParse(wired.StringSettings[0], out time);

                        IWiredTrigger action = new PositionReset(wired.wiredItems, time, wired.StringSettings[1], wired.originalPositionList, room.GetRoomItemHandler(), room.GetWiredHandler(), item);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionresettimer:
                    {
                        int time = 5;
                        int.TryParse(wired.StringSettings[0], out time);

                        IWiredTrigger action = new TimerReset(room, room.GetWiredHandler(), time, item);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionshowmessage:
                    {
                        IWiredTrigger action = new ShowMessage(wired.StringSettings[0], room.GetWiredHandler(), item);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionhandiitemcustom:
                    {
                        IWiredTrigger action = new HandiCustom(wired.StringSettings[0], room.GetWiredHandler(), item);
                        HandleItemLoad(action, item);
                        break;
                    }
                case InteractionType.actioneffectcustom:
                    {
                        IWiredTrigger action = new EffectCustom(wired.StringSettings[0], room.GetWiredHandler(), item);
                        HandleItemLoad(action, item);
                        break;
                    }
                case InteractionType.actiondiamantescustom:
                    {
                        IWiredTrigger action = new DiamantesCustom(wired.StringSettings[0], room.GetWiredHandler(), item);
                        HandleItemLoad(action, item);
                        break;
                    }
                case InteractionType.actiondancecustom:
                    {
                        IWiredTrigger action = new DanceCustom(wired.StringSettings[0], room.GetWiredHandler(), item);
                        HandleItemLoad(action, item);
                        break;
                    }
                case InteractionType.actionfastwalk:
                    {
                        int time = 2;
                        int.TryParse(wired.StringSettings[0], out time);

                        IWiredTrigger action = new FastWalkCustom(room.GetWiredHandler(), item, time);
                        HandleItemLoad(action, item);
                        break;
                    }
                case InteractionType.actionfreezecustom:
                    {
                        int time = 2;
                        int.TryParse(wired.StringSettings[0], out time);

                        IWiredTrigger action = new FreezeCustom(room.GetWiredHandler(), item, time);
                        HandleItemLoad(action, item);
                        break;
                    }
                case InteractionType.actionteleportto:
                    {
                        IWiredTrigger action = new TeleportToItem(room.GetGameMap(), room.GetWiredHandler(), wired.wiredItems, 0, item);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actiontogglestate:
                    {
                        int time = 5;
                        int.TryParse(wired.StringSettings[0], out time);

                        IWiredTrigger action = new ToggleItemState(room.GetWiredHandler(), wired.wiredItems, time, item);
                        
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionmoverotate:
                    {
                        int time = 5;
                        int.TryParse(wired.StringSettings[0], out time);

                        int movementInt = 0;
                        int rotationInt = 0;
                        if (wired.StringSettings[1].Length > 0 && wired.StringSettings[1].Contains(","))
                        {
                            int.TryParse(wired.StringSettings[1].Split(',')[0], out movementInt);
                            int.TryParse(wired.StringSettings[1].Split(',')[1], out rotationInt);
                        }

                        IWiredTrigger action = new MoveRotate((MovementState)Convert.ToInt32(movementInt), (RotationState)Convert.ToInt32(rotationInt), wired.wiredItems, time, room, room.GetWiredHandler(), item);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actiongivereward:
                    {
                        int Amount = 0;
                        int Type = 0;
                        int AllUsers = 0;
                        int nInt = 1;

                        if(wired.StringSettings[1].Length > 0 && wired.StringSettings[1].Contains(",") && wired.StringSettings[1].Split(',').Length >= 4)
                        {
                            int.TryParse(wired.StringSettings[1].Split(',')[0], out Amount);
                            int.TryParse(wired.StringSettings[1].Split(',')[1], out Type);
                            int.TryParse(wired.StringSettings[1].Split(',')[2], out AllUsers);
                            int.TryParse(wired.StringSettings[1].Split(',')[3], out nInt);
                        }

                        IWiredTrigger action = new GiveReward(wired.StringSettings[0], Amount, Type, AllUsers, nInt, item);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionchase:
                    {
                        IWiredTrigger action = new Chase(wired.wiredItems, 0, room, room.GetWiredHandler(), item);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionkickuser:
                    {
                        IWiredTrigger action = new KickUser(wired.StringSettings[0], room.GetWiredHandler(), item);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionescape:
                    {
                        IWiredTrigger action = new Escape(wired.wiredItems, 0, room, room.GetWiredHandler(), item);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionjointoteam:
                    {
                        int teamid = 1;
                        int.TryParse(wired.StringSettings[0], out teamid);

                        IWiredTrigger action = new JoinToTeam(room.GetWiredHandler(), item, (Team)teamid);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionleaveteam:
                    {
                        IWiredTrigger action = new LeaveTeam(room.GetWiredHandler(), item);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actiongiveteamscore:
                    {
                        int maxCountPerGame = 1;
                        int scoreToGive = 10;
                        if (wired.StringSettings[0].Contains(","))
                        {
                            int.TryParse(wired.StringSettings[0].Split(',')[1], out maxCountPerGame);
                            int.TryParse(wired.StringSettings[0].Split(',')[0], out scoreToGive);
                        }

                        int teamid = 1;
                        int.TryParse(wired.StringSettings[1], out teamid);

                        IWiredTrigger action = new GiveTeamScore(maxCountPerGame, scoreToGive, (Team)teamid, room.GetGameManager(), item);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actioncallstacks:
                    {
                        IWiredTrigger action = new CallStacks(wired.wiredItems, room, room.GetWiredHandler(), item);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionmovetodir:
                    {
                        int movementInt = 0;
                        int rotationInt = 0;
                        int.TryParse(wired.StringSettings[0], out movementInt);
                        int.TryParse(wired.StringSettings[1], out rotationInt);

                        IWiredTrigger action = new MoveToDir(wired.wiredItems, (MovementDirection)Convert.ToInt32(movementInt), (WhenMovementBlock)Convert.ToInt32(rotationInt), room, room.GetWiredHandler(), item);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionbotmove:
                    {
                        string botName = wired.StringSettings[0];

                        uint time = 5;
                        uint.TryParse(wired.StringSettings[1], out time);

                        IWiredTrigger action = new BotMove(item.Id, room, room.GetWiredHandler(), botName, wired.wiredItems, time);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionbotwhisper:
                    {
                        string message = wired.StringSettings[0];

                        uint time = 5;
                        uint.TryParse(wired.StringSettings[1], out time);

                        bool talkorwhisper = wired.StringSettings[2].ToLower() == "true";

                        IWiredTrigger action = new BotTalkToUser(item.Id, room, room.GetWiredHandler(), message, talkorwhisper);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionbotteleport:
                    {
                        string message = wired.StringSettings[0];

                        uint time = 5;
                        uint.TryParse(wired.StringSettings[1], out time);

                        IWiredTrigger action = new BotTeleport(item.Id, room, room.GetWiredHandler(), message, wired.wiredItems, time);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionbotclothes:
                    {
                        string message = wired.StringSettings[0];

                        uint time = 5;
                        uint.TryParse(wired.StringSettings[2], out time);

                        IWiredTrigger action = new BotChangeLook(item.Id, room, room.GetWiredHandler(), message, time);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionbottalk:
                    {
                        string message = wired.StringSettings[0];

                        uint time = 5;
                        uint.TryParse(wired.StringSettings[1], out time);

                        bool talkorwhisper = wired.StringSettings[2].ToLower() == "true";

                        IWiredTrigger action = new BotTalkToAll(item.Id, room, room.GetWiredHandler(), message, talkorwhisper, time);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionbothanditem:
                    {
                        string message = wired.StringSettings[0];

                        int handitem = 0;
                        int.TryParse(wired.StringSettings[1], out handitem);

                        uint time = 5;
                        uint.TryParse(wired.StringSettings[2], out time);

                        IWiredTrigger action = new BotGiveHandItem(item.Id, room, room.GetWiredHandler(), message, handitem, time);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionbotfollowavt:
                    {
                        string botname = wired.StringSettings[0];

                        uint time = 0;
                        uint.TryParse(wired.StringSettings[1], out time);

                        bool followorstop = wired.StringSettings[2].ToLower() == "true";

                        IWiredTrigger action = new BotFollowUser(item.Id, room, room.GetWiredHandler(), botname, followorstop, time);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionmutetriggerer:
                    {
                        string botname = wired.StringSettings[0];

                        uint mutetimer = 0;
                        uint.TryParse(wired.StringSettings[1], out mutetimer);

                        uint time = 0;
                        uint.TryParse(wired.StringSettings[2], out time);

                        IWiredTrigger action = new MuteTriggerer(room.GetWiredHandler(), botname, mutetimer, time, item);
                        HandleItemLoad(action, item);
                        break;
                    }

                case InteractionType.actionmovetofurni:
                    {
                        int length = 0;
                        int.TryParse(wired.StringSettings[0], out length);

                        int direction = 0;
                        int.TryParse(wired.StringSettings[1], out direction);

                        int time = 0;
                        int.TryParse(wired.StringSettings[2], out time);

                        IWiredTrigger action = new MoveToFurni(room.GetWiredHandler(), wired.wiredItems, length, direction, time, item);
                        HandleItemLoad(action, item);
                        break;
                    }
                #endregion

                #region Condiciones
                case InteractionType.conditionfurnishaveusers:
                    {
                        IWiredCondition furniHasUsers = new FurniHasUsers(item, wired.wiredItems);
                        HandleConditionLoad(furniHasUsers, item, room);
                        break;
                    }

                case InteractionType.conditionhasfurnion:
                    {
                        int hasfurni = 0;
                        int.TryParse(wired.StringSettings[0], out hasfurni);

                        IWiredCondition furniHasFurni = new FurniHasFurni(item, wired.wiredItems, hasfurni);
                        HandleConditionLoad(furniHasFurni, item, room);
                        break;
                    }

                case InteractionType.conditiontriggeronfurni:
                    {
                        IWiredCondition triggerUserIsOnFurni = new TriggerUserIsOnFurni(item, wired.wiredItems);
                        HandleConditionLoad(triggerUserIsOnFurni, item, room);
                        break;
                    }

                case InteractionType.conditionstatepos:
                    {
                        IWiredCondition furnistatepos = new FurniStatePosMatch(item, wired.wiredItems, wired.StringSettings[0], wired.originalPositionList);
                        HandleConditionLoad(furnistatepos, item, room);
                        break;
                    }

                case InteractionType.conditiontimelessthan:
                    {
                        int time = 18;
                        int.TryParse(wired.StringSettings[0], out time);

                        IWiredCondition timeLessThan = new LessThanTimer(time, room, item);
                        HandleConditionLoad(timeLessThan, item, room);
                        break;
                    }

                case InteractionType.conditiontimemorethan:
                    {
                        int time = 18;
                        int.TryParse(wired.StringSettings[0], out time);

                        IWiredCondition timeMoreThan = new MoreThanTimer(time, room, item);
                        HandleConditionLoad(timeMoreThan, item, room);
                        break;
                    }

                case InteractionType.conditionactoringroup:
                    {
                        IWiredCondition actionInGroup = new ActorInGroup(room.RoomData.GroupId, item);
                        HandleConditionLoad(actionInGroup, item, room);
                        break;
                    }

                case InteractionType.conditionactorinteam:
                    {
                        int teamid = 1;
                        int.TryParse(wired.StringSettings[0], out teamid);

                        IWiredCondition actionInGroup = new ActorInTeam((Team)teamid, item);
                        HandleConditionLoad(actionInGroup, item, room);
                        break;
                    }

                case InteractionType.conditionusercountin:
                    {
                        uint minUsers = 1;
                        uint.TryParse(wired.StringSettings[0], out minUsers);

                        uint maxUsers = 50;
                        uint.TryParse(wired.StringSettings[1], out maxUsers);

                        IWiredCondition userCountIn = new UserCountIn(minUsers, maxUsers, item);
                        HandleConditionLoad(userCountIn, item, room);
                        break;
                    }

                case InteractionType.conditionstuffis:
                    {
                        IWiredCondition struffIn = new StuffIs(item, wired.wiredItems);
                        HandleConditionLoad(struffIn, item, room);
                        break;
                    }

                case InteractionType.conditionhandleitemid:
                    {
                        int handleId = 18;
                        int.TryParse(wired.StringSettings[0], out handleId);

                        IWiredCondition handleItem = new HandleItemUser(handleId, item);
                        HandleConditionLoad(handleItem, item, room);
                        break;
                    }

                case InteractionType.conditionnotfurnishaveusers:
                    {
                        IWiredCondition furniHasUsers = new NotFurniHasUsers(item, wired.wiredItems);
                        HandleConditionLoad(furniHasUsers, item, room);
                        break;
                    }

                case InteractionType.conditionnotfurnion:
                    {
                        int hasfurni = 0;
                        int.TryParse(wired.StringSettings[0], out hasfurni);

                        IWiredCondition furniHasFurni = new NotFurniHasFurni(item, wired.wiredItems, hasfurni);
                        HandleConditionLoad(furniHasFurni, item, room);
                        break;
                    }

                case InteractionType.conditionnottriggeronfurni:
                    {
                        IWiredCondition triggerUserIsOnFurni = new NotTriggerUserIsOnFurni(item, wired.wiredItems);
                        HandleConditionLoad(triggerUserIsOnFurni, item, room);
                        break;
                    }

                case InteractionType.conditionnotstatepos:
                    {
                        IWiredCondition furnistatepos = new NotFurniStatePosMatch(item, wired.wiredItems, wired.StringSettings[0], wired.originalPositionList);
                        HandleConditionLoad(furnistatepos, item, room);
                        break;
                    }

                case InteractionType.conditionnotingroup:
                    {
                        IWiredCondition actionInGroup = new NotActorInGroup(room.RoomData.GroupId, item);
                        HandleConditionLoad(actionInGroup, item, room);
                        break;
                    }

                case InteractionType.conditionnotinteam:
                    {
                        int teamid = 1;
                        int.TryParse(wired.StringSettings[0], out teamid);

                        IWiredCondition actionInGroup = new NotActorInTeam((Team)teamid, item);
                        HandleConditionLoad(actionInGroup, item, room);
                        break;
                    }

                case InteractionType.conditionnotusercount:
                    {
                        uint minUsers = 1;
                        uint.TryParse(wired.StringSettings[0], out minUsers);

                        uint maxUsers = 50;
                        uint.TryParse(wired.StringSettings[1], out maxUsers);

                        IWiredCondition userCountIn = new NotUserCountIn(minUsers, maxUsers, item);
                        HandleConditionLoad(userCountIn, item, room);
                        break;
                    }

                case InteractionType.conditionnotstuffis:
                    {
                        IWiredCondition struffIn = new NotStuffIs(item, wired.wiredItems);
                        HandleConditionLoad(struffIn, item, room);
                        break;
                    }

                case InteractionType.conditionwearingeffect:
                    {
                        uint effect = 0;
                        uint.TryParse(wired.StringSettings[0], out effect);

                        IWiredCondition wearingEffect = new UserWearingEffect(effect, item);
                        HandleConditionLoad(wearingEffect, item, room);
                        break;
                    }

                case InteractionType.conditionnotwearingeffect:
                    {
                        uint effect = 0;
                        uint.TryParse(wired.StringSettings[0], out effect);

                        IWiredCondition notWearingEffect = new UserNotWearingEffect(effect, item);
                        HandleConditionLoad(notWearingEffect, item, room);
                        break;
                    }

                case InteractionType.conditionwearingbadge:
                    {
                        string badge = wired.StringSettings[0];

                        IWiredCondition wearingBadge = new UserWearingBadge(badge, item);
                        HandleConditionLoad(wearingBadge, item, room);
                        break;
                    }

                case InteractionType.conditionnotwearingbadge:
                    {
                        string badge = wired.StringSettings[0];

                        IWiredCondition notWearingBadge = new UserNotWearingBadge(badge, item);
                        HandleConditionLoad(notWearingBadge, item, room);
                        break;
                    }

                case InteractionType.conditiondaterange:
                    {
                        int startDate = 0;
                        int.TryParse(wired.StringSettings[0], out startDate);

                        int endDate = 0;
                        int.TryParse(wired.StringSettings[1], out endDate);

                        IWiredCondition dateRangeActive = new DateRangeActive(startDate, endDate, item);
                        HandleConditionLoad(dateRangeActive, item, room);
                        break;
                    }
                #endregion
            }
        }

        private static void HandleItemLoad(IWiredTrigger handler, RoomItem item)
        {
            if (item.wiredHandler != null)
                item.wiredHandler.Dispose();

            item.wiredHandler = handler;
        }

        private static void HandleConditionLoad(IWiredCondition handler, RoomItem item, Room room)
        {
            if (item.wiredCondition != null)
                item.wiredCondition.Dispose();

            // room.GetWiredHandler().conditionHandler.AddConditionToTile(item.Coordinate, item.wiredCondition);
            item.wiredCondition = handler;
        }
    }
}