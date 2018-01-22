using System;
using System.Collections.Generic;
using System.Data;
using Butterfly.HabboHotel.Quests;
using ButterStorm;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Pathfinding;
using Butterfly.HabboHotel.Rooms;
using Butterfly.HabboHotel.Rooms.Games;
using System.Drawing;
using Butterfly.Messages;
using HabboEvents;
using Butterfly.HabboHotel.Rooms.Wired;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Triggers;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Effects;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Conditions;
using Butterfly.HabboHotel.SoundMachine;
using Butterfly.HabboHotel.Users;
using Butterfly.HabboHotel.Pets.Plantas;
using Butterfly.HabboHotel.Pets;
using Butterfly.HabboHotel.Catalogs;
using Butterfly.HabboHotel.RoomBots;
using Butterfly.Core;

namespace Butterfly.HabboHotel.Items.Interactors
{
    abstract class FurniInteractor
    {
        internal abstract void OnPlace(GameClient Session, RoomItem Item);
        internal abstract void OnRemove(GameClient Session, RoomItem Item);
        internal abstract void OnTrigger(GameClient Session, RoomItem Item, int Request, Boolean UserHasRights);
    }

    class InteractorTeleport : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "0";

            if (Item.InteractingUser != 0)
            {
                var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                if (User != null)
                {
                    User.ClearMovement(true);
                    User.AllowOverride = false;
                    User.CanWalk = true;
                }

                Item.InteractingUser = 0;
            }

            if (Item.InteractingUser2 != 0)
            {
                var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser2);

                if (User != null)
                {
                    User.ClearMovement(true);
                    User.AllowOverride = false;
                    User.CanWalk = true;
                }

                Item.InteractingUser2 = 0;
            }
        }

        internal override void OnRemove(GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "0";

            if (Item.InteractingUser != 0)
            {
                var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                if (User != null)
                {
                    User.UnlockWalking();
                }

                Item.InteractingUser = 0;
            }

            if (Item.InteractingUser2 != 0)
            {
                var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser2);

                if (User != null)
                {
                    User.UnlockWalking();
                }

                Item.InteractingUser2 = 0;
            }

            //Item.GetRoom().RegenerateUserMatrix();
        }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            // Is this user valid?
            if (Item == null || Item.GetRoom() == null || Session == null || Session.GetHabbo() == null)
                return;
            var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
            {
                return;
            }

            // Alright. But is this user in the right position?
            if (User.Coordinate == Item.Coordinate || User.Coordinate == Item.SquareInFront)
            {
                // Fine. But is this tele even free?
                if (Item.InteractingUser != 0)
                {
                    return;
                }

                //User.TeleDelay = -1;
                Item.InteractingUser = User.GetClient().GetHabbo().Id;
            }
            else if (User.CanWalk)
            {
                User.MoveTo(Item.SquareInFront);
            }
        }
    }
    class InteractorBanzaiPuck : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (Session == null)
                return;
            RoomUser interactingUser = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            Point userCoord = interactingUser.Coordinate;
            Point ballCoord = Item.Coordinate;

            int differenceX = userCoord.X - ballCoord.X;
            int differenceY = userCoord.Y - ballCoord.Y;

            if (differenceX <= 1 && differenceX >= -1 && differenceY <= 1 && differenceY >= -1)
            {
                differenceX = differenceX * 2;
                differenceY = differenceY * 2;

                int newX = Item.GetX + differenceX;
                int newY = Item.GetY + differenceY;

                Item.GetRoom().GetGameManager().GetBanzai().MovePuck(Item, Session, newX, newY, interactingUser.team);
                interactingUser.MoveTo(ballCoord);
            }
            else //if (differenceX == 2 || differenceY == 2 || differenceY == - 2 || differenceX == -2)
            {
                Item.interactingBallUser = Session.GetHabbo().Id;

                differenceX = differenceX * (-1);
                differenceY = differenceY * (-1);

                if (differenceX > 1)
                    differenceX = 1;
                else if (differenceX < -1)
                    differenceX = -1;


                if (differenceY > 1)
                    differenceY = 1;
                else if (differenceY < -1)
                    differenceY = -1;


                int newX = Item.GetX + differenceX;
                int newY = Item.GetY + differenceY;

                interactingUser.MoveTo(new Point(newX, newY));
            }
        }
    }
    class InteractorSpinningBottle : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "0";
            Item.UpdateState(false, false);
        }

        internal override void OnRemove(GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "0";
        }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (Item.ExtraData != "-1")
            {
                Item.ExtraData = "-1";
                Item.UpdateState(false, true);
                Item.ReqUpdate(3, true);
            }
        }
    }

    class InteractorDice : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            RoomUser User = null;
            if (Session != null)
                User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
            {
                if (Item.ExtraData != "-1")
                {
                    Item.InteractingUser = Session.GetHabbo().Id;

                    if (Request == -1)
                    {
                        Item.ExtraData = "0";
                        Item.UpdateState();
                    }
                    else
                    {
                        Item.ExtraData = "-1";
                        Item.UpdateState(false, true);
                        Item.ReqUpdate(4, true);
                    }
                }
            }
            else
            {
                User.MoveTo(Item.SquareInFront);
            }
        }
    }

    class InteractorHabboWheel : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "-1";
            Item.ReqUpdate(10, true);
        }

        internal override void OnRemove(GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "-1";
        }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (!UserHasRights)
            {
                return;
            }

            if (Item.ExtraData != "-1")
            {
                Item.ExtraData = "-1";
                Item.UpdateState();
                Item.ReqUpdate(10, true);
            }
        }
    }

    class InteractorLoveShuffler : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "-1";
        }

        internal override void OnRemove(GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "-1";
        }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (!UserHasRights)
            {
                return;
            }

            if (Item.ExtraData != "0")
            {
                Item.ExtraData = "0";
                Item.UpdateState(false, true);
                Item.ReqUpdate(10, true);
            }
        }
    }

    class InteractorOneWayGate : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "0";

            if (Item.InteractingUser != 0)
            {
                var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                if (User != null)
                {
                    User.ClearMovement(true);
                    User.UnlockWalking();
                }

                Item.InteractingUser = 0;
            }
        }

        internal override void OnRemove(GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "0";

            if (Item.InteractingUser != 0)
            {
                var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                if (User != null)
                {
                    User.ClearMovement(true);
                    User.UnlockWalking();
                }

                Item.InteractingUser = 0;
            }
        }
        
        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (Session == null)
                return;
            var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
            {
                return;
            }

            if (User.Coordinate != Item.SquareInFront && User.CanWalk)
            {
                User.MoveTo(Item.SquareInFront);
                return;
            }

            if (!Item.GetRoom().GetGameMap().tileIsWalkable(Item.SquareBehind.X, Item.SquareBehind.Y, true))
            {
                return;
            }

            if (Item.InteractingUser == 0)
            {
                Item.InteractingUser = User.HabboId;

                User.CanWalk = false;

                if (User.IsWalking && (User.GoalX != Item.SquareInFront.X || User.GoalY != Item.SquareInFront.Y))
                {
                    User.ClearMovement(true);
                }

                User.AllowOverride = true;
                User.MoveTo(Item.GetX, Item.GetY);

                Item.ReqUpdate(4, true);
            }
        }
    }

    class InteractorAlert : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "0";
        }

        internal override void OnRemove(GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "0";
        }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (!UserHasRights)
            {
                return;
            }

            if (Item.ExtraData == "0")
            {
                Item.ExtraData = "1";
                Item.UpdateState(false, true);
                Item.ReqUpdate(4, true);
            }
        }
    }

    class InteractorVendor : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "0";

            if (Item.InteractingUser > 0)
            {
                var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                if (User != null)
                {
                    User.CanWalk = true;
                }
            }
        }
       
        internal override void OnRemove(GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "0";

            if (Item.InteractingUser > 0)
            {
                var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                if (User != null)
                {
                    User.CanWalk = true;
                }
            }
        }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (Item.ExtraData != "1" && Item.GetBaseItem().VendingIds.Count >= 1 && Item.InteractingUser == 0 && Session != null)
            {
                var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

                if (User == null)
                {
                    return;
                }

                if (!Gamemap.TilesTouching(User.X, User.Y, Item.GetX, Item.GetY))
                {
                    User.MoveTo(Item.SquareInFront);
                    return;
                }

                if (User.IsWalking)
                    return;

                Item.InteractingUser = Session.GetHabbo().Id;

                User.CanWalk = false;
                User.ClearMovement(true);
                User.SetRot(Rotation.Calculate(User.X, User.Y, Item.GetX, Item.GetY), false);

                Item.ReqUpdate(2, true);

                Item.ExtraData = "1";
                Item.UpdateState(false, true);
            }
        }
    }

    class InteractorMuteSignal : FurniInteractor
    {
        private const int Modes = 1;

        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (!UserHasRights)
            {
                return;
            }
            var currentMode = 0;
            var newMode = 0;

            try
            {
                currentMode = int.Parse(Item.ExtraData);
            }
            catch
            {

            }

            if (currentMode <= 0)
            {
                newMode = 1;
            }
            else if (currentMode >= Modes)
            {
                newMode = 0;
            }
            else
            {
                newMode = currentMode + 1;
            }

            //1 = muted, 0 = no mute
            Room salaUsuario = Item.GetRoom();
            switch (newMode)
            {
                case 0:
                    salaUsuario.muteSignalEnabled = false;
                    break;

                case 1:
                    salaUsuario.muteSignalEnabled = true;
                    break;
            }

            Item.ExtraData = newMode.ToString();
            Item.UpdateState();
        }
    }

    class InteractorGenericSwitch : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (Item == null || Item.GetBaseItem() == null)
                return;

            if (Request == 3 && (Item.GetBaseItem().InteractionType == InteractionType.floorswitch1 || Item.GetBaseItem().InteractionType == InteractionType.floorswitch2))
                return;

            if (Request != 3)
            {
                if (Session == null || Session.GetHabbo() == null)
                    return;

                if (!UserHasRights)
                    return;

                OtanixEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_SWITCH);
            }

            if (Item.GetBaseItem().MultiHeight.Count > 0 && Item.GetRoom().GetGameMap().CanUpdateMultiHeight(Item) == false)
            {
                return;
            }

            Item.updateInteractionCount(Item); // atualiza conforme o interaction count, separei pra usar em outros lugares

            if (Item.GetBaseItem().MultiHeight.Count > 0)
            {
                Item.GetRoom().GetGameMap().updateMapForItem(Item);

                if (Session == null || Session.GetHabbo() == null)
                    return;

                List<RoomUser> userList = Item.GetRoom().GetGameMap().GetUsersOnItem(Item);
                foreach(RoomUser User in userList)
                {
                    Item.GetRoom().GetRoomUserManager().UpdateUserStatus(User, false);
                }
            }
        }
    }

    class InteractorNone : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
        }
    }

    class InteractorGate : FurniInteractor
    {
        readonly int Modes;

        internal InteractorGate(int Modes)
        {
            this.Modes = (Modes - 1);

            if (this.Modes < 0)
            {
                this.Modes = 0;
            }
        }

        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (!UserHasRights)
            {
                return;
            }

            if (Item.GetBaseItem().InteractionType != InteractionType.gate)
                return;

            if (Modes == 0)
            {
                Item.UpdateState(false, true);
            }

            var currentMode = 0;
            var newMode = 0;

            int.TryParse(Item.ExtraData, out currentMode);

            if (currentMode <= 0)
            {
                newMode = 1;
            }
            else if (currentMode >= Modes)
            {
                newMode = 0;
            }
            else
            {
                newMode = currentMode + 1;
            }

            if (newMode == 0)
            {
                if (!Item.GetRoom().GetGameMap().tileIsWalkable(Item.GetX, Item.GetY, false))
                    return;
            }

            Item.ExtraData = newMode.ToString();
            Item.UpdateState();
            Item.GetRoom().GetGameMap().updateMapForItem(Item);
        }
    }

    class InteractorScoreboard : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item)
        {
            if ((Item.GetRoom().GetGameManager().IsGameStarted() || Item.GetRoom().GetGameManager().IsGamePaused()) && Item.GetRoom().GetGameManager().IsSameChronometer(Item))
            {
                Item.GetRoom().GetGameManager().EndGame();
            }
        }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (!UserHasRights)
            {
                return;
            }

            var oldValue = 0;
            int.TryParse(Item.ExtraData, out oldValue);

            if (Request == 1)
            {
                if ((!Item.GetRoom().GetGameManager().IsGameStarted() || !Item.GetRoom().GetGameManager().IsSameChronometer(Item)) && oldValue > 0)
                {
                    Item.UpdateNeeded = !Item.UpdateNeeded;

                    if (Item.UpdateNeeded)
                        Item.GetRoom().GetGameManager().SetChronometer(Item);

                    Item.pendingReset = true;
                }
                else
                {
                    if (Item.GetRoom().GetGameManager().IsGameStarted())
                    {
                        Item.GetRoom().GetGameManager().UpdatePauseGame();
                    }
                }
            }
            else if (Request == 2)
            {
                if (Item.pendingReset && oldValue > 0)
                {
                    oldValue = 0;
                    Item.pendingReset = false;
                }
                else
                {
                    if (oldValue == 0 || oldValue == 30 || oldValue == 60 || oldValue == 120 || oldValue == 180 || oldValue == 300 || oldValue == 600)
                    {
                        if (oldValue == 0)
                            oldValue = 30;
                        else if (oldValue == 30)
                            oldValue = 60;
                        else if (oldValue == 60)
                            oldValue = 120;
                        else if (oldValue == 120)
                            oldValue = 180;
                        else if (oldValue == 180)
                            oldValue = 300;
                        else if (oldValue == 300)
                            oldValue = 600;
                        else if (oldValue == 600)
                            oldValue = 0;
                    }
                    else
                        oldValue = 0;
                    Item.originalExtraData = oldValue.ToString();
                    Item.UpdateNeeded = false;
                }
            }
            else if (Request == 3)
            {
                if (!Item.GetRoom().GetGameManager().IsGameStarted() || !Item.GetRoom().GetGameManager().IsSameChronometer(Item))
                {
                    Item.UpdateNeeded = !Item.UpdateNeeded;

                    if (Item.UpdateNeeded)
                    {
                        Item.GetRoom().GetGameManager().SetChronometer(Item);
                    }

                    int origianldata = 30;
                    int.TryParse(Item.originalExtraData, out origianldata);
                    oldValue = origianldata;
                    Item.pendingReset = true;
                }
            }

            Item.ExtraData = oldValue.ToString();
            Item.UpdateState();
        }
    }

    class InteractorFootball : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            /*if (Session == null)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
            {
                User.MoveTo(Item.GetX, Item.GetY);
                return;
            }

            Point userPoint = new Point(User.X, User.Y);
            Item.ExtraData = "55";
            Item.ballIsMoving = true;
            Item._iBallValue = 1;
            Item.ballMover = User;
            Item.GetRoom().GetSoccer().MoveBall(Item, User.GetClient(), userPoint);*/
        }
    }

    class InteractorScoreCounter : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item)
        {
            if (Item.team == Team.none)
                return;

            Item.ExtraData = Item.GetRoom().GetGameManager().GetTeamPoints((int)Item.team).ToString();
            Item.UpdateState(false, true);
        }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (!UserHasRights)
            {
                return;
            }

            var oldValue = 0;

            if (!string.IsNullOrEmpty(Item.ExtraData))
            {
                try
                {
                    oldValue = int.Parse(Item.ExtraData);
                }
                catch { }
            }


            if (Request == 1)
            {
                oldValue++;
            }
            else if (Request == 2)
            {
                oldValue--;
            }
            else if (Request == 3)
            {
                oldValue = 0;
            }

            Item.ExtraData = oldValue.ToString();
            Item.UpdateState(false, true);
        }
    }

    class InteractorBanzaiScoreCounter : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item)
        {
            if (Item.team == Team.none)
                return;

            Item.ExtraData = Item.GetRoom().GetGameManager().GetTeamPoints((int)Item.team).ToString();
            Item.UpdateState(false, true);
        }

        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights) { }
    }

    class InteractorFreezeTile : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (Session == null || Session.GetHabbo() == null || Item.InteractingUser > 0)
                return;

            var username = Session.GetHabbo().Username;
            var user = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(username);
            if (user == null)
                return;

            user.GoalX = Item.GetX;
            user.GoalY = Item.GetY;

            if (user.team != Team.none && user.CurrentEffect >= 40 && user.CurrentEffect <= 43) // tiene equipo y lleva efecto freeze
                user.throwBallAtGoal = true;
        }
    }

    class InteractorIncrementer : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {

            var oldValue = 0;

            if (!string.IsNullOrEmpty(Item.ExtraData))
            {
                try
                {
                    oldValue = int.Parse(Item.ExtraData);
                }
                catch { }
            }
            oldValue += 1;
            //Console.WriteLine(oldValue.ToString());

            Item.ExtraData = oldValue.ToString();
            Item.UpdateState();
        }
    }

    class InteractorIgnore : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
        }
    }

    class WiredInteractor : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (Session == null || Item == null)
                return;

            if (!UserHasRights)
                return;

            var WiredException = Item.GetRoom().GetWiredHandler().InvalidWired(Item);

            switch (Item.GetBaseItem().InteractionType)
            {
                #region Causantes
                case InteractionType.triggerroomenter:
                    {
                        var message = new ServerMessage(Outgoing.WiredTrigger);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);

                        if (Item.wiredHandler != null)
                        {
                            EntersRoom handler = (EntersRoom)Item.wiredHandler;
                            message.AppendString(handler.Username);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }

                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(7);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);

                        break;
                    }

                case InteractionType.triggerwalkonfurni:
                    {
                        var message = new ServerMessage(Outgoing.WiredTrigger);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);

                        if (Item.wiredHandler != null)
                        {
                            WalksOnFurni handler = (WalksOnFurni)Item.wiredHandler;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }

                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(1);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }


                case InteractionType.triggerwalkofffurni:
                    {
                        var message = new ServerMessage(Outgoing.WiredTrigger);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);

                        if (Item.wiredHandler != null)
                        {
                            WalksOffFurni handler = (WalksOffFurni)Item.wiredHandler;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }

                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(2);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.triggergamestart:
                    {
                        var message = new ServerMessage(Outgoing.WiredTrigger);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(8);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.triggergameend:
                    {
                        var message = new ServerMessage(Outgoing.WiredTrigger);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(9);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.triggertimer:
                    {
                        var message = new ServerMessage(Outgoing.WiredTrigger);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(1);
                        if (Item.wiredHandler != null)
                        {
                            Timer handler = (Timer)Item.wiredHandler;
                            message.AppendInt32(handler.IntTimer);
                        }
                        else
                        {
                            message.AppendInt32(50);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(3);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.triggerrepeater:
                    {
                        var message = new ServerMessage(Outgoing.WiredTrigger);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(1);
                        if (Item.wiredHandler != null)
                        {
                            Repeater handler = (Repeater)Item.wiredHandler;
                            message.AppendInt32(handler.IntTimer);
                        }
                        else
                        {
                            message.AppendInt32(10);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(6);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.triggeronusersay:
                    {
                        var message = new ServerMessage(Outgoing.WiredTrigger);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);

                        if (Item.wiredHandler != null)
                        {
                            UserSays handler = (UserSays)Item.wiredHandler;
                            message.AppendString(handler.Message);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }

                        message.AppendInt32(1);
                        if (Item.wiredHandler != null)
                        {
                            UserSays handler = (UserSays)Item.wiredHandler;
                            message.AppendInt32((handler.IsOwnerOnly) ? 1: 0);
                        }
                        else
                        {
                             message.AppendInt32(0);
                        }

                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.triggerscoreachieved:
                    {
                        var message = new ServerMessage(Outgoing.WiredTrigger);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(1);
                        if (Item.wiredHandler != null)
                        {
                            ScoreAchieved handler = (ScoreAchieved)Item.wiredHandler;
                            message.AppendInt32(handler.Score);
                        }
                        else
                        {
                            message.AppendInt32(100);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(10);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.triggerstatechanged:
                    {
                        var message = new ServerMessage(Outgoing.WiredTrigger);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredHandler != null)
                        {
                            StateChanged handler = (StateChanged)Item.wiredHandler;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(4);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.triggercollision:
                    {
                        var message = new ServerMessage(Outgoing.WiredTrigger);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(11);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.triggerlongperiodic:
                    {
                        var message = new ServerMessage(Outgoing.WiredTrigger);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(1);
                        if (Item.wiredHandler != null)
                        {
                            LongRepeater handler = (LongRepeater)Item.wiredHandler;
                            message.AppendInt32(handler.Time);
                        }
                        else
                        {
                            message.AppendInt32(10);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(12);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.triggerbotreachedavtr:
                    {
                        var message = new ServerMessage(Outgoing.WiredTrigger);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredHandler != null)
                        {
                            BotAlcanzaUsuario handler = (BotAlcanzaUsuario)Item.wiredHandler;
                            message.AppendString(handler.Botname);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(14);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.triggerbotreachedstf:
                    {
                        var message = new ServerMessage(Outgoing.WiredTrigger);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredHandler != null)
                        {
                            BotAlcanzaFurni handler = (BotAlcanzaFurni)Item.wiredHandler;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredHandler != null)
                        {
                            BotAlcanzaFurni handler = (BotAlcanzaFurni)Item.wiredHandler;
                            message.AppendString(handler.Botname);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(13);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                #endregion

                #region Efectos
                case InteractionType.actiongivescore:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(2);

                        if (Item.wiredHandler != null)
                        {
                            GiveScore handler = (GiveScore)Item.wiredHandler;
                            message.AppendInt32(handler.ScoreToGive);
                            message.AppendInt32(handler.MaxCountPerGame);
                        }
                        else
                        {
                            message.AppendInt32(50);
                            message.AppendInt32(5);
                        }

                        message.AppendInt32(0);
                        message.AppendInt32(6);
                        message.AppendInt32(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionposreset:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredHandler != null)
                        {
                            PositionReset handler = (PositionReset)Item.wiredHandler;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString(""); // ??
                        message.AppendInt32(3);
                        if (Item.wiredHandler != null)
                        {
                            PositionReset handler = (PositionReset)Item.wiredHandler;
                            message.AppendInt32(handler.furniState);
                            message.AppendInt32(handler.furniDirection);
                            message.AppendInt32(handler.furniPosition);
                        }
                        else
                        {
                            message.AppendInt32(0);
                            message.AppendInt32(0);
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(3); // wired id
                        if (Item.wiredHandler != null)
                        {
                            PositionReset handler = (PositionReset)Item.wiredHandler;
                            message.AppendInt32(handler.Time);
                        }
                        else
                        {
                            message.AppendInt32(5);
                        }
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionresettimer:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(1);
                        if (Item.wiredHandler != null)
                        {
                            TimerReset handler = (TimerReset)Item.wiredHandler;
                            message.AppendInt32(handler.Time);
                        }
                        else
                        {
                            message.AppendInt32(5);
                        }
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionshowmessage:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredHandler != null)
                        {
                            ShowMessage handler = (ShowMessage)Item.wiredHandler;
                            message.AppendString(handler.Message);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(7);
                        message.AppendInt32(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionhandiitemcustom:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredHandler != null)
                        {
                            HandiCustom handler = (HandiCustom)Item.wiredHandler;
                            message.AppendString(handler.Message);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(7);
                        message.AppendInt32(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }
                case InteractionType.actioneffectcustom:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredHandler != null)
                        {
                            EffectCustom handler = (EffectCustom)Item.wiredHandler;
                            message.AppendString(handler.Message);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(7);
                        message.AppendInt32(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }
                case InteractionType.actiondiamantescustom:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredHandler != null)
                        {
                            DiamantesCustom handler = (DiamantesCustom)Item.wiredHandler;
                            message.AppendString(handler.Message);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(7);
                        message.AppendInt32(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }
                case InteractionType.actiondancecustom:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredHandler != null)
                        {
                            DanceCustom handler = (DanceCustom)Item.wiredHandler;
                            message.AppendString(handler.Message);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(7);
                        message.AppendInt32(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }
                case InteractionType.actionfastwalk:
                    {
                        var message = new ServerMessage(Outgoing.WiredTrigger);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(1);
                        if (Item.wiredHandler != null)
                        {
                            FastWalkCustom handler = (FastWalkCustom)Item.wiredHandler;
                            message.AppendInt32(handler.IntTimer);
                        }
                        else
                        {
                            message.AppendInt32(10);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(6);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }
                case InteractionType.actionfreezecustom:
                    {
                        var message = new ServerMessage(Outgoing.WiredTrigger);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(1);
                        if (Item.wiredHandler != null)
                        {
                            FreezeCustom handler = (FreezeCustom)Item.wiredHandler;
                            message.AppendInt32(handler.IntTimer);
                        }
                        else
                        {
                            message.AppendInt32(10);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(6);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }
                case InteractionType.actionteleportto:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(10);
                        if (Item.wiredHandler != null)
                        {
                            TeleportToItem handler = (TeleportToItem)Item.wiredHandler;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(8);
                        message.AppendInt32(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actiontogglestate:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredHandler != null)
                        {
                            ToggleItemState handler = (ToggleItemState)Item.wiredHandler;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        if (Item.wiredHandler != null)
                        {
                            ToggleItemState handler = (ToggleItemState)Item.wiredHandler;
                            message.AppendInt32(handler.Time);
                        }
                        else
                        {
                            message.AppendInt32(5);
                        }
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionmoverotate:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredHandler != null)
                        {
                            MoveRotate handler = (MoveRotate)Item.wiredHandler;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(2);
                        if (Item.wiredHandler != null)
                        {
                            MoveRotate handler = (MoveRotate)Item.wiredHandler;
                            message.AppendInt32(handler.Movement);
                            message.AppendInt32(handler.Rotation);
                        }
                        else
                        {
                            message.AppendInt32(0);
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(4);
                        if (Item.wiredHandler != null)
                        {
                            MoveRotate handler = (MoveRotate)Item.wiredHandler;
                            message.AppendInt32(handler.Time);
                        }
                        else
                        {
                            message.AppendInt32(5);
                        }
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actiongivereward:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredHandler != null)
                        {
                            GiveReward handler = (GiveReward)Item.wiredHandler;
                            message.AppendString(handler.Message);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }
                        message.AppendInt32(4);
                        if (Item.wiredHandler != null)
                        {
                            GiveReward handler = (GiveReward)Item.wiredHandler;
                            message.AppendInt32(handler._Type);
                            message.AppendInt32(handler._AllUsers);
                            message.AppendInt32(handler._Amount);
                            message.AppendInt32(handler._nInt);
                        }
                        else
                        {
                            message.AppendInt32(0);
                            message.AppendInt32(0);
                            message.AppendInt32(0);
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(17); // wired id
                        message.AppendInt32(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);

                        break;
                    }

                case InteractionType.actionchase:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredHandler != null)
                        {
                            Chase handler = (Chase)Item.wiredHandler;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(11);
                        message.AppendInt32(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionkickuser:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredHandler != null)
                        {
                            KickUser handler = (KickUser)Item.wiredHandler;
                            message.AppendString(handler.Message);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(19);
                        message.AppendInt32(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionescape:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredHandler != null)
                        {
                            Escape handler = (Escape)Item.wiredHandler;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(12);
                        message.AppendInt32(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionjointoteam:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                            message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(1);
                        if (Item.wiredHandler != null)
                        {
                            JoinToTeam handler = (JoinToTeam)Item.wiredHandler;
                            message.AppendInt32((int)handler.Team);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(9);
                        message.AppendInt32(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionleaveteam:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(10);
                        message.AppendInt32(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actiongiveteamscore:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(3);

                        if (Item.wiredHandler != null)
                        {
                            GiveTeamScore handler = (GiveTeamScore)Item.wiredHandler;
                            message.AppendInt32(handler.ScoreToGive);
                        }
                        else
                        {
                            message.AppendInt32(50);
                        }

                        if (Item.wiredHandler != null)
                        {
                            GiveTeamScore handler = (GiveTeamScore)Item.wiredHandler;
                            message.AppendInt32(handler.MaxCountPerGame);
                        }
                        else
                        {
                            message.AppendInt32(5);
                        }

                        if (Item.wiredHandler != null)
                        {
                            GiveTeamScore handler = (GiveTeamScore)Item.wiredHandler;
                            message.AppendInt32((int)handler.Team);
                        }
                        else
                        {
                            message.AppendInt32(1);
                        }


                        message.AppendInt32(0);
                        message.AppendInt32(14);
                        message.AppendInt32(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actioncallstacks:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredHandler != null)
                        {
                            CallStacks handler = (CallStacks)Item.wiredHandler;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(18);
                        message.AppendInt32(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionmovetodir:
                    {
                        var message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredHandler != null)
                        {
                            MoveToDir handler = (MoveToDir)Item.wiredHandler;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(2);
                        if (Item.wiredHandler != null)
                        {
                            MoveToDir handler = (MoveToDir)Item.wiredHandler;
                            message.AppendInt32(handler.StartDirection);
                        }
                        else
                        {
                            message.AppendInt32(50);
                        }

                        if (Item.wiredHandler != null)
                        {
                            MoveToDir handler = (MoveToDir)Item.wiredHandler;
                            message.AppendInt32(handler.WhenMoveIsBlocked);
                        }
                        else
                        {
                            message.AppendInt32(5);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(13);
                        message.AppendInt32(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionbotteleport:
                    {
                        ServerMessage message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredHandler != null)
                        {
                            BotTeleport handler = (BotTeleport)Item.wiredHandler;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredHandler != null)
                        {
                            BotTeleport handler = (BotTeleport)Item.wiredHandler;
                            message.AppendString(handler.Botname);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(21);
                        if (Item.wiredHandler != null)
                        {
                            BotTeleport handler = (BotTeleport)Item.wiredHandler;
                            message.AppendUInt(handler.Time);
                        }
                        else
                        {
                            message.AppendUInt(0);
                        }
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionbotmove:
                    {
                        ServerMessage message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredHandler != null)
                        {
                            BotMove handler = (BotMove)Item.wiredHandler;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredHandler != null)
                        {
                            BotMove handler = (BotMove)Item.wiredHandler;
                            message.AppendString(handler.Botname);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(22);
                        if (Item.wiredHandler != null)
                        {
                            BotMove handler = (BotMove)Item.wiredHandler;
                            message.AppendUInt(handler.Time);
                        }
                        else
                        {
                            message.AppendUInt(0);
                        }
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionbotwhisper:
                    {
                        ServerMessage message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                            message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredHandler != null)
                        {
                            BotTalkToUser handler = (BotTalkToUser)Item.wiredHandler;
                            message.AppendString(handler.Message);
                        }
                        else
                        {
                            message.AppendString(((char)9).ToString());
                        }
                        message.AppendInt32(1);
                        if (Item.wiredHandler != null)
                        {
                            BotTalkToUser handler = (BotTalkToUser)Item.wiredHandler;
                            message.AppendInt32(handler.TalkOrWhisper);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(27);
                        message.AppendUInt(0);
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionbotclothes:
                    {
                        ServerMessage message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredHandler != null)
                        {
                            BotChangeLook handler = (BotChangeLook)Item.wiredHandler;
                            message.AppendString(handler.Message);
                        }
                        else
                        {
                            message.AppendString(((char)9).ToString());
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(26);
                        if (Item.wiredHandler != null)
                        {
                            BotChangeLook handler = (BotChangeLook)Item.wiredHandler;
                            message.AppendUInt(handler.Time);
                        }
                        else
                        {
                            message.AppendUInt(0);
                        }
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionbottalk:
                    {
                        ServerMessage message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredHandler != null)
                        {
                            BotTalkToAll handler = (BotTalkToAll)Item.wiredHandler;
                            message.AppendString(handler.Message);
                        }
                        else
                        {
                            message.AppendString(((char)9).ToString());
                        }
                        message.AppendInt32(1);
                        if (Item.wiredHandler != null)
                        {
                            BotTalkToAll handler = (BotTalkToAll)Item.wiredHandler;
                            message.AppendInt32(handler.TalkOrShout);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(23);
                        if (Item.wiredHandler != null)
                        {
                            BotTalkToAll handler = (BotTalkToAll)Item.wiredHandler;
                            message.AppendUInt(handler.Time);
                        }
                        else
                        {
                            message.AppendUInt(0);
                        }
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionbothanditem:
                    {
                        ServerMessage message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredHandler != null)
                        {
                            BotGiveHandItem handler = (BotGiveHandItem)Item.wiredHandler;
                            message.AppendString(handler.BotName);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }
                        message.AppendInt32(1);
                        if (Item.wiredHandler != null)
                        {
                            BotGiveHandItem handler = (BotGiveHandItem)Item.wiredHandler;
                            message.AppendInt32(handler.HandItem);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(24);
                        if (Item.wiredHandler != null)
                        {
                            BotGiveHandItem handler = (BotGiveHandItem)Item.wiredHandler;
                            message.AppendUInt(handler.Time);
                        }
                        else
                        {
                            message.AppendUInt(0);
                        }
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionbotfollowavt:
                    {
                        ServerMessage message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredHandler != null)
                        {
                            BotFollowUser handler = (BotFollowUser)Item.wiredHandler;
                            message.AppendString(handler.BotName);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }
                        message.AppendInt32(1);
                        if (Item.wiredHandler != null)
                        {
                            BotFollowUser handler = (BotFollowUser)Item.wiredHandler;
                            message.AppendInt32(handler.StartFollow);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(25);
                        if (Item.wiredHandler != null)
                        {
                            BotFollowUser handler = (BotFollowUser)Item.wiredHandler;
                            message.AppendUInt(handler.Time);
                        }
                        else
                        {
                            message.AppendUInt(0);
                        }
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionmutetriggerer:
                    {
                        ServerMessage message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredHandler != null)
                        {
                            MuteTriggerer handler = (MuteTriggerer)Item.wiredHandler;
                            message.AppendString(handler.Message);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }
                        message.AppendInt32(1);
                        if (Item.wiredHandler != null)
                        {
                            MuteTriggerer handler = (MuteTriggerer)Item.wiredHandler;
                            message.AppendUInt(handler.MuteTime);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(20);
                        if (Item.wiredHandler != null)
                        {
                            MuteTriggerer handler = (MuteTriggerer)Item.wiredHandler;
                            message.AppendUInt(handler.Time);
                        }
                        else
                        {
                            message.AppendUInt(0);
                        }
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.actionmovetofurni:
                    {
                        ServerMessage message = new ServerMessage(Outgoing.WiredEffect);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredHandler != null)
                        {
                            MoveToFurni handler = (MoveToFurni)Item.wiredHandler;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                            message.AppendString(string.Empty);
                        message.AppendInt32(2);
                        if (Item.wiredHandler != null)
                        {
                            MoveToFurni handler = (MoveToFurni)Item.wiredHandler;
                            message.AppendInt32(handler.Direction);
                            message.AppendInt32(handler.Length);
                        }
                        else
                        {
                            message.AppendInt32(0);
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(16);
                        if (Item.wiredHandler != null)
                        {
                            MoveToFurni handler = (MoveToFurni)Item.wiredHandler;
                            message.AppendInt32(handler.Time);
                        }
                        else
                        {
                            message.AppendUInt(0);
                        }
                        message.AppendInt32(WiredException.Count);
                        foreach (var SpriteEx in WiredException)
                            message.AppendInt32(SpriteEx);
                        Session.SendMessage(message);
                        break;
                    }
                    
                case InteractionType.specialrandom:
                    {
                        // havent got panel... ^^
                        break;
                    }

                case InteractionType.specialunseen:
                    {
                        // havent got panel... ^^
                        break;
                    }
                #endregion

                #region Condiciones
                case InteractionType.conditionfurnishaveusers:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredCondition != null)
                        {
                            FurniHasUsers handler = (FurniHasUsers)Item.wiredCondition;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(1);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionhasfurnion:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredCondition != null)
                        {
                            FurniHasFurni handler = (FurniHasFurni)Item.wiredCondition;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(1);
                        if (Item.wiredCondition != null)
                        {
                            FurniHasFurni handler = (FurniHasFurni)Item.wiredCondition;
                            message.AppendInt32(handler.OnlyOneFurniOn);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(7);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditiontriggeronfurni:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredCondition != null)
                        {
                            TriggerUserIsOnFurni handler = (TriggerUserIsOnFurni)Item.wiredCondition;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(2);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionstatepos:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredCondition != null)
                        {
                            FurniStatePosMatch handler = (FurniStatePosMatch)Item.wiredCondition;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(3);
                        if (Item.wiredCondition != null)
                        {
                            FurniStatePosMatch handler = (FurniStatePosMatch)Item.wiredCondition;
                            message.AppendInt32(handler.FurniState);
                            message.AppendInt32(handler.FurniDirection);
                            message.AppendInt32(handler.FurniPosition);
                        }
                        else
                        {
                            message.AppendInt32(0);
                            message.AppendInt32(0);
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(0); // wired Id
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditiontimemorethan:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(1);
                        if (Item.wiredCondition != null)
                        {
                            MoreThanTimer handler = (MoreThanTimer)Item.wiredCondition;
                            message.AppendInt32(handler.Time);
                        }
                        else
                        {
                            message.AppendInt32(18);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(3);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditiontimelessthan:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(1);
                        if (Item.wiredCondition != null)
                        {
                            LessThanTimer handler = (LessThanTimer)Item.wiredCondition;
                            message.AppendInt32(handler.Time);
                        }
                        else
                        {
                            message.AppendInt32(18);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(4);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionactoringroup:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(10);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionactorinteam:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(1);
                        if (Item.wiredCondition != null)
                        {
                            ActorInTeam handler = (ActorInTeam)Item.wiredCondition;
                            message.AppendInt32((int)handler.Team);
                        }
                        else
                        {
                            message.AppendInt32(1);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(6);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionusercountin:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(2);
                        if (Item.wiredCondition != null)
                        {
                            UserCountIn handler = (UserCountIn)Item.wiredCondition;
                            message.AppendInt32((int)handler.MinUsers);
                            message.AppendInt32((int)handler.MaxUsers);
                        }
                        else
                        {
                            message.AppendInt32(1);
                            message.AppendInt32(50);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(5);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionstuffis:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredCondition != null)
                        {
                            StuffIs handler = (StuffIs)Item.wiredCondition;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(8); // wired Id
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionwearingeffect:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                            message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(1);
                        if (Item.wiredCondition != null)
                        {
                            UserWearingEffect handler = (UserWearingEffect)Item.wiredCondition;
                            message.AppendUInt(handler.Effect);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(12); // wired Id
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditiondaterange:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(2);
                        if (Item.wiredCondition != null)
                        {
                            DateRangeActive handler = (DateRangeActive)Item.wiredCondition;
                            message.AppendInt32(handler.StartDate);
                            message.AppendInt32(handler.EndDate);
                        }
                        else
                        {
                            message.AppendInt32(0);
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(24); // wired Id
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionnotfurnishaveusers:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredCondition != null)
                        {
                            NotFurniHasUsers handler = (NotFurniHasUsers)Item.wiredCondition;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(14);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionhandleitemid:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(1);
                        if (Item.wiredCondition != null)
                        {
                            HandleItemUser handler = (HandleItemUser)Item.wiredCondition;
                            message.AppendInt32(handler.HandleId);
                        }
                        else
                        {
                            message.AppendInt32(1);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(25);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionnotfurnion:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredCondition != null)
                        {
                            NotFurniHasFurni handler = (NotFurniHasFurni)Item.wiredCondition;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(1);
                        if (Item.wiredCondition != null)
                        {
                            NotFurniHasFurni handler = (NotFurniHasFurni)Item.wiredCondition;
                            message.AppendInt32(handler.OnlyOneFurniOn);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(18);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionnottriggeronfurni:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredCondition != null)
                        {
                            NotTriggerUserIsOnFurni handler = (NotTriggerUserIsOnFurni)Item.wiredCondition;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(15);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionnotstatepos:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredCondition != null)
                        {
                            NotFurniStatePosMatch handler = (NotFurniStatePosMatch)Item.wiredCondition;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(3);
                        if (Item.wiredCondition != null)
                        {
                            NotFurniStatePosMatch handler = (NotFurniStatePosMatch)Item.wiredCondition;
                            message.AppendInt32(handler.FurniState);
                            message.AppendInt32(handler.FurniDirection);
                            message.AppendInt32(handler.FurniPosition);
                        }
                        else
                        {
                            message.AppendInt32(0);
                            message.AppendInt32(0);
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(13); // wired Id
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionnotingroup:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(21);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionnotinteam:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(1);
                        if (Item.wiredCondition != null)
                        {
                            NotActorInTeam handler = (NotActorInTeam)Item.wiredCondition;
                            message.AppendInt32((int)handler.Team);
                        }
                        else
                        {
                            message.AppendInt32(1);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(17);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionnotusercount:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(2);
                        if (Item.wiredCondition != null)
                        {
                            NotUserCountIn handler = (NotUserCountIn)Item.wiredCondition;
                            message.AppendInt32((int)handler.MinUsers);
                            message.AppendInt32((int)handler.MaxUsers);
                        }
                        else
                        {
                            message.AppendInt32(1);
                            message.AppendInt32(50);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(16);
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionnotstuffis:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        if (Item.wiredCondition != null)
                        {
                            NotStuffIs handler = (NotStuffIs)Item.wiredCondition;

                            message.AppendInt32(handler.Items.Count);
                            foreach (var item in handler.Items)
                                message.AppendUInt(item.Id);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(19); // wired Id
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionnotwearingeffect:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        message.AppendString("");
                        message.AppendInt32(1);
                        if (Item.wiredCondition != null)
                        {
                            UserNotWearingEffect handler = (UserNotWearingEffect)Item.wiredCondition;
                            message.AppendUInt(handler.Effect);
                        }
                        else
                        {
                            message.AppendInt32(0);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(12); // wired Id
                        Session.SendMessage(message);
                        break;
                    }

                case InteractionType.conditionwearingbadge:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredCondition != null)
                        {
                            UserWearingBadge handler = (UserWearingBadge)Item.wiredCondition;
                            message.AppendString(handler.BadgeID);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(11); // wired Id
                        Session.SendMessage(message);

                        break;
                    }

                case InteractionType.conditionnotwearingbadge:
                    {
                        var message = new ServerMessage(Outgoing.WiredCondition);
                        message.AppendBoolean(false);
                        message.AppendInt32(5);
                        message.AppendInt32(0);
                        message.AppendInt32(Item.GetBaseItem().SpriteId);
                        message.AppendUInt(Item.Id);
                        if (Item.wiredCondition != null)
                        {
                            UserNotWearingBadge handler = (UserNotWearingBadge)Item.wiredCondition;
                            message.AppendString(handler.BadgeID);
                        }
                        else
                        {
                            message.AppendString(string.Empty);
                        }
                        message.AppendInt32(0);
                        message.AppendInt32(0);
                        message.AppendInt32(11); // wired Id
                        Session.SendMessage(message);
                        break;
                    }
                #endregion
            }
        }
    }

    class InteractorJukebox : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (UserHasRights == false || Session == null || Item == null || Request == -1)
                return;

            var RM = Session.GetHabbo().CurrentRoom.GetRoomMusicController();
            Session.SendMessage(JukeboxDiscksComposer.SerializeSongInventory(Session.GetHabbo().GetInventoryComponent().songDisks));
            if (RM.IsPlaying)
                RM.Stop();
            else
                RM.Start(Request);
        }
    }

    class InteractorPuzzleBox : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y)) // vamos a la caja
            {
                if (User.CanWalk)
                {
                    User.MoveTo(Item.SquareInFront);
                }

                return;
            }
            else
            {
                int NewX = 0, NewY = 0;
                if (User.X == Item.GetX && User.Y - 1 == Item.GetY)
                {
                    NewX = Item.GetX;
                    NewY = Item.GetY - 1;
                }
                else if (User.X + 1 == Item.GetX && User.Y == Item.GetY)
                {
                    NewX = Item.GetX + 1;
                    NewY = Item.GetY;
                }
                else if (User.X == Item.GetX && User.Y + 1 == Item.GetY)
                {
                    NewX = Item.GetX;
                    NewY = Item.GetY + 1;
                }
                else if (User.X - 1 == Item.GetX && User.Y == Item.GetY)
                {
                    NewX = Item.GetX - 1;
                    NewY = Item.GetY;
                }
                else
                {
                    if (User.CanWalk)
                    {
                        User.MoveTo(Item.SquareInFront);
                    }

                    return;
                }

                if (Item.GetRoom().GetGameMap().ValidTile(NewX, NewY))
                {
                    Room.GetRoomItemHandler().SetFloorItem(Session, Item, NewX, NewY, Item.Rot, false, false, true, true);
                    User.MoveTo(Item.OldX, Item.OldY);
                }
            }
        }
    }

    class InteractorChangeBackgrounds : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            if (Item == null)
                return;

            if (!UserHasRights)
                return;

            if (Request == 0)
            {
                if (Item.ExtraData.StartsWith("on"))
                    Item.ExtraData = Item.ExtraData.Replace("on", "off");
                else if (Item.ExtraData.StartsWith("off"))
                    Item.ExtraData = Item.ExtraData.Replace("off", "on");

                Item.UpdateState();
            }
        }
    }

    class InteractorManiqui : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            if (Item == null || Item.GetBaseItem() == null || Item.GetBaseItem().InteractionType != InteractionType.maniqui)
                return;

            if (!Item.ExtraData.Contains(";"))
                return;

            if (Session.GetHabbo().Gender.ToLower() != Item.ExtraData.Split(';')[0].ToLower()) // Si el ususario es de género Chico y el muñeco chica o alrevés, no vale.
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            // Generating New Look.
            var Parts = Session.GetHabbo().Look.Split('.');
            var NewLook = "";
            foreach (var Part in Parts)
            {
                if (Part.StartsWith("ch") || Part.StartsWith("lg") || Part.StartsWith("cc") || Part.StartsWith("ca") || Part.StartsWith("sh") || Part.StartsWith("wa"))
                    continue;
                NewLook += Part + ".";
            }
            if (Item.ExtraData.Split(';').Length > 0)
                NewLook += Item.ExtraData.Split(';')[1];
            else
                NewLook += "lg-270-82.ch-210-66";

            /*if (!OtanixEnvironment.GetGame().GetUserLookManager().IsValidLook(Session.GetHabbo(), NewLook))
            {
                Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id).WhisperComposer("El look que intentas ponerte no es válido para tu usuario.");
                return;
            }*/

            Session.GetHabbo().Look = NewLook;

            var UpdateLook = new ServerMessage(Outgoing.UpdateUserInformation);
            UpdateLook.AppendInt32(-1);
            UpdateLook.AppendString(Session.GetHabbo().Look);
            UpdateLook.AppendString(Session.GetHabbo().Gender.ToLower());
            UpdateLook.AppendString(Session.GetHabbo().Motto);
            UpdateLook.AppendUInt(Session.GetHabbo().AchievementPoints);
            Session.SendMessage(UpdateLook);

            if (Session.GetHabbo().InRoom)
            {
                var UpdateLookRoom = new ServerMessage(Outgoing.UpdateUserInformation);
                UpdateLookRoom.AppendInt32(Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id).VirtualId);
                UpdateLookRoom.AppendString(Session.GetHabbo().Look);
                UpdateLookRoom.AppendString(Session.GetHabbo().Gender.ToLower());
                UpdateLookRoom.AppendString(Session.GetHabbo().Motto);
                UpdateLookRoom.AppendUInt(Session.GetHabbo().AchievementPoints);
                Room.SendMessage(UpdateLookRoom);
            }
        }
    }

    class InteractorYttv : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            if (Item == null)
                return;

            if (!OtanixEnvironment.GetGame().GetYoutubeManager().Videos.ContainsKey((int)Item.GetBaseItem().ItemId))
                return;

            if (Item.videoOn.Length > 0 && Item.isStarted)
            {
                if (!OtanixEnvironment.GetGame().GetYoutubeManager().Videos[(int)Item.GetBaseItem().ItemId].Videos.ContainsKey(Item.videoOn))
                    Item.videoOn = OtanixEnvironment.GetGame().GetYoutubeManager().Videos[(int)Item.GetBaseItem().ItemId].favVideo;

                var _message = new ServerMessage(Outgoing.ShowYoutubeVideo);
                _message.AppendUInt(Item.Id);
                _message.AppendString(Item.videoOn);
                _message.AppendInt32(0);
                _message.AppendInt32(0);
                _message.AppendInt32(-1);
                Session.SendMessage(_message);
            }

            var message = new ServerMessage(Outgoing.OpenYoutubeTv);
            message.AppendUInt(Item.Id);
            message.AppendInt32(Item.videosInformation.Count); // videos count
            foreach (var videoInfo in Item.videosInformation)
            {
                if (videoInfo.Split('>').Length == 3)
                {
                    message.AppendString(videoInfo.Split('>')[0]);
                    message.AppendString(videoInfo.Split('>')[1]);
                    message.AppendString(videoInfo.Split('>')[2]);
                }
                else
                {
                    message.AppendString("");
                    message.AppendString("");
                    message.AppendString("");
                }
            }
            message.AppendString(Item.videoOn); // predetermined video
            Session.SendMessage(message);    
        }
    }

    class InteractorPirateCannon : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (Item == null || Item.GetRoom() == null || Item.GetRoom().GetRoomUserManager() == null)
                return;

            if (Request != 3)
            {
                var user = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Username);
                if (!Item.UserIsBehindCanone(user.Coordinate))
                    return;
            }

            if (Item.ExtraData == "" || Item.ExtraData == "1")
                Item.ExtraData = "0";
            else
                Item.ExtraData = "1";

            Item.UpdateState();

            if(Item.Rot == 0)
            {
                for(int i = 1; i < 4; i++)
                {
                    RoomUser kikedUser = Item.GetRoom().GetRoomUserManager().GetUserForSquare(Item.GetX - i, Item.GetY);
                    if(kikedUser != null && kikedUser.GetClient() != null)
                    {
                        if (!Item.GetRoom().CheckRights(kikedUser.GetClient()))
                        {
                            ServerMessage newm = new ServerMessage(Outgoing.GeneratingNotification);
                            newm.AppendString("info." + EmuSettings.HOTEL_LINK);
                            newm.AppendInt32(5);
                            newm.AppendString("image");
                            newm.AppendString(LanguageLocale.GetValue("canhao.imagem"));
                            newm.AppendString("title");
                            newm.AppendString("Kickado do Quarto");
                            newm.AppendString("message");
                            newm.AppendString("<i>Oh! Você foi kickado do jogo. Tente novamente! :D</i>");
                            newm.AppendString("linkTitle");
                            newm.AppendString("Entendido!");
                            newm.AppendString("linkUrl");
                            newm.AppendString("event:");
                            kikedUser.GetClient().SendMessage(newm);

                            Item.GetRoom().GetRoomUserManager().RemoveUserFromRoom(kikedUser.GetClient(), true, false);
                        }
                        break;
                    }
                }
            }
            else if (Item.Rot == 2)
            {
                for (int i = 1; i < 4; i++)
                {
                    RoomUser kikedUser = Item.GetRoom().GetRoomUserManager().GetUserForSquare(Item.GetX, Item.GetY - i);
                    if (kikedUser != null && kikedUser.GetClient() != null)
                    {
                        if (!Item.GetRoom().CheckRights(kikedUser.GetClient()))
                        {
                            ServerMessage newm = new ServerMessage(Outgoing.GeneratingNotification);
                            newm.AppendString("info." + EmuSettings.HOTEL_LINK);
                            newm.AppendInt32(5);
                            newm.AppendString("image");
                            newm.AppendString(LanguageLocale.GetValue("canhao.imagem"));
                            newm.AppendString("title");
                            newm.AppendString("Kickado do Quarto");
                            newm.AppendString("message");
                            newm.AppendString("<i>Oh! Você foi kickado do jogo. Tente novamente! :D</i>");
                            newm.AppendString("linkTitle");
                            newm.AppendString("Entendido!");
                            newm.AppendString("linkUrl");
                            newm.AppendString("event:");
                            kikedUser.GetClient().SendMessage(newm);

                            Item.GetRoom().GetRoomUserManager().RemoveUserFromRoom(kikedUser.GetClient(), true, false);
                        }
                        break;
                    }
                }
            }
            else if (Item.Rot == 4)
            {
                for (int i = 2; i < 5; i++)
                {
                    RoomUser kikedUser = Item.GetRoom().GetRoomUserManager().GetUserForSquare(Item.GetX + i, Item.GetY);
                    if (kikedUser != null)
                    {
                        if (!Item.GetRoom().CheckRights(kikedUser.GetClient()))
                        {
                            ServerMessage newm = new ServerMessage(Outgoing.GeneratingNotification);
                            newm.AppendString("info." + EmuSettings.HOTEL_LINK);
                            newm.AppendInt32(5);
                            newm.AppendString("image");
                            newm.AppendString(LanguageLocale.GetValue("canhao.imagem"));
                            newm.AppendString("title");
                            newm.AppendString("Kickado do Quarto");
                            newm.AppendString("message");
                            newm.AppendString("<i>Oh! Você foi kickado do jogo. Tente novamente! :D</i>");
                            newm.AppendString("linkTitle");
                            newm.AppendString("Entendido!");
                            newm.AppendString("linkUrl");
                            newm.AppendString("event:");
                            kikedUser.GetClient().SendMessage(newm);

                            Item.GetRoom().GetRoomUserManager().RemoveUserFromRoom(kikedUser.GetClient(), true, false);
                        }
                        break;
                    }
                }
            }
            else if (Item.Rot == 6)
            {
                for (int i = 2; i < 5; i++)
                {
                    RoomUser kikedUser = Item.GetRoom().GetRoomUserManager().GetUserForSquare(Item.GetX, Item.GetY + i);
                    if (kikedUser != null)
                    {
                        if (!Item.GetRoom().CheckRights(kikedUser.GetClient()))
                        {
                            ServerMessage newm = new ServerMessage(Outgoing.GeneratingNotification);
                            newm.AppendString("info." + EmuSettings.HOTEL_LINK);
                            newm.AppendInt32(5);
                            newm.AppendString("image");
                            newm.AppendString(LanguageLocale.GetValue("canhao.imagem"));
                            newm.AppendString("title");
                            newm.AppendString("Kickado do Quarto");
                            newm.AppendString("message");
                            newm.AppendString("<i>Oh! Você foi kickado do jogo. Tente novamente! :D</i>");
                            newm.AppendString("linkTitle");
                            newm.AppendString("Entendido!");
                            newm.AppendString("linkUrl");
                            newm.AppendString("event:");
                            kikedUser.GetClient().SendMessage(newm);

                            Item.GetRoom().GetRoomUserManager().RemoveUserFromRoom(kikedUser.GetClient(), true, false);
                        }
                        break;
                    }
                }
            }
        }
    }

    class InteractorWaterbowl : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }

        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (!UserHasRights)
            {
                return;
            }

            if (Session == null || Session.GetHabbo() == null)
                return;

            if (Item == null || Item.GetRoom() == null || Item.GetRoom().GetRoomUserManager() == null)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
            {
                User.MoveTo(Item.GetX, Item.GetY);
                return;
            }

            Item.ExtraData = "5";
            Item.UpdateState();
        }
    }

    class InteractorPetfood : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }
        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights) { }
    }

    class InteractorUsersLock : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }
        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (!UserHasRights)
                return;

            if (Item.GetBaseItem().InteractionType != InteractionType.userslock) // no es un furni de candado ??
                return;

            string[] usersLockParams = Item.ExtraData.Split(';');
            if (usersLockParams[0] == "1") // alredy locked
                return;

            if (Item.usersLock == null)
                return;

            if (Item.usersLock.roomUserOne != null && Item.usersLock.roomUserTwo != null)
                return;

            bool isOwnerItem = (Item.OwnerId == Session.GetHabbo().Id);
            if (Item.usersLock.roomUserOne != null && isOwnerItem)
                return;

            if (Item.usersLock.roomUserTwo != null && !isOwnerItem)
                return;

            RoomUser myUser = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (isOwnerItem)
            {
                Item.usersLock.roomUserOne = myUser;
                myUser.MoveTo(Item.RightSide);
            }
            else
            {
                if (!myUser.GetClient().GetHabbo().GetMessenger().FriendshipExists(Item.OwnerId))
                    return;

                Item.usersLock.roomUserTwo = myUser;
                myUser.MoveTo(Item.LeftSide);
            }

            if (Item.usersLock.roomUserOne != null && Item.usersLock.roomUserTwo != null)
            {
                ServerMessage LockDialogueOwner = new ServerMessage(Outgoing.LoveLockDialogueMessageComposer);
                LockDialogueOwner.AppendUInt(Item.Id);
                LockDialogueOwner.AppendBoolean(true);
                Item.usersLock.roomUserOne.GetClient().SendMessage(LockDialogueOwner);

                ServerMessage LockDialogueUser = new ServerMessage(Outgoing.LoveLockDialogueMessageComposer);
                LockDialogueUser.AppendUInt(Item.Id);
                LockDialogueUser.AppendBoolean(false);
                Item.usersLock.roomUserTwo.GetClient().SendMessage(LockDialogueUser);
            }
        }
    }

    class InteractorVikingHouse : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }
        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if(User.CurrentEffect == 5 || User.CurrentEffect == 172 || User.CurrentEffect == 173)
            {
                if(Item.ExtraData != "5" && !Item.VikingHouseBurning && Gamemap.TilesTouching2x2(Item.GetX, Item.GetY,User.X, User.Y))
                {
                    Item.ExtraData = "1";
                    Item.UpdateNeeded = true;
                    Item.VikingHouseBurning = true;
                    OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_ViciousViking", 1);
                }
            }
        }
    }

    class InteractorWiredClassification : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }
        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (!UserHasRights)
                return;

            if (Item.wiredPuntuation != null)
            {
                Item.wiredPuntuation.ChangeEnable();
                Item.UpdateState(false, true);
            }
        }
    }

    class InteractorGnomeBox : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }
        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (!UserHasRights)
                return;

            if (Item.GetBaseItem().InteractionType != InteractionType.gnomebox)
                return;

            ServerMessage Message = new ServerMessage(Outgoing.GnomeBoxPanel);
            Message.AppendUInt(Item.Id);
            Session.SendMessage(Message);
        }
    }

    class InteractorFxBox : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }
        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (!UserHasRights)
                return;

            if (Session == null || Session.GetHabbo() == null)

                if (Item == null || Item.GetBaseItem() == null || !Item.GetBaseItem().Name.StartsWith("fxbox_fx"))
                    return;

            if (Item.GetBaseItem().InteractionType != InteractionType.fxbox)
                return;

            Room Room = Item.GetRoom();
            if (Room == null || Room.GetRoomItemHandler() == null)
                return;

            int EffectId = int.Parse(Item.GetBaseItem().Name.Replace("fxbox_fx", ""));

            Room.GetRoomItemHandler().RemoveFurniture(Session, Item);
            Session.GetHabbo().GetAvatarEffectsInventoryComponent().AddEffect(EffectId, -1); // -1 == permanent
        }
    }

    class InteractorBalloon15 : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }
        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (!UserHasRights)
                return;

            if (Item.GetBaseItem().InteractionType != InteractionType.balloon15)
                return;

            Room Room = Item.GetRoom();
            if (Room == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
            {
                User.MoveTo(Item.SquareInFront);
            }
            else
            {
                OtanixEnvironment.GetGame().GetPiñataManager().DeliverBalloonRandomItem(User, Room, Item);
            }
        }
    }

    class InteractorDalia : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }
        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (!UserHasRights)
                return;

            if (Item.GetBaseItem().InteractionType != InteractionType.dalia)
                return;

            Room Room = Item.GetRoom();
            if (Room == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
            {
                User.MoveTo(Item.SquareInFront);
            }
            else
            {
                if (Item.ExtraData.Length > 0)
                {
                    var golpesdados = int.Parse(Item.ExtraData);
                    if (golpesdados < Item.GetBaseItem().VendingIds[0])
                    {
                        if (User.CurrentEffect == 192) // el efecto de Regadera!
                        {
                            golpesdados++;
                            Item.ExtraData = golpesdados.ToString();
                            Item.UpdateState();

                            if (golpesdados >= Item.GetBaseItem().VendingIds[0]) // regalito! rompemos piñata!
                            {
                                // Ponemos el Item en cola para que pueda relizar el efecto "12" y luego vuelva a la normalidad.
                                Item.UpdateCounter = 2;
                                Item.UpdateNeeded = true;

                                // OtanixEnvironment.GetGame().GetPiñataManager().DeliverPiñataRandomItem(User, Room, Item);
                                OtanixEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session.GetHabbo().Id, "ACH_Horticulturist", 1);
                            }
                        }
                    }
                }
            }
        }
    }

    class InteractorCraftable : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }
        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (!UserHasRights)
                return;

            if (Item.GetBaseItem().InteractionType != InteractionType.craftable)
                return;

            Session.SendMessage(OtanixEnvironment.GetGame().GetCraftableProductsManager().GetMessage());
        }
    }

    class InteractorSeed : FurniInteractor
    {
        internal override void OnPlace(GameClient Session, RoomItem Item) { }
        internal override void OnRemove(GameClient Session, RoomItem Item) { }
        internal override void OnTrigger(GameClient Session, RoomItem Item, int Request, bool UserHasRights)
        {
            if (!UserHasRights)
                return;

            if (Item.GetBaseItem().InteractionType != InteractionType.seed)
                return;

            Room Room = Item.GetRoom();
            if (Room == null)
                return;

            string PlantName = PlantsName.GenerateRandomName(int.Parse(Item.ExtraData));

            Room.GetRoomItemHandler().RemoveFurniture(Session, Item);

            Pet Pet = Catalog.CreatePet(Session, PlantName, 16, "0", "ffffff");
            if (Pet == null)
                return;

            RoomUser PetUser = Room.GetRoomUserManager().DeployBot(new RoomBot(Pet.PetId, Pet.OwnerId, Pet.RoomId, AIType.Pet, true, Pet.Name, "", "", Pet.Look, Item.GetX, Item.GetY, 0, 0, false, "", 0, false), Pet);
            if (PetUser == null)
                return;

            PetUser.CanWalk = false;

            if (Pet.DBState != DatabaseUpdateState.NeedsInsert)
                Pet.DBState = DatabaseUpdateState.NeedsUpdate;
        }
    }
}
