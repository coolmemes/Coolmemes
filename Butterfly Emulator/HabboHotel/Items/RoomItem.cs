using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Butterfly.Core;
using Butterfly.HabboHotel.Items.Interactors;
using Butterfly.HabboHotel.Pathfinding;
using Butterfly.HabboHotel.Rooms;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.Messages;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using ButterStorm;
using HabboEvents;
using ButterStorm.HabboHotel.Rooms;
using Butterfly.HabboHotel.Rooms.Wired;
using Butterfly.HabboHotel.Pets;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Classifications;
using Butterfly.HabboHotel.Users;
using Butterfly.HabboHotel.Misc;
using Newtonsoft.Json;

namespace Butterfly.HabboHotel.Items
{
    public delegate void OnItemTrigger(object sender, ItemTriggeredArgs e);
    [JsonObject(MemberSerialization.OptIn)]
    public class RoomItem : IEquatable<RoomItem>
    {
        [JsonProperty]
        internal UInt32 Id;
        internal UInt32 PremiumId;
        internal UInt32 RoomId;
        internal UInt32 BaseItem;
        internal string ExtraData;
        internal string GroupData;
        internal Int32 LimitedValue;
        internal UInt32 OwnerId;
        internal uint interactingBallUser;
        internal Team team;
        internal byte interactionCountHelper;
        public byte interactionCount;
        internal int value;
        internal FreezePowerUp freezePowerUp;
        internal IWiredCondition wiredCondition;
        internal MovementState escapeMovement;
        internal MovementDirection movetodirMovement = MovementDirection.none;
        internal string originalExtraData;
        internal List<RoomUser> wiredEventUser;
        internal Puntuations wiredPuntuation;
        internal bool ScoreboardIsPaused;
        internal bool IsPremiumItem;

        internal ComeDirection comeDirection;
        internal int _iBallValue;
        internal RoomUser ballMover;
        internal bool ballIsMoving;

        internal UsersLock usersLock;
        internal Boolean VikingHouseBurning;
        internal uint teleLink;
        internal List<Pet> havepetscount = new List<Pet>(2);

        // if is a yttv:
        internal bool isStarted;
        internal string videoOn;
        internal string tvImage;
        internal List<string> videosInformation;

        internal IWiredTrigger wiredHandler;
        internal event OnItemTrigger itemTriggerEventHandler;
        internal event UserWalksFurniDelegate OnUserWalksOffFurni;
        internal event UserWalksFurniDelegate OnUserWalksOnFurni;
        internal event UserWalksFurniDelegate OnBotWalksOnFurni;

        private Dictionary<int, ThreeDCoord> mAffectedPoints;

        internal Dictionary<int, ThreeDCoord> GetAffectedTiles
        {
            get
            {
                return mAffectedPoints;
            }
        }

        private Dictionary<int, ThreeDCoord> mBackupAffectedPoints;

        internal Dictionary<int, ThreeDCoord> GetBackupAffectedTiles
        {
            get
            {
                return mBackupAffectedPoints;
            }
        }

        private int mX; //byte
        [JsonProperty]
        internal int GetX
        {
            get
            {
                return mX;
            }
        }

        internal int OldX;
        internal int OldY;
        internal int OldRot;

        private int mY;//byte
        [JsonProperty]
        internal int GetY
        {
            get
            {
                return mY;
            }
        }

        private Double mZ; //Float??
        [JsonProperty]
        internal Double GetZ
        {
            get
            {
                return mZ;
            }
        }

        internal void SetState(int pX, int pY)
        {
            mX = pX;
            mY = pY;
        }

        internal void SetState(int pX, int pY, Double pZ, Dictionary<int, ThreeDCoord> Tiles)
        {
            mX = pX;
            mY = pY;
            if (!double.IsInfinity(pZ))
            {
                mZ = pZ;
            }
            mBackupAffectedPoints = mAffectedPoints;
            mAffectedPoints = Tiles;
        }

        internal void SetHeight(double pZ)
        {
            if (!double.IsInfinity(pZ))
            {
                mZ = pZ;
            }
        }

        internal int Rot;

        internal WallCoordinate wallCoord;

        private bool updateNeeded;
        internal bool UpdateNeeded
        {
            get
            {
                return updateNeeded;
            }
            set
            {
                if (value == true)
                    GetRoom().GetRoomItemHandler().QueueRoomItemUpdate(this);

                updateNeeded = value;
            }
        }

        internal int UpdateCounter;

        internal UInt32 InteractingUser;
        internal UInt32 InteractingUser2;

        private Room mRoom;
        private readonly bool mIsWallItem;
        private readonly bool mIsFloorItem;

        private readonly bool mIsRoller;
        internal bool IsTrans;
        internal bool pendingReset = false;
        internal bool MagicRemove = false;

        internal bool IsRoller
        {
            get
            {
                return mIsRoller;
            }
        }

        internal Point Coordinate
        {
            get
            {
                return new Point(mX, mY);
            }
        }
        internal Point RightSide
        {
            get
            {
                if (Rot == 0)
                    return new Point(mX + 1, mY);
                else if (Rot == 2)
                    return new Point(mX, mY + 1);
                else if (Rot == 4)
                    return new Point(mX - 1, mY);
                else if (Rot == 6)
                    return new Point(mX, mY - 1);

                return new Point(mX, mY);
            }
        }

        internal Point LeftSide
        {
            get
            {
                if (Rot == 0)
                    return new Point(mX - 1, mY);
                else if (Rot == 2)
                    return new Point(mX, mY - 1);
                else if (Rot == 4)
                    return new Point(mX + 1, mY);
                else if (Rot == 6)
                    return new Point(mX, mY + 1);

                return new Point(mX, mY);
            }
        }

        internal List<Point> GetCoords
        {
            get
            {
                try
                {
                    var toReturn = new List<Point>();
                    toReturn.AddRange(mAffectedPoints.Values.Select(tile => new Point(tile.X, tile.Y)));

                    return toReturn;
                }
                catch
                {
                    return null;
                }
            }
        }

        internal double TotalHeight
        {
            get
            {
                if (GetBaseItem() == null)
                    return mZ;

                if (this.GetBaseItem().MultiHeight.Count > 0)
                {
                    if (ExtraData == "")
                        ExtraData = "0";

                    return mZ + this.GetBaseItem().MultiHeight[int.Parse(ExtraData)];
                }
                else
                {
                    return mZ + GetBaseItem().Height;
                }
            }
        }

        internal bool IsWallItem
        {
            get
            {
                return mIsWallItem;
            }
        }

        internal bool IsFloorItem
        {
            get
            {
                return mIsFloorItem;
            }
        }

        internal Point SquareInFront
        {
            get
            {
                var Sq = new Point(mX, mY);

                if (Rot == 0)
                {
                    Sq.Y--;
                }
                else if (Rot == 2)
                {
                    Sq.X++;
                }
                else if (Rot == 4)
                {
                    Sq.Y++;
                }
                else if (Rot == 6)
                {
                    Sq.X--;
                }

                return Sq;
            }
        }

        internal Point SquareBehind
        {
            get
            {
                var Sq = new Point(mX, mY);

                if (Rot == 0)
                {
                    Sq.Y++;
                }
                else if (Rot == 2)
                {
                    Sq.X--;
                }
                else if (Rot == 4)
                {
                    Sq.Y--;
                }
                else if (Rot == 6)
                {
                    Sq.X++;
                }

                return Sq;
            }
        }

        internal Boolean UserIsBehindCanone(Point Coordinate)
        {
            if (Rot == 0)
            {
                Point CanoneSquareBehind = new Point(GetX + 2, GetY);
                if (Coordinate == CanoneSquareBehind || (Coordinate.X == CanoneSquareBehind.X && Coordinate.Y == CanoneSquareBehind.Y + 1) || (Coordinate.X == CanoneSquareBehind.X && Coordinate.Y == CanoneSquareBehind.Y - 1) || (Coordinate.X == CanoneSquareBehind.X - 1 && Coordinate.Y == CanoneSquareBehind.Y - 1) || (Coordinate.X == CanoneSquareBehind.X - 1 && Coordinate.Y == CanoneSquareBehind.Y + 1))
                    return true;
            }
            else if (Rot == 2)
            {
                Point CanoneSquareBehind = new Point(GetX, GetY + 2);
                if (Coordinate == CanoneSquareBehind || (Coordinate.X == CanoneSquareBehind.X + 1 && Coordinate.Y == CanoneSquareBehind.Y) || (Coordinate.X == CanoneSquareBehind.X - 1 && Coordinate.Y == CanoneSquareBehind.Y) || (Coordinate.X == CanoneSquareBehind.X + 1 && Coordinate.Y == CanoneSquareBehind.Y - 1) || (Coordinate.X == CanoneSquareBehind.X - 1 && Coordinate.Y == CanoneSquareBehind.Y - 1))
                    return true;
            }
            else if (Rot == 4)
            {
                Point CanoneSquareBehind = new Point(GetX - 1, GetY);
                if (Coordinate == CanoneSquareBehind || (Coordinate.X == CanoneSquareBehind.X && Coordinate.Y == CanoneSquareBehind.Y + 1) || (Coordinate.X == CanoneSquareBehind.X && Coordinate.Y == CanoneSquareBehind.Y - 1) || (Coordinate.X == CanoneSquareBehind.X + 1 && Coordinate.Y == CanoneSquareBehind.Y + 1) || (Coordinate.X == CanoneSquareBehind.X + 1 && Coordinate.Y == CanoneSquareBehind.Y - 1))
                    return true;
            }
            else if (Rot == 6)
            {
                Point CanoneSquareBehind = new Point(GetX, GetY - 1);
                if (Coordinate == CanoneSquareBehind || (Coordinate.X == CanoneSquareBehind.X - 1 && Coordinate.Y == CanoneSquareBehind.Y) || (Coordinate.X == CanoneSquareBehind.X + 1 && Coordinate.Y == CanoneSquareBehind.Y) || (Coordinate.X == CanoneSquareBehind.X - 1 && Coordinate.Y == CanoneSquareBehind.Y + 1) || (Coordinate.X == CanoneSquareBehind.X + 1 && Coordinate.Y == CanoneSquareBehind.Y + 1))
                    return true;
            }

            return false;
        }

        internal FurniInteractor Interactor
        {
            get
            {
                if (GetBaseItem() == null)
                    return new InteractorNone();

                switch (GetBaseItem().InteractionType)
                {
                    case InteractionType.banzaipuck:
                        return new InteractorBanzaiPuck();
                    case InteractionType.saltasalas:
                    case InteractionType.teleport:
                        return new InteractorTeleport();
                    case InteractionType.bottle:
                        return new InteractorSpinningBottle();
                    case InteractionType.dice:
                        return new InteractorDice();
                    case InteractionType.habbowheel:
                        return new InteractorHabboWheel();
                    case InteractionType.loveshuffler:
                        return new InteractorLoveShuffler();
                    case InteractionType.onewaygate:
                        return new InteractorOneWayGate();
                    case InteractionType.alert:
                        return new InteractorAlert();
                    case InteractionType.vendingmachine:
                        return new InteractorVendor();
                    case InteractionType.gate:
                        return new InteractorGate(GetBaseItem().Modes);
                    case InteractionType.scoreboard:
                        return new InteractorScoreboard();
                    case InteractionType.football:
                        return new InteractorFootball();
                    case InteractionType.footballcounterblue:
                    case InteractionType.footballcountergreen:
                    case InteractionType.footballcounterred:
                    case InteractionType.footballcounteryellow:
                        return new InteractorScoreCounter();
                    case InteractionType.guildgate:
                    case InteractionType.banzaifloor:
                        return new InteractorNone();
                    case InteractionType.banzaiscoreblue:
                    case InteractionType.banzaiscoregreen:
                    case InteractionType.banzaiscorered:
                    case InteractionType.banzaiscoreyellow:
                        return new InteractorBanzaiScoreCounter();
                    case InteractionType.freezetile:
                    case InteractionType.freezetileblock:
                        return new InteractorFreezeTile();
                    case InteractionType.triggertimer:
                    case InteractionType.triggerroomenter:
                    case InteractionType.triggergameend:
                    case InteractionType.triggergamestart:
                    case InteractionType.triggerrepeater:
                    case InteractionType.triggeronusersay:
                    case InteractionType.triggerscoreachieved:
                    case InteractionType.triggerstatechanged:
                    case InteractionType.triggerwalkonfurni:
                    case InteractionType.triggerwalkofffurni:
                    case InteractionType.triggercollision:
                    case InteractionType.triggerlongperiodic:
                    case InteractionType.triggerbotreachedavtr:
                    case InteractionType.triggerbotreachedstf:
                    case InteractionType.actiongivescore:
                    case InteractionType.actionposreset:
                    case InteractionType.actionmoverotate:
                    case InteractionType.actionresettimer:
                    case InteractionType.actionshowmessage:
                    case InteractionType.actionhandiitemcustom:
                    case InteractionType.actioneffectcustom:
                    case InteractionType.actiondiamantescustom:
                    case InteractionType.actiondancecustom:
                    case InteractionType.actionfastwalk:
                    case InteractionType.actionfreezecustom:
                    case InteractionType.actionteleportto:
                    case InteractionType.actiontogglestate:
                    case InteractionType.actionchase:
                    case InteractionType.actiongivereward:
                    case InteractionType.actionkickuser:
                    case InteractionType.actionescape:
                    case InteractionType.actionjointoteam:
                    case InteractionType.actionleaveteam:
                    case InteractionType.actiongiveteamscore:
                    case InteractionType.actioncallstacks:
                    case InteractionType.actionmovetodir:
                    case InteractionType.actionbotmove:
                    case InteractionType.actionbotteleport:
                    case InteractionType.actionbotwhisper:
                    case InteractionType.actionbotclothes:
                    case InteractionType.actionbotfollowavt:
                    case InteractionType.actionbothanditem:
                    case InteractionType.actionbottalk:
                    case InteractionType.actionmutetriggerer:
                    case InteractionType.actionmovetofurni:
                    case InteractionType.conditionfurnishaveusers:
                    case InteractionType.conditionstatepos:
                    case InteractionType.conditiontimelessthan:
                    case InteractionType.conditiontimemorethan:
                    case InteractionType.conditiontriggeronfurni:
                    case InteractionType.conditionhasfurnion:
                    case InteractionType.conditionactoringroup:
                    case InteractionType.conditionactorinteam:
                    case InteractionType.conditionusercountin:
                    case InteractionType.conditionstuffis:
                    case InteractionType.conditionhandleitemid:
                    case InteractionType.conditionnotfurnion:
                    case InteractionType.conditionnotfurnishaveusers:
                    case InteractionType.conditionnotingroup:
                    case InteractionType.conditionnotinteam:
                    case InteractionType.conditionnotstatepos:
                    case InteractionType.conditionnotstuffis:
                    case InteractionType.conditionnottriggeronfurni:
                    case InteractionType.conditionnotusercount:
                    case InteractionType.conditionwearingeffect:
                    case InteractionType.conditionnotwearingeffect:
                    case InteractionType.conditiondaterange:
                    case InteractionType.conditionwearingbadge:
                    case InteractionType.conditionnotwearingbadge:
                        return new WiredInteractor();
                    case InteractionType.puzzlebox:
                        return new InteractorPuzzleBox();
                    case InteractionType.jukebox:
                        return new InteractorJukebox();
                    case InteractionType.maniqui:
                        return new InteractorManiqui();
                    case InteractionType.changeBackgrounds:
                        return new InteractorChangeBackgrounds();
                    case InteractionType.yttv:
                        return new InteractorYttv();
                    case InteractionType.mutesignal:
                        return new InteractorMuteSignal();
                    case InteractionType.piratecannon:
                        return new InteractorPirateCannon();
                    case InteractionType.waterbowl:
                        return new InteractorWaterbowl();
                    case InteractionType.petfood:
                        return new InteractorPetfood();
                    case InteractionType.userslock:
                        return new InteractorUsersLock();
                    case InteractionType.vikinghouse:
                        return new InteractorVikingHouse();
                    case InteractionType.wiredClassification:
                        return new InteractorWiredClassification();
                    case InteractionType.gnomebox:
                        return new InteractorGnomeBox();
                    case InteractionType.fxbox:
                        return new InteractorFxBox();
                    case InteractionType.balloon15:
                        return new InteractorBalloon15();
                    case InteractionType.dalia:
                        return new InteractorDalia();
                    case InteractionType.craftable:
                        return new InteractorCraftable();
                    case InteractionType.seed:
                        return new InteractorSeed();
                    case InteractionType.none:
                    default:
                        return new InteractorGenericSwitch();
                }
            }
        }

        internal void OnTrigger(RoomUser user)
        {
            if (itemTriggerEventHandler != null)
                itemTriggerEventHandler(null, new ItemTriggeredArgs(user, this));
        }

        internal RoomItem(UInt32 Id, UInt32 RoomId, UInt32 BaseItem, string ExtraData, uint OwnerId, int X, int Y, Double Z, int Rot, Room pRoom, bool IsPremiumItem)
        {
            this.Id = Id;
            this.RoomId = RoomId;
            this.BaseItem = BaseItem;
            this.ExtraData = ExtraData;
            this.originalExtraData = ExtraData;
            this.OwnerId = OwnerId;
            this.mX = X;
            this.mY = Y;
            if (!double.IsInfinity(Z))
                this.mZ = Z;
            this.Rot = Rot;
            this.UpdateNeeded = false;
            this.UpdateCounter = 0;
            this.InteractingUser = 0;
            this.InteractingUser2 = 0;
            this.IsTrans = false;
            this.interactingBallUser = 0;
            this.interactionCount = 0;
            this.value = 0;
            this.videosInformation = new List<string>();
            this.wiredEventUser = new List<RoomUser>();
            this.OldX = -1;
            this.OldY = -1;
            this.OldRot = -1;
            this.IsPremiumItem = IsPremiumItem;

            if (GetBaseItem().Name.StartsWith("gld_") || GetBaseItem().Name.StartsWith("guild_") || GetBaseItem().Name.Contains("grp"))
            {
                this.GroupData = ExtraData;
                this.ExtraData = GroupData.Split(';')[0];

                // for fix!
                if (GroupData.Contains(";;"))
                {
                    this.GroupData = GroupData.Replace(";;", ";");
                    mRoom.GetRoomItemHandler().UpdateItem(this);
                }
            }

            if (GetBaseItem().LimitedStack > 0)
            {
                try
                {
                    this.LimitedValue = int.Parse(this.ExtraData.Split(';')[1]);
                    this.ExtraData = this.ExtraData.Split(';')[0];
                }
                catch // fix
                {
                    this.LimitedValue = -1;
                    this.ExtraData = "0";
                    pRoom.GetRoomItemHandler().RemoveFurniture(null, this);
                }
            }

            this.mRoom = pRoom; //Todo: rub my penis

            switch (GetBaseItem().InteractionType)
            {
                case InteractionType.saltasalas:

                    IsTrans = true;
                    ReqUpdate(0, true);

                    break;

                case InteractionType.teleport:

                    if (ExtraData.Contains(";"))
                    {
                        teleLink = Convert.ToUInt32(ExtraData.Split(';')[1]);
                        ExtraData = ExtraData.Split(';')[0];
                    }

                    IsTrans = true;
                    ReqUpdate(0, true);

                    break;

                case InteractionType.roller:
                    mIsRoller = true;
                    pRoom.GetRoomItemHandler().GotRollers = true;
                    break;

                case InteractionType.banzaiscoreblue:
                case InteractionType.footballcounterblue:
                case InteractionType.footballgoalblue:
                case InteractionType.banzaigateblue:
                case InteractionType.freezebluegate:
                case InteractionType.freezebluecounter:
                    team = Team.blue;
                    break;

                case InteractionType.banzaiscoregreen:
                case InteractionType.footballcountergreen:
                case InteractionType.footballgoalgreen:
                case InteractionType.banzaigategreen:
                case InteractionType.freezegreencounter:
                case InteractionType.freezegreengate:
                    team = Team.green;
                    break;

                case InteractionType.banzaiscorered:
                case InteractionType.footballcounterred:
                case InteractionType.footballgoalred:
                case InteractionType.banzaigatered:
                case InteractionType.freezeredcounter:
                case InteractionType.freezeredgate:
                    team = Team.red;
                    break;

                case InteractionType.banzaiscoreyellow:
                case InteractionType.footballcounteryellow:
                case InteractionType.footballgoalyellow:
                case InteractionType.banzaigateyellow:
                case InteractionType.freezeyellowcounter:
                case InteractionType.freezeyellowgate:
                    team = Team.yellow;
                    break;

                case InteractionType.banzaitele:
                    {
                        this.ExtraData = "";
                        break;
                    }

                case InteractionType.yttv:
                    {
                        try
                        {
                            // 1;bXprqAkr_fQ;5;PL4F5KzcUTpEf4MFGEr_RoXjnNMNqtRToE,#HabboPalooza! Spain,Click on the youtube logo to access the subtitles in your language

                            if (this.ExtraData.Length > 2 && this.ExtraData.Contains(";"))
                            {
                                isStarted = (this.ExtraData.Split(';')[0] == "1");
                                videoOn = this.ExtraData.Split(';')[1];
                                tvImage = LanguageLocale.GetValue("habbo.imaging.yttv") + videoOn;
                                var VideosCount = OtanixEnvironment.GetGame().GetYoutubeManager().Videos[(int)GetBaseItem().ItemId].Videos.Count;
                                videosInformation.AddRange(OtanixEnvironment.GetGame().GetYoutubeManager().Videos[(int)GetBaseItem().ItemId].Videos.Values);
                            }
                        }
                        catch
                        {
                            GetRoom().GetRoomItemHandler().RemoveFurniture(null, this);
                        }
                        break;
                    }

                case InteractionType.breedingpet:
                    {
                        if (!pRoom.GetRoomItemHandler().breedingPet.ContainsKey(Id))
                            pRoom.GetRoomItemHandler().breedingPet.Add(Id, this);

                        break;
                    }           
                case InteractionType.waterbowl:
                    {
                        if (!pRoom.GetRoomItemHandler().waterBowls.ContainsKey(Id))
                            pRoom.GetRoomItemHandler().waterBowls.Add(Id, this);

                        break;
                    }
                case InteractionType.pethomes:
                    {
                        if (!pRoom.GetRoomItemHandler().petHomes.ContainsKey(Id))
                            pRoom.GetRoomItemHandler().petHomes.Add(Id, this);

                        break;
                    }
                case InteractionType.petfood:
                    {
                        if (!pRoom.GetRoomItemHandler().petFoods.ContainsKey(Id))
                            pRoom.GetRoomItemHandler().petFoods.Add(Id, this);

                        break;
                    }
                case InteractionType.userslock:
                    {
                        if (this.ExtraData.Split(';')[0] == "0")
                            this.usersLock = new UsersLock(this);

                        break;
                    }

                case InteractionType.wiredClassification:
                    {
                        this.wiredPuntuation = new Puntuations(this);
                        this.wiredPuntuation.CheckTimeOnline();

                        break;
                    }
            }

            this.mIsWallItem = (GetBaseItem().Type.ToString().ToLower() == "i");
            this.mIsFloorItem = (GetBaseItem().Type.ToString().ToLower() == "s");
            this.mAffectedPoints = Gamemap.GetAffectedTiles(GetBaseItem().Length, GetBaseItem().Width, mX, mY, Rot);
            this.mBackupAffectedPoints = new Dictionary<int, ThreeDCoord>();
        }

        internal RoomItem(UInt32 Id, UInt32 RoomId, UInt32 BaseItem, string ExtraData, uint OwnerId, WallCoordinate wallCoord, Room pRoom, bool IsPremiumItem)
        {
            this.Id = Id;
            this.RoomId = RoomId;
            this.BaseItem = BaseItem;
            this.ExtraData = ExtraData;
            this.originalExtraData = ExtraData;
            this.OwnerId = OwnerId;
            this.mX = 0;
            this.mY = 0;
            this.mZ = 0.0;
            this.UpdateNeeded = false;
            this.UpdateCounter = 0;
            this.InteractingUser = 0;
            this.InteractingUser2 = 0;
            this.IsTrans = false;
            this.interactingBallUser = 0;
            this.interactionCount = 0;
            this.value = 0;
            this.wallCoord = wallCoord;
            this.videosInformation = new List<string>();
            this.wiredEventUser = new List<RoomUser>();
            this.OldX = -1;
            this.OldY = -1;
            this.mRoom = pRoom;
            this.mIsWallItem = true;
            this.mIsFloorItem = false;
            this.mBackupAffectedPoints = new Dictionary<int, ThreeDCoord>();
            this.mAffectedPoints = new Dictionary<int, ThreeDCoord>();
            this.IsPremiumItem = IsPremiumItem;
        }

        internal void ClearCoordinates()
        {
            this.mX = -1;
            this.mY = -1;
        }

        internal void Destroy()
        {
            mRoom = null;
            mBackupAffectedPoints.Clear();
            mAffectedPoints.Clear();

            if (wiredHandler != null)
                wiredHandler.Dispose();
            wiredHandler = null;

            itemTriggerEventHandler = null;
            OnUserWalksOffFurni = null;
            OnUserWalksOnFurni = null;
            OnBotWalksOnFurni = null;
        }

        public bool Equals(RoomItem comparedItem)
        {
            return (comparedItem.Id == Id);
        }

        internal void ProcessUpdates()
        {
            if (GetBaseItem() == null)
                return;

            UpdateCounter--;

            if (UpdateCounter <= 0 || IsTrans)
            {
                UpdateNeeded = false;
                UpdateCounter = 0;

                RoomUser User = null;
                RoomUser User2 = null;

                switch (GetBaseItem().InteractionType)
                {
                    case InteractionType.gift:
                        // do nothing
                        break;
                    case InteractionType.onewaygate:

                        User = null;

                        if (InteractingUser > 0)
                        {
                            User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);
                            //GetRoom().FreeSqareForUsers(mX, mY);
                        }

                        if (User != null && User.X == mX && User.Y == mY)
                        {
                            ExtraData = "1";

                            User.MoveTo(SquareBehind);

                            ReqUpdate(1, false);
                            UpdateState(false, true);
                        }
                        else if (User != null && User.Coordinate == SquareBehind)
                        {
                            User.UnlockWalking();

                            ExtraData = "0";
                            InteractingUser = 0;

                            UpdateState(false, true);
                        }
                        else if (ExtraData == "1")
                        {
                            ExtraData = "0";
                            UpdateState(false, true);
                        }

                        if (User == null)
                        {
                            InteractingUser = 0;
                        }

                        break;

                    case InteractionType.saltasalas:
                        {
                            User = null;
                            User2 = null;

                            var keepDoorOpen = false;
                            var showTeleEffect = false;

                            // Do we have a primary user that wants to go somewhere?
                            if (InteractingUser > 0)
                            {
                                User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);

                                // Is this user okay?
                                if (User != null)
                                {
                                    // Is he in the tele?
                                    if (User.Coordinate == Coordinate)
                                    {
                                        //Remove the user from the square
                                        User.AllowOverride = false;

                                        uint TeleId = 0;
                                        uint RoomId = 0;

                                        if (TeleHandler.GetSaltaSalasRoomId(GetBaseItem().SpriteId, out TeleId, out RoomId))
                                        {
                                            showTeleEffect = true;

                                            // Do we need to tele to the same room or gtf to another?
                                            if (RoomId == this.RoomId)
                                            {
                                                var Item = GetRoom().GetRoomItemHandler().GetItem(TeleId);

                                                if (Item == null)
                                                {
                                                    User.UnlockWalking();
                                                }
                                                else
                                                {
                                                    // Set pos
                                                    User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                                                    User.SetRot(Item.Rot, false);

                                                    // Force tele effect update (dirty)
                                                    Item.ExtraData = "2";
                                                    Item.UpdateState(false, true);

                                                    // Set secondary interacting user
                                                    Item.InteractingUser2 = InteractingUser;
                                                }
                                            }
                                            else
                                            {
                                                // Let's run the teleport delegate to take futher care of this.. WHY DARIO?!
                                                if (!User.IsBot && User != null && User.GetClient() != null && User.GetClient().GetHabbo() != null && User.GetClient().GetMessageHandler() != null)
                                                {
                                                    User.GetClient().GetHabbo().IsTeleporting = true;
                                                    User.GetClient().GetHabbo().TeleportingRoomID = RoomId;
                                                    User.GetClient().GetHabbo().TeleporterId = TeleId;
                                                    User.GetClient().GetMessageHandler().PrepareRoomForUser(RoomId, "");
                                                }
                                            }

                                            // We're done with this tele. We have another one to bother.
                                            InteractingUser = 0;
                                        }
                                        else
                                        {
                                            // This tele is not linked, so let's gtfo.
                                            User.UnlockWalking();
                                            InteractingUser = 0;
                                            User.MoveTo(SquareInFront);
                                        }
                                    }

                                    // Is he in front of the tele?
                                    else if (User.Coordinate == SquareInFront)
                                    {
                                        User.AllowOverride = true;
                                        // Open the door
                                        keepDoorOpen = true;

                                        // Lock his walking. We're taking control over him. Allow overriding so he can get in the tele.
                                        if (User.IsWalking && (User.GoalX != mX || User.GoalY != mY))
                                        {
                                            User.ClearMovement(true);
                                        }

                                        User.CanWalk = false;
                                        User.AllowOverride = true;

                                        // Move into the tele
                                        User.MoveTo(Coordinate.X, Coordinate.Y);
                                    }
                                    // Not even near, do nothing and move on for the next user.
                                    else
                                    {
                                        InteractingUser = 0;
                                    }
                                }
                                else
                                {
                                    // Invalid user, do nothing and move on for the next user. 
                                    InteractingUser = 0;
                                }
                            }

                            // Do we have a secondary user that wants to get out of the tele?
                            if (InteractingUser2 > 0)
                            {
                                User2 = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser2);

                                // Is this user okay?
                                if (User2 != null)
                                {
                                    // If so, open the door, unlock the user's walking, and try to push him out in the right direction. We're done with him!
                                    keepDoorOpen = true;
                                    User2.UnlockWalking();
                                    User2.MoveTo(SquareInFront);
                                }

                                // This is a one time thing, whether the user's valid or not.
                                InteractingUser2 = 0;
                            }

                            // Set the new item state, by priority
                            if (keepDoorOpen)
                            {
                                if (ExtraData != "1")
                                {
                                    ExtraData = "1";
                                    UpdateState(false, true);
                                }
                            }
                            else if (showTeleEffect)
                            {
                                if (ExtraData != "2")
                                {
                                    ExtraData = "2";
                                    UpdateState(false, true);
                                }
                            }
                            else
                            {
                                if (ExtraData != "0")
                                {
                                    ExtraData = "0";
                                    UpdateState(false, true);
                                }
                            }

                            // We're constantly going!
                            ReqUpdate(1, false);

                            break;
                        }
                    case InteractionType.teleport:
                        {
                            User = null;
                            User2 = null;

                            var keepDoorOpen = false;
                            var showTeleEffect = false;

                            // Do we have a primary user that wants to go somewhere?
                            if (InteractingUser > 0)
                            {
                                User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);

                                // Is this user okay?
                                if (User != null)
                                {
                                    // Is he in the tele?
                                    if (User.Coordinate == Coordinate)
                                    {
                                        //Remove the user from the square
                                        User.AllowOverride = false;

                                        if (teleLink == 0)
                                        {
                                            // Console.WriteLine("[ERROR]: No linked teleport: " + Id + ";" + ExtraData);
                                            return;
                                        }

                                        if (TeleHandler.IsTeleLinked(teleLink, mRoom))
                                        {
                                            showTeleEffect = true;

                                            // Woop! No more delay.
                                            var TeleId = teleLink;
                                            var RoomId = TeleHandler.GetTeleRoomId(TeleId, mRoom);

                                            // Do we need to tele to the same room or gtf to another?
                                            if (RoomId == this.RoomId)
                                            {
                                                var Item = GetRoom().GetRoomItemHandler().GetItem(TeleId);

                                                if (Item == null)
                                                {
                                                    User.UnlockWalking();
                                                }
                                                else
                                                {
                                                    // Set pos
                                                    User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                                                    User.SetRot(Item.Rot, false);

                                                    // Force tele effect update (dirty)
                                                    Item.ExtraData = "2";
                                                    Item.UpdateState(false, true);

                                                    // Set secondary interacting user
                                                    Item.InteractingUser2 = InteractingUser;
                                                }
                                            }
                                            else
                                            {
                                                // Let's run the teleport delegate to take futher care of this.. WHY DARIO?!
                                                if (!User.IsBot && User != null && User.GetClient() != null && User.GetClient().GetHabbo() != null && User.GetClient().GetMessageHandler() != null)
                                                {
                                                    User.GetClient().GetHabbo().IsTeleporting = true;
                                                    User.GetClient().GetHabbo().TeleportingRoomID = RoomId;
                                                    User.GetClient().GetHabbo().TeleporterId = TeleId;
                                                    User.GetClient().GetMessageHandler().PrepareRoomForUser(RoomId, "");
                                                }
                                            }

                                            // We're done with this tele. We have another one to bother.
                                            InteractingUser = 0;
                                        }
                                        else
                                        {
                                            // This tele is not linked, so let's gtfo.
                                            User.UnlockWalking();
                                            InteractingUser = 0;
                                            User.MoveTo(SquareInFront);
                                        }
                                    }

                                    // Is he in front of the tele?
                                    else if (User.Coordinate == SquareInFront)
                                    {
                                        User.AllowOverride = true;
                                        // Open the door
                                        keepDoorOpen = true;

                                        // Lock his walking. We're taking control over him. Allow overriding so he can get in the tele.
                                        if (User.IsWalking && (User.GoalX != mX || User.GoalY != mY))
                                        {
                                            User.ClearMovement(true);
                                        }

                                        User.CanWalk = false;
                                        User.AllowOverride = true;

                                        // Move into the tele
                                        User.MoveTo(Coordinate.X, Coordinate.Y);
                                    }
                                    // Not even near, do nothing and move on for the next user.
                                    else
                                    {
                                        InteractingUser = 0;
                                    }
                                }
                                else
                                {
                                    // Invalid user, do nothing and move on for the next user. 
                                    InteractingUser = 0;
                                }
                            }

                            // Do we have a secondary user that wants to get out of the tele?
                            if (InteractingUser2 > 0)
                            {
                                User2 = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser2);

                                // Is this user okay?
                                if (User2 != null)
                                {
                                    // If so, open the door, unlock the user's walking, and try to push him out in the right direction. We're done with him!
                                    keepDoorOpen = true;
                                    User2.UnlockWalking();
                                    User2.MoveTo(SquareInFront);
                                }

                                // This is a one time thing, whether the user's valid or not.
                                InteractingUser2 = 0;
                            }

                            // Set the new item state, by priority
                            if (keepDoorOpen)
                            {
                                if (ExtraData != "1")
                                {
                                    ExtraData = "1";
                                    UpdateState(false, true);
                                }
                            }
                            else if (showTeleEffect)
                            {
                                if (ExtraData != "2")
                                {
                                    ExtraData = "2";
                                    UpdateState(false, true);
                                }
                            }
                            else
                            {
                                if (ExtraData != "0")
                                {
                                    ExtraData = "0";
                                    UpdateState(false, true);
                                }
                            }

                            // We're constantly going!
                            ReqUpdate(1, false);

                            break;
                        }
                    case InteractionType.bottle:

                        ExtraData = new Random().Next(0, 7).ToString();
                        UpdateState();
                        break;

                    case InteractionType.dice:

                        User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);

                        if (User == null)
                            return;

                        if (User.GetClient().GetHabbo().DiceNumber > 0)
                        {
                            ExtraData = User.GetClient().GetHabbo().DiceNumber.ToString();
                            User.GetClient().GetHabbo().DiceNumber = 0;
                        }
                        else
                        {
                            ExtraData = new Random().Next(1, 7).ToString();
                        }

                        UpdateState();
                        OnTrigger(GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser));
                        InteractingUser = 0;

                        break;

                    case InteractionType.habbowheel:

                        ExtraData = new Random().Next(1, 10).ToString();
                        UpdateState();
                        break;

                    case InteractionType.loveshuffler:

                        if (ExtraData == "0")
                        {
                            ExtraData = new Random().Next(1, 4).ToString();
                            ReqUpdate(20, false);
                        }
                        else if (ExtraData != "-1")
                        {
                            ExtraData = "-1";
                        }

                        UpdateState(false, true);
                        break;

                    case InteractionType.alert:

                        if (ExtraData == "1")
                        {
                            ExtraData = "0";
                            UpdateState(false, true);
                        }

                        break;

                    case InteractionType.vendingmachine:

                        if (ExtraData == "1")
                        {
                            User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);

                            if (User != null)
                            {
                                User.UnlockWalking();

                                var randomDrink = GetBaseItem().VendingIds[new Random().Next(0, (GetBaseItem().VendingIds.Count - 1))];
                                User.CarryItem(randomDrink);
                            }

                            InteractingUser = 0;
                            ExtraData = "0";

                            UpdateState(false, true);
                        }

                        break;

                    case InteractionType.scoreboard:
                        {
                            if (string.IsNullOrEmpty(ExtraData))
                                break;

                            uint seconds = 0;
                            uint.TryParse(ExtraData, out seconds);

                            if (seconds > 0)
                            {
                                if (interactionCountHelper == 1)
                                {
                                    seconds--;
                                    interactionCountHelper = 0;
                                    if (!ScoreboardIsPaused)
                                    {
                                        ExtraData = seconds.ToString();
                                        UpdateState();
                                    }
                                }
                                else
                                {
                                    interactionCountHelper++;
                                }

                                UpdateCounter = 1;
                            }
                            else
                            {
                                UpdateCounter = 0;
                                if (GetRoom().GetGameManager() != null && GetRoom().GetGameManager().IsSameChronometer(this))
                                    GetRoom().GetGameManager().EndGame();
                            }

                            break;
                        }

                    case InteractionType.banzaitele:
                        {
                            ExtraData = string.Empty;
                            UpdateState();
                            break;
                        }

                    case InteractionType.banzaifloor:
                        {
                            if (value == 3)
                            {
                                if (interactionCountHelper == 1)
                                {
                                    interactionCountHelper = 0;

                                    switch (team)
                                    {
                                        case Team.blue:
                                            {
                                                ExtraData = "11";
                                                break;
                                            }

                                        case Team.green:
                                            {
                                                ExtraData = "8";
                                                break;
                                            }

                                        case Team.red:
                                            {
                                                ExtraData = "5";
                                                break;
                                            }

                                        case Team.yellow:
                                            {
                                                ExtraData = "14";
                                                break;
                                            }
                                    }
                                }
                                else
                                {
                                    ExtraData = "";
                                    interactionCountHelper++;
                                }

                                UpdateState();

                                interactionCount++;

                                if (interactionCount < 16)
                                {
                                    UpdateCounter = 1;
                                }
                                else
                                    UpdateCounter = 0;
                            }
                            break;
                        }

                    case InteractionType.banzaipuck:
                        {

                            if (interactionCount > 4)
                            {
                                interactionCount++;
                                UpdateCounter = 1;
                            }
                            else
                            {
                                interactionCount = 0;
                                UpdateCounter = 0;
                            }

                            break;
                        }

                    case InteractionType.freezetile:
                        {
                            if (InteractingUser > 0)
                            {
                                ExtraData = "11000";
                                UpdateState(false, true);

                                GetRoom().GetGameManager().GetFreeze().onFreezeTiles(this, freezePowerUp, InteractingUser);
                                InteractingUser = 0;
                                interactionCountHelper = 0;
                            }
                            break;
                        }

                    case InteractionType.vikinghouse:
                        {
                            if (VikingHouseBurning)
                            {
                                int extradating = 1;
                                int.TryParse(ExtraData, out extradating);
                                extradating++;
                                ExtraData = extradating.ToString();
                                UpdateState();

                                if (extradating > 4)
                                    VikingHouseBurning = false;

                                ReqUpdate(40, true);
                            }

                            break;
                        }

                    case InteractionType.dalia:
                        {
                            ExtraData = "0";
                            UpdateState();
                            break;
                        }
                }
            }
        }
        internal void updateInteractionCount(RoomItem theItem, bool extradataOriginal = false)
        {
            if (extradataOriginal)
            {
                theItem.originalExtraData = theItem.ExtraData;

                theItem.ExtraData = "1";
                theItem.UpdateState();

                theItem.ExtraData = theItem.originalExtraData;
                return;
            }

            int Modes = (theItem.GetBaseItem().Modes - 1);

            if (Modes < 0)
            {
                Modes = 0;
            }

            if (Modes == 0)
            {
                return;
            }

            var currentMode = 0;
            var newMode = 0;

            try
            {
                currentMode = int.Parse(theItem.ExtraData);
            }
            catch { }

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
        
            theItem.ExtraData = newMode.ToString();
            theItem.UpdateState();
            
        }

        internal void ReqUpdate(int Cycles, bool setUpdate)
        {
            UpdateCounter = Cycles;
            if (setUpdate)
                UpdateNeeded = true;
        }

        internal void UpdateState()
        {
            UpdateState(true, true);
        }

        internal void UpdateState(bool inDb, bool inRoom)
        {
            if (GetRoom() == null || GetBaseItem() == null)
                return;

            if (inDb)
            {
                GetRoom().GetRoomItemHandler().UpdateItem(this);
            }

            if (inRoom)
            {
                var Message = new ServerMessage(0);

                if (IsFloorItem)
                {
                    Message.Init(Outgoing.UpdateFloorItemExtraData);
                    Message.AppendString(Id.ToString());
                    if (GetBaseItem().InteractionType == InteractionType.changeBackgrounds && ExtraData.Contains(","))
                    {
                        Message.AppendInt32(5);
                        Message.AppendInt32(4);
                        Message.AppendInt32(ExtraData.StartsWith("on") ? 1 : 0);
                        Message.AppendInt32(int.Parse(ExtraData.Split(',')[1]));
                        Message.AppendInt32(int.Parse(ExtraData.Split(',')[2]));
                        Message.AppendInt32(int.Parse(ExtraData.Split(',')[3]));
                    }
                    else if (GetBaseItem().InteractionType == InteractionType.piñata || GetBaseItem().InteractionType == InteractionType.dalia)
                    {
                        Message.AppendInt32(7); // ??
                        if (ExtraData.Length <= 0)
                        {
                            Message.AppendString("0"); // 6: normal || 8: break
                            Message.AppendInt32(0); // golpes recibidos
                            Message.AppendInt32(GetBaseItem().VendingIds[0]); // total de golpes
                        }
                        else
                        {
                            Message.AppendString(ExtraData); // 6: normal || 8: break
                            Message.AppendInt32(int.Parse(ExtraData)); // golpes recibidos
                            Message.AppendInt32(GetBaseItem().VendingIds[0]); // total de golpes
                        }
                    }
                    else if (GetBaseItem().InteractionType == InteractionType.balloon15)
                    {
                        Message.AppendInt32(7); // ??
                        if (ExtraData == "0")
                        {
                            Message.AppendString("0");
                            Message.AppendInt32(0); // golpes recibidos
                            Message.AppendInt32(1); // total de golpes
                        }
                        else
                        {
                            Message.AppendString("2");
                            Message.AppendInt32(1); // golpes recibidos
                            Message.AppendInt32(1); // total de golpes
                        }
                    }
                    else if (GetBaseItem().InteractionType == InteractionType.wiredClassification)
                    {
                        Message.AppendInt32(6);
                        Message.AppendString(wiredPuntuation == null ? "0" : wiredPuntuation.EnableValue); // 1 = show; 0 = hide
                        Message.AppendInt32(wiredPuntuation == null ? 0 : wiredPuntuation.getMainInt());
                        Message.AppendInt32(wiredPuntuation == null ? 0 : wiredPuntuation.getSecondInt());
                        if (wiredPuntuation == null)
                        {
                            Message.AppendInt32(0);
                        }
                        else
                        {
                            Message.AppendInt32(wiredPuntuation.puntuationRows.Count);
                            foreach (PuntuationRow ranks in wiredPuntuation.puntuationRows)
                            {
                                Message.AppendUInt(ranks.puntuation);
                                Message.AppendInt32(1);
                                Message.AppendString(ranks.names);
                            }
                        }
                    }
                    else if (GetBaseItem().InteractionType == InteractionType.gift)
                    {
                        uint PurchaserId = uint.Parse(ExtraData.Split(';')[0]);
                        Habbo Purchaser = UsersCache.getHabboCache(PurchaserId);
                        Message.AppendInt32(1);
                        Message.AppendInt32(6);
                        Message.AppendString("MESSAGE");
                        Message.AppendString(ChatCommandHandler.MergeParams(ExtraData.Split(';'), 2, ";"));
                        Message.AppendString("EXTRA_PARAM");
                        Message.AppendString("");
                        Message.AppendString("PURCHASER_FIGURE");
                        Message.AppendString(Purchaser != null ? Purchaser.Look : "");
                        Message.AppendString("PURCHASER_NAME");
                        Message.AppendString(Purchaser != null ? Purchaser.Username : "");
                        Message.AppendString("PRODUCT_CODE");
                        Message.AppendString(OtanixEnvironment.GetGame().GetItemManager().GetItemNameByGiftId(Id));
                        Message.AppendString("state");
                        Message.AppendString(MagicRemove ? "1" : "0");
                    }
                    else
                    {
                        Message.AppendInt32(0);
                        Message.AppendString((GetBaseItem().InteractionType == InteractionType.changeBackgrounds) ? "" : ExtraData);
                    }
                }
                else
                {
                    Message.Init(Outgoing.UpdateWallItemOnRoom);
                    Serialize(Message);
                }

                GetRoom().SendMessage(Message);
            }
        }

        internal void Serialize(ServerMessage Message)
        {
            if (GetBaseItem() == null)
                return;

            if (IsFloorItem)
            {
                Message.AppendUInt(Id);
                Message.AppendInt32(GetBaseItem().SpriteId);
                Message.AppendInt32(mX);
                Message.AppendInt32(mY);
                Message.AppendInt32(Rot);
                Message.AppendString(String.Format("{0:0.00}", TextHandling.GetString(mZ))); // altura a la que se encuentra
                Message.AppendString(String.Format("{0:0.00}", TextHandling.GetString(mZ))); // altura del furni
                if (GetBaseItem().InteractionType == InteractionType.gift)
                {
                    try
                    {
                        Message.AppendInt32(int.Parse(ExtraData.Split(';')[1])); // giftView

                        uint PurchaserId = uint.Parse(ExtraData.Split(';')[0]);
                        Habbo Purchaser = UsersCache.getHabboCache(PurchaserId);
                        Message.AppendInt32(1);
                        Message.AppendInt32(6);
                        Message.AppendString("MESSAGE");
                        Message.AppendString(ChatCommandHandler.MergeParams(ExtraData.Split(';'), 2, ";"));
                        Message.AppendString("EXTRA_PARAM");
                        Message.AppendString("");
                        Message.AppendString("PURCHASER_FIGURE");
                        Message.AppendString(Purchaser != null ? Purchaser.Look : "");
                        Message.AppendString("PURCHASER_NAME");
                        Message.AppendString(Purchaser != null ? Purchaser.Username : "");
                        Message.AppendString("PRODUCT_CODE");
                        Message.AppendString(OtanixEnvironment.GetGame().GetItemManager().GetItemNameByGiftId(Id));
                        Message.AppendString("state");
                        Message.AppendString(MagicRemove ? "1" : "0");
                    }
                    catch { Logging.LogException("Error al serializar un regalo: (" + Id + ") " + ExtraData); return; }
                }
                else if (GetBaseItem().InteractionType == InteractionType.piñata || GetBaseItem().InteractionType == InteractionType.dalia)
                {
                    Message.AppendInt32(0); // ??
                    Message.AppendInt32(7); // ??
                    Message.AppendString(ExtraData); // ??
                    if (ExtraData.Length <= 0)
                    {
                        Message.AppendInt32(0); // golpes recibidos
                        Message.AppendInt32(GetBaseItem().VendingIds[0]); // total de golpes
                    }
                    else
                    {
                        Message.AppendInt32(int.Parse(ExtraData)); // golpes recibidos
                        Message.AppendInt32(GetBaseItem().VendingIds[0]); // total de golpes
                    }
                }
                else if (GetBaseItem().InteractionType == InteractionType.balloon15)
                {
                    Message.AppendInt32(0); // ??
                    Message.AppendInt32(7); // ??
                    Message.AppendString("0"); // ??
                    Message.AppendInt32(0); // golpes recibidos
                    Message.AppendInt32(1); // total de golpes
                }
                else if (GetBaseItem().InteractionType == InteractionType.userslock)
                {
                    Message.AppendInt32(0);
                    Message.AppendInt32(2);
                    if (ExtraData.Length <= 0 || !ExtraData.Contains(";") || ExtraData.Split(';').Length < 5)
                    {
                        Message.AppendInt32(0);
                    }
                    else
                    {
                        string[] Data = ExtraData.Split(';');
                        Message.AppendInt32(Data.Length);
                        foreach (string datak in Data)
                        {
                            Message.AppendString(datak);
                        }
                    }
                }
                else if (GetBaseItem().InteractionType == InteractionType.maniqui)
                {
                    Message.AppendInt32(0);
                    Message.AppendInt32(1);

                    if (ExtraData.Length <= 0 || !ExtraData.Contains(";") || ExtraData.Split(';').Length < 3)
                    {
                        Message.AppendInt32(3); // Coun Of Values
                        Message.AppendString("GENDER");
                        Message.AppendString("m");
                        Message.AppendString("FIGURE");
                        Message.AppendString("");
                        Message.AppendString("OUTFIT_NAME");
                        Message.AppendString("");
                    }
                    else
                    {
                        var Extradatas = ExtraData.Split(';');

                        Message.AppendInt32(3); // Coun Of Values
                        Message.AppendString("GENDER");
                        Message.AppendString(Extradatas[0]);
                        Message.AppendString("FIGURE");
                        Message.AppendString(Extradatas[1]);
                        Message.AppendString("OUTFIT_NAME");
                        Message.AppendString(Extradatas[2]);
                    }

                }
                else if (GetBaseItem().InteractionType == InteractionType.badge_display)
                {
                    Message.AppendInt32(0);
                    Message.AppendInt32(2);
                    Message.AppendInt32(4); // Count of Values
                    Message.AppendString("0");
                    if (ExtraData.Split(';').Length == 3)
                    {
                        Message.AppendString(ExtraData.Split(';')[0]); // BadgeCode
                        Message.AppendString(ExtraData.Split(';')[1]); // OwnerName
                        Message.AppendString(ExtraData.Split(';')[2]); // DateBuyed
                    }
                    else
                    {
                        Message.AppendString(ExtraData); // BadgeCode
                        Message.AppendString(""); // OwnerName
                        Message.AppendString(""); // DateBuyed
                    }
                }
                else if (GetBaseItem().InteractionType == InteractionType.changeBackgrounds && ExtraData.Contains(","))
                {
                    Message.AppendInt32(0);
                    Message.AppendInt32(5);
                    Message.AppendInt32(4);
                    Message.AppendInt32(ExtraData.StartsWith("on") ? 1 : 0);
                    Message.AppendInt32(int.Parse(ExtraData.Split(',')[1]));
                    Message.AppendInt32(int.Parse(ExtraData.Split(',')[2]));
                    Message.AppendInt32(int.Parse(ExtraData.Split(',')[3]));
                }
                else if (GetBaseItem().InteractionType == InteractionType.seed)
                {
                    Message.AppendInt32(0);
                    Message.AppendInt32(1);
                    Message.AppendInt32(1);
                    Message.AppendString("rarity");
                    Message.AppendString(ExtraData);
                }
                else if (GetBaseItem().IsGroupItem)
                {
                    Message.AppendInt32(0);
                    try
                    {
                        uint GroupID = uint.Parse(GroupData.Split(';')[1]);

                        var Group = OtanixEnvironment.GetGame().GetGroup().LoadGroup(GroupID);
                        Message.AppendInt32(2);
                        Message.AppendInt32(5);
                        Message.AppendString(ExtraData);
                        Message.AppendString(GroupID.ToString());
                        Message.AppendString(Group == null ? "" : Group.GroupImage);
                        Message.AppendString(GroupData.Split(';')[2]);
                        Message.AppendString(GroupData.Split(';')[3]);
                    }
                    catch
                    {
                        Message.AppendInt32(2);
                        Message.AppendInt32(0);
                    }
                }
                else if (GetBaseItem().LimitedStack > 0)
                {
                    Message.AppendInt32(0);
                    Message.AppendString("");
                    Message.AppendBoolean(true);
                    Message.AppendBoolean(false);
                    Message.AppendString(ExtraData);
                    Message.AppendInt32(LimitedValue);
                    Message.AppendInt32(GetBaseItem().LimitedStack);
                }
                else if (GetBaseItem().InteractionType == InteractionType.ads_mpu || GetBaseItem().InteractionType == InteractionType.fxprovider)
                {
                    Message.AppendInt32(0);
                    Message.AppendInt32(1);
                    if (ExtraData.Length > 0 && ExtraData.Contains(";"))
                    {
                        Message.AppendInt32(ExtraData.Split(';').Length / 2);
                        foreach (var Data in ExtraData.Split(';'))
                        {
                            Message.AppendString(Data);
                        }
                    }
                    else
                    {
                        Message.AppendInt32(0);
                    }
                }
                else if (GetBaseItem().InteractionType == InteractionType.yttv)
                {
                    try
                    {
                        Message.AppendInt32(0);
                        Message.AppendInt32(1);
                        Message.AppendInt32(1);
                        Message.AppendString("THUMBNAIL_URL");
                        Message.AppendString(tvImage != null ? tvImage : ""); // Message.AppendString(tvImage);
                    }
                    catch // fixer TV
                    {
                        GetRoom().GetRoomItemHandler().RemoveFurniture(null, this);
                    }
                }
                else if (GetBaseItem().InteractionType == InteractionType.wiredClassification)
                {
                    Message.AppendInt32(0);
                    Message.AppendInt32(6);
                    Message.AppendString(ExtraData.Contains(";") ? ExtraData.Split(';')[0] : "0");
                    Message.AppendInt32(wiredPuntuation == null ? 0 : wiredPuntuation.getMainInt());
                    Message.AppendInt32(wiredPuntuation == null ? 0 : wiredPuntuation.getSecondInt());
                    if (wiredPuntuation == null)
                    {
                        Message.AppendInt32(0);
                    }
                    else
                    {
                        Message.AppendInt32(wiredPuntuation.puntuationRows.Count);
                        foreach (PuntuationRow ranks in wiredPuntuation.puntuationRows)
                        {
                            Message.AppendUInt(ranks.puntuation);
                            Message.AppendInt32(1);
                            Message.AppendString(ranks.names);
                        }
                    }
                }
                else
                {
                    Message.AppendInt32(0);
                    Message.AppendInt32(0);
                    if (GetBaseItem().InteractionType == InteractionType.changeBackgrounds)
                    {
                        Message.AppendString(String.Empty);
                    }
                    else
                    {
                        Message.AppendString(ExtraData);
                    }
                }
                Message.AppendInt32(-1);
                Message.AppendInt32(GetBaseItem().Modes > 1 || GetBaseItem().InteractionType != InteractionType.none ? 1 : 0); // Botón Usar.
                Message.AppendUInt(OwnerId);
            }
            else if (IsWallItem)
            {
                Message.AppendString(Id + String.Empty);
                Message.AppendInt32(GetBaseItem().SpriteId);
                if (wallCoord == null)
                    Message.AppendString("");
                else
                    Message.AppendString(wallCoord.ToString());
                switch (GetBaseItem().InteractionType)
                {
                    case InteractionType.postit:
                        Message.AppendString(ExtraData.Split(' ')[0]);
                        break;
                    case InteractionType.dimmer:
                        var moodData = mRoom.MoodlightData;
                        if (moodData == null && mRoom.GetRoomItemHandler().GetItem(moodData.ItemId) == null)
                        {
                            mRoom.MoodlightData = new MoodlightData(Id);
                            ExtraData = moodData.GenerateExtraData();
                        }
                        
                        Message.AppendString(ExtraData);
                        break;
                    default:
                        Message.AppendString(ExtraData);
                        break;
                }
                Message.AppendInt32(-1); // time to end 
                Message.AppendInt32(GetBaseItem().Modes > 1 || GetBaseItem().InteractionType != InteractionType.none ? 1 : 0); // Botón Usar.
                Message.AppendUInt(OwnerId);
            }
        }

        internal Item GetBaseItem()
        {
            return OtanixEnvironment.GetGame().GetItemManager().GetItem(BaseItem);
        }

        internal Room GetRoom()
        {
            if (mRoom == null)
                mRoom = OtanixEnvironment.GetGame().GetRoomManager().GetRoom(RoomId); //Todo: rub my penis

            return mRoom;
        }

        internal void BotWalksOnFurni(RoomUser user)
        {
            if (OnBotWalksOnFurni != null)
                OnBotWalksOnFurni(user, new UserWalksOnArgs(user));
        }

        internal void UserWalksOnFurni(RoomUser user)
        {
            if (OnUserWalksOnFurni != null)
                OnUserWalksOnFurni(this, new UserWalksOnArgs(user));
        }

        internal void UserWalksOffFurni(RoomUser user)
        {
            if (OnUserWalksOffFurni != null)
                OnUserWalksOffFurni(this, new UserWalksOnArgs(user));
        }
    }
}
