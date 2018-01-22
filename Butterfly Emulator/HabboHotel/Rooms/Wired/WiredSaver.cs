using System.Collections.Generic;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Triggers;
using Butterfly.Messages;
using ButterStorm;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Conditions;
using Butterfly.HabboHotel.GameClients;
using HabboEvents;
using System;
using Otanix.HabboHotel.Rooms.Wired;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.Core;
using Butterfly.HabboHotel.Filter;

namespace Butterfly.HabboHotel.Rooms.Wired
{
    class WiredSaver
    {
        internal static void HandleSave(GameClient Session, uint itemID, Room room, ClientMessage clientMessage)
        {
            if (room == null || room.GetRoomItemHandler() == null)
                return;

            var item = room.GetRoomItemHandler().GetItem(itemID);
            if (item == null)
                return;

            if (item.wiredHandler != null)
            {
                item.wiredHandler.Dispose();
                item.wiredHandler = null;
            }

            var type = item.GetBaseItem().InteractionType;
            switch (type)
            {
                #region Causantes
                case InteractionType.triggeronusersay:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var isOnlyOwner = (clientMessage.PopWiredInt32() == 1);
                        var message = clientMessage.PopFixedString();

                        IWiredTrigger handler = new UserSays(item, room.GetWiredHandler(), isOnlyOwner, message, room);
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);
                        break;
                    }
                case InteractionType.triggerwalkonfurni:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var message = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);

                        IWiredTrigger handler = new WalksOnFurni(item, room.GetWiredHandler(), items);
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);
                        break;
                    }
                case InteractionType.triggerwalkofffurni:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var message = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);

                        IWiredTrigger handler = new WalksOffFurni(item, room.GetWiredHandler(), items);
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);
                        break;
                    }
                case InteractionType.triggergameend:
                    {
                        IWiredTrigger handler = new GameEnds(item, room.GetWiredHandler(), room.GetGameManager());
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.triggergamestart:
                    {
                        IWiredTrigger handler = new GameStarts(item, room.GetWiredHandler(), room.GetGameManager());
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);
                        break;
                    }
                case InteractionType.triggertimer:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var cycles = clientMessage.PopWiredInt32();

                        IWiredTrigger handler = new Timer(item, room.GetWiredHandler(), cycles, room.GetGameManager());
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);

                        break;
                    }
                case InteractionType.triggerrepeater:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var cycleTimes = clientMessage.PopWiredInt32();

                        IWiredTrigger handler = new Repeater(room.GetWiredHandler(), item, cycleTimes);
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);

                        break;
                    }

                case InteractionType.triggerroomenter:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var users = clientMessage.PopFixedString();

                        IWiredTrigger handler = new EntersRoom(item, room.GetWiredHandler(), room.GetRoomUserManager(), !string.IsNullOrEmpty(users), users);
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.triggerscoreachieved:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var score = clientMessage.PopWiredInt32();

                        IWiredTrigger handler = new ScoreAchieved(item, room.GetWiredHandler(), score, room.GetGameManager());
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);

                        break;
                    }

                case InteractionType.triggerstatechanged:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var junk3 = clientMessage.PopWiredBoolean();
                        var junk2 = clientMessage.PopWiredBoolean();

                        int furniAmount;
                        var items = GetItems(clientMessage, room, out furniAmount);
                        var delay = clientMessage.PopWiredInt32();

                        IWiredTrigger handler = new StateChanged(room.GetWiredHandler(), item, items);
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);

                        break;
                    }

                case InteractionType.triggercollision:
                    {
                        IWiredTrigger handler = new Collision(item, room.GetWiredHandler(), room.GetRoomUserManager());
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.triggerlongperiodic:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var cycleTimes = clientMessage.PopWiredInt32();

                        IWiredTrigger handler = new LongRepeater(room.GetWiredHandler(), item, cycleTimes);
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);

                        break;
                    }

                case InteractionType.triggerbotreachedavtr:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var botname = clientMessage.PopFixedString();

                        IWiredTrigger handler = new BotAlcanzaUsuario(item, room.GetWiredHandler(), room.GetRoomUserManager(), botname);
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);

                        break;
                    }

                case InteractionType.triggerbotreachedstf:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var botname = clientMessage.PopFixedString();

                        int furniAmount;
                        var items = GetItems(clientMessage, room, out furniAmount);

                        IWiredTrigger handler = new BotAlcanzaFurni(item, room.GetWiredHandler(), room.GetRoomUserManager(), items, botname);
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);

                        break;
                    }
                #endregion

                #region Efectos
                case InteractionType.actiongivescore:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var points = clientMessage.PopWiredInt32();
                        var games = clientMessage.PopWiredInt32();

                        IWiredTrigger action = new GiveScore(games, points, room.GetGameManager(), item);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);

                        break;
                    }

                case InteractionType.actionposreset:
                    {
                        var junk = clientMessage.PopWiredInt32();

                        var state = clientMessage.PopWiredInt32();
                        var direction = clientMessage.PopWiredInt32();
                        var position = clientMessage.PopWiredInt32();

                        var junk3 = clientMessage.PopFixedString();

                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();

                        IWiredTrigger action = new PositionReset(items, delay, state + "," + direction + "," + position, new Dictionary<uint, OriginalItemLocation>(), room.GetRoomItemHandler(), room.GetWiredHandler(), item);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);

                        break;
                    }

                case InteractionType.actionresettimer:
                    {

                        var junk = clientMessage.PopWiredInt32();
                        var junk3 = clientMessage.PopWiredBoolean();
                        var junk2 = clientMessage.PopWiredBoolean();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();

                        IWiredTrigger action = new TimerReset(room, room.GetWiredHandler(), delay, item);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);

                        break;
                    }

                case InteractionType.actionshowmessage:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var message = OtanixEnvironment.FilterInjectionChars(clientMessage.PopFixedString());

                        if (BlackWordsManager.Check(message, BlackWordType.Hotel, Session, "<WiredMensaje>"))
                            message = "Mensaje bloqueado por el filtro bobba.";

                        IWiredTrigger action = new ShowMessage(message, room.GetWiredHandler(), item);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }
                case InteractionType.actionhandiitemcustom:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var message = OtanixEnvironment.FilterInjectionChars(clientMessage.PopFixedString());
                        int valorInteiro;
                        bool inteiroCustom = int.TryParse(message, out valorInteiro);

                        if (inteiroCustom)
                        {
                            IWiredTrigger action = new HandiCustom(message, room.GetWiredHandler(), item);
                            HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        }
                        else
                        {
                            RoomUser usuario = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                            usuario.WhisperComposer("Você não pode colocar letras neste wired, apenas números.");
                        }
                        break;
                    }
                case InteractionType.actioneffectcustom:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var message = OtanixEnvironment.FilterInjectionChars(clientMessage.PopFixedString());
                        int valorInteiro;
                        bool inteiroCustom = int.TryParse(message, out valorInteiro);

                        if (inteiroCustom)
                        {
                            IWiredTrigger action = new EffectCustom(message, room.GetWiredHandler(), item);
                            HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        }
                        else
                        {
                            RoomUser usuario = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                            usuario.WhisperComposer("Você não pode colocar letras neste wired, apenas números.");
                        }
                        break;
                    }
                case InteractionType.actiondiamantescustom:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var message = OtanixEnvironment.FilterInjectionChars(clientMessage.PopFixedString());
                        int valorInteiro;
                        bool inteiroCustom = int.TryParse(message, out valorInteiro);

                        if (inteiroCustom)
                        {
                            IWiredTrigger action = new DiamantesCustom(message, room.GetWiredHandler(), item);
                            HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                            break;
                        }
                        else
                        {
                            RoomUser usuario = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                            usuario.WhisperComposer("Você não pode colocar letras neste wired, apenas números e hífen (-).");
                        }
                        break;
                    }
                case InteractionType.actiondancecustom:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var message = OtanixEnvironment.FilterInjectionChars(clientMessage.PopFixedString());
                        int valorInteiro;
                        bool inteiroCustom = int.TryParse(message, out valorInteiro);

                        if (inteiroCustom)
                        {
                            IWiredTrigger action = new DanceCustom(message, room.GetWiredHandler(), item);
                            HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        }else
                        {
                            RoomUser usuario = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                            usuario.WhisperComposer("Você não pode colocar letras neste wired, apenas números.");
                        }
                        break;
                    }
                case InteractionType.actionfastwalk:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var cycleTimes = clientMessage.PopWiredInt32();

                        IWiredTrigger handler = new FastWalkCustom(room.GetWiredHandler(), item, cycleTimes);
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);

                        break;
                    }
                case InteractionType.actionfreezecustom:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var cycleTimes = clientMessage.PopWiredInt32();

                        IWiredTrigger handler = new FreezeCustom(room.GetWiredHandler(), item, cycleTimes);
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);

                        break;
                    }
                case InteractionType.actionteleportto:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var junk2 = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();

                        IWiredTrigger action = new TeleportToItem(room.GetGameMap(), room.GetWiredHandler(), items, delay, item);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actiontogglestate:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var message = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();
                        
                        IWiredTrigger action = new ToggleItemState(room.GetWiredHandler(), items, delay, item);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actionmoverotate:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var movement = (MovementState)clientMessage.PopWiredInt32();
                        var rotation = (RotationState)clientMessage.PopWiredInt32();

                        var junk3 = clientMessage.PopWiredBoolean();
                        var junk2 = clientMessage.PopWiredBoolean();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();

                        IWiredTrigger handler = new MoveRotate(movement, rotation, items, delay, room, room.GetWiredHandler(), item);
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actiongivereward:
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_wired_rewards"))
                        {
                            Session.SendNotif("No tienes permitido usar este Wired.");
                            break;
                        }

                        var junk = clientMessage.PopWiredInt32();
                        var often = clientMessage.PopWiredInt32();
                        var unique = clientMessage.PopWiredInt32();
                        var limite = clientMessage.PopWiredInt32();
                        var nInt = clientMessage.PopWiredInt32();
                        var extrainfo = clientMessage.PopFixedString();

                        #region Posible Bug?
                        if (extrainfo.Contains(";"))
                        {
                            foreach (var s in extrainfo.Split(';'))
                            {
                                if (s.StartsWith("1"))
                                {
                                    string value = s.Split(',')[1];
                                    if (!value.StartsWith("diamonds:") && !value.StartsWith("alert:"))
                                    {
                                        try { int.Parse(value); }
                                        catch { Session.SendNotif("Has intentado poner un item inválido. Recuerda que debes poner el item_id."); return; }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (extrainfo.StartsWith("1"))
                            {
                                string value = extrainfo.Split(',')[1];
                                if (!value.StartsWith("diamonds:") && !value.StartsWith("alert:"))
                                {
                                    try { int.Parse(value); }
                                    catch { Session.SendNotif("Has intentado poner un item inválido. Recuerda que debes poner el item_id."); return; }
                                }
                            }
                        }
                        #endregion

                        OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, "", "WiredReward", "Wired Id: " + item.Id + ", RoomId: " + item.RoomId + ".");

                        IWiredTrigger action = new GiveReward(extrainfo, limite, often, unique, nInt, item);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actionchase:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var junk2 = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();

                        IWiredTrigger action = new Chase(items, delay, room, room.GetWiredHandler(), item);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actionkickuser:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var message = clientMessage.PopFixedString();

                        IWiredTrigger action = new KickUser(message, room.GetWiredHandler(), item);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actionescape:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var junk2 = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();

                        IWiredTrigger action = new Escape(items, delay, room, room.GetWiredHandler(), item);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actionjointoteam:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var teamid = (Team)clientMessage.PopWiredInt32();

                        IWiredTrigger action = new JoinToTeam(room.GetWiredHandler(), item, teamid);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actionleaveteam:
                    {
                        IWiredTrigger action = new LeaveTeam(room.GetWiredHandler(), item);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actiongiveteamscore:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var points = clientMessage.PopWiredInt32();
                        var games = clientMessage.PopWiredInt32();
                        var teamid = (Team)clientMessage.PopWiredInt32();

                        IWiredTrigger action = new GiveTeamScore(games, points, teamid, room.GetGameManager(), item);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);

                        break;
                    }

                case InteractionType.actioncallstacks:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var junk2 = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();

                        IWiredTrigger action = new CallStacks(items, room, room.GetWiredHandler(), item);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actionmovetodir:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var movement = (MovementDirection)clientMessage.PopWiredInt32();
                        var rotation = (WhenMovementBlock)clientMessage.PopWiredInt32();

                        var junk3 = clientMessage.PopWiredBoolean();
                        var junk2 = clientMessage.PopWiredBoolean();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();

                        IWiredTrigger handler = new MoveToDir(items, movement, rotation, room, room.GetWiredHandler(), item);
                        HandleTriggerSave(handler, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actionbotmove:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var botName = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredUInt();

                        IWiredTrigger action = new BotMove(item.Id, room,  room.GetWiredHandler(), botName, items, delay);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actionbotwhisper:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        bool talkorwhisper = clientMessage.PopWiredInt32() == 1;
                        string message = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);

                        IWiredTrigger action = new BotTalkToUser(item.Id, room, room.GetWiredHandler(), message, talkorwhisper);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actionbotteleport:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var botName = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredUInt();

                        IWiredTrigger action = new BotTeleport(item.Id, room, room.GetWiredHandler(), botName, items, delay);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actionbotclothes:
                    {
                        int junk = clientMessage.PopWiredInt32();
                        string message = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredUInt();

                        IWiredTrigger action = new BotChangeLook(item.Id, room, room.GetWiredHandler(), message, delay);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actionbottalk:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        bool talkorshout = clientMessage.PopWiredInt32() == 1;
                        string message = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredUInt();

                        IWiredTrigger action = new BotTalkToAll(item.Id, room, room.GetWiredHandler(), message, talkorshout, delay);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actionbothanditem:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        int handitem = clientMessage.PopWiredInt32();
                        string botname = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredUInt();

                        IWiredTrigger action = new BotGiveHandItem(item.Id, room, room.GetWiredHandler(), botname, handitem, delay);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actionbotfollowavt:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        bool followorstop = clientMessage.PopWiredInt32() == 1;
                        string botname = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredUInt();

                        IWiredTrigger action = new BotFollowUser(item.Id, room, room.GetWiredHandler(), botname, followorstop, delay);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actionmutetriggerer:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        uint mutetimer = clientMessage.PopWiredUInt();
                        string botname = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredUInt();

                        IWiredTrigger action = new MuteTriggerer(room.GetWiredHandler(), botname, mutetimer, delay, item);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }

                case InteractionType.actionmovetofurni:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        int direction = clientMessage.PopWiredInt32();
                        int length = clientMessage.PopWiredInt32();
                        clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();

                        IWiredTrigger action = new MoveToFurni(room.GetWiredHandler(), items, length, direction, delay, item);
                        HandleTriggerSave(action, room.GetWiredHandler(), room, item);
                        break;
                    }
                #endregion
            }
            Session.SendMessage(new ServerMessage(Outgoing.SaveWired));
        }

        internal static void HandleConditionSave(GameClient Session, uint itemID, Room room, ClientMessage clientMessage)
        {
            if (room == null || room.GetRoomItemHandler() == null)
                return;

            var item = room.GetRoomItemHandler().GetItem(itemID);
            if (item == null)
                return;

            if (item.wiredCondition != null)
            {
                room.GetWiredHandler().conditionHandler.RemoveConditionToTile(item.Coordinate, item.wiredCondition);

                item.wiredCondition.Dispose();
                item.wiredCondition = null;
            }

            var type = item.GetBaseItem().InteractionType;

            if (!WiredUtillity.TypeIsWiredCondition(type))
                return;

            IWiredCondition handler = null;

            switch (type)
            {
                case InteractionType.conditionfurnishaveusers:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var message = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();

                        handler = new FurniHasUsers(item, items);
                        break;
                    }

                case InteractionType.conditionhasfurnion:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var onlyOneItem = clientMessage.PopWiredInt32();
                        var message = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();

                        handler = new FurniHasFurni(item, items, onlyOneItem);
                        break;
                    }

                case InteractionType.conditiontriggeronfurni:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var message = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();

                        handler = new TriggerUserIsOnFurni(item, items);
                        break;
                    }

                case InteractionType.conditionstatepos:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var item1 = clientMessage.PopWiredInt32();
                        var item2 = clientMessage.PopWiredInt32();
                        var item3 = clientMessage.PopWiredInt32();
                        var message = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();
                        var itemsState = item1 + "," + item2 + "," + item3;
                        var originalItemLocation = new Dictionary<uint, OriginalItemLocation>();
                        foreach (RoomItem nItem in items)
                        {
                            originalItemLocation.Add(nItem.Id, new OriginalItemLocation(nItem.Id, nItem.GetX, nItem.GetY, nItem.TotalHeight, nItem.Rot, nItem.ExtraData));
                        }

                        handler = new FurniStatePosMatch(item, items, itemsState, originalItemLocation);
                        break;
                    }

                case InteractionType.conditiontimelessthan:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var time = clientMessage.PopWiredInt32();
                        handler = new LessThanTimer(time, room, item);
                        break;
                    }

                case InteractionType.conditiontimemorethan:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var time = clientMessage.PopWiredInt32();
                        handler = new MoreThanTimer(time, room, item);
                        break;
                    }

                case InteractionType.conditionactoringroup:
                    {
                        handler = new ActorInGroup(room.RoomData.GroupId, item);
                        break;
                    }

                case InteractionType.conditionactorinteam:
                    {    
                        var junk = clientMessage.PopWiredInt32();
                        var team = (Team)clientMessage.PopWiredInt32();
                        handler = new ActorInTeam(team, item);
                        break;
                    }

                case InteractionType.conditionusercountin:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var minUsers = clientMessage.PopWiredUInt();
                        var maxUsers = clientMessage.PopWiredUInt();
                        handler = new UserCountIn(minUsers, maxUsers, item);
                        break;
                    }

                case InteractionType.conditionstuffis:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var message = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();

                        handler = new StuffIs(item, items);
                        break;
                    }

                case InteractionType.conditionhandleitemid:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var handleItem = clientMessage.PopWiredInt32();
                        handler = new HandleItemUser(handleItem, item);
                        break;
                    }

                case InteractionType.conditionnotfurnishaveusers:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var message = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();

                        handler = new NotFurniHasUsers(item, items);
                        break;
                    }

                case InteractionType.conditionnotfurnion:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var onlyOneItem = clientMessage.PopWiredInt32();
                        var message = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();

                        handler = new NotFurniHasFurni(item, items, onlyOneItem);
                        break;
                    }

                case InteractionType.conditionnottriggeronfurni:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var message = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();

                        handler = new NotTriggerUserIsOnFurni(item, items);
                        break;
                    }

                case InteractionType.conditionnotstatepos:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var item1 = clientMessage.PopWiredInt32();
                        var item2 = clientMessage.PopWiredInt32();
                        var item3 = clientMessage.PopWiredInt32();
                        var message = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();
                        var itemsState = item1 + "," + item2 + "," + item3;
                        var originalItemLocation = new Dictionary<uint, OriginalItemLocation>();
                        foreach (RoomItem nItem in items)
                        {
                            originalItemLocation.Add(nItem.Id, new OriginalItemLocation(nItem.Id, nItem.GetX, nItem.GetY, nItem.TotalHeight, nItem.Rot, nItem.ExtraData));
                        }

                        handler = new NotFurniStatePosMatch(item, items, itemsState, originalItemLocation);
                        break;
                    }

                case InteractionType.conditionnotingroup:
                    {
                        handler = new NotActorInGroup(room.RoomData.GroupId, item);
                        break;
                    }

                case InteractionType.conditionnotinteam:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var team = (Team)clientMessage.PopWiredInt32();
                        handler = new NotActorInTeam(team, item);
                        break;
                    }

                case InteractionType.conditionnotusercount:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var minUsers = clientMessage.PopWiredUInt();
                        var maxUsers = clientMessage.PopWiredUInt();
                        handler = new NotUserCountIn(minUsers, maxUsers, item);
                        break;
                    }

                case InteractionType.conditionnotstuffis:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var message = clientMessage.PopFixedString();
                        int furniCount;
                        var items = GetItems(clientMessage, room, out furniCount);
                        var delay = clientMessage.PopWiredInt32();

                        handler = new NotStuffIs(item, items);
                        break;
                    }

                case InteractionType.conditionwearingeffect:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var effect = clientMessage.PopWiredUInt();
                        handler = new UserWearingEffect(effect, item);
                        break;
                    }

                case InteractionType.conditionnotwearingeffect:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var effect = clientMessage.PopWiredUInt();
                        handler = new UserNotWearingEffect(effect, item);
                        break;
                    }

                case InteractionType.conditionwearingbadge:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var badgeID = clientMessage.PopFixedString();
                        handler = new UserWearingBadge(badgeID, item);
                        break;
                    }

                case InteractionType.conditionnotwearingbadge:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var badgeID = clientMessage.PopFixedString();
                        handler = new UserNotWearingBadge(badgeID, item);
                        break;
                    }

                case InteractionType.conditiondaterange:
                    {
                        var junk = clientMessage.PopWiredInt32();
                        var startdate = clientMessage.PopWiredInt32();
                        var enddate = clientMessage.PopWiredInt32();
                        handler = new DateRangeActive(startdate, enddate, item);
                        break;
                    }
                default:
                    return;
            }

            item.wiredCondition = handler;
            room.GetWiredHandler().conditionHandler.AddConditionToTile(item.Coordinate, item.wiredCondition);
            room.GetRoomItemHandler().UpdateWiredItem(item);
            Session.SendMessage(new ServerMessage(Outgoing.SaveWired));
        }

        private static List<RoomItem> GetItems(ClientMessage message, Room room, out int itemCount)
        {
            var items = new List<RoomItem>();
            itemCount = message.PopWiredInt32();

            uint itemID;
            RoomItem item;
            for (var i = 0; i < itemCount; i++)
            {
                itemID = message.PopWiredUInt();
                item = room.GetRoomItemHandler().GetItem(itemID);

                if (item != null) // && (!WiredUtillity.TypeIsWired(item.GetBaseItem().InteractionType))
                    items.Add(item);
            }

            return items;
        }

        private static void HandleTriggerSave(IWiredTrigger handler, WiredHandler manager, Room room, RoomItem item)
        {
            item.wiredHandler = handler;
            manager.RemoveFurniture(item);
            manager.AddFurniture(item);

            room.GetRoomItemHandler().UpdateWiredItem(item);
        }
    }
}
