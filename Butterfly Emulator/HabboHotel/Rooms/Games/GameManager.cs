using Butterfly.HabboHotel.Items;
using Butterfly.Messages;
using HabboEvents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterfly.HabboHotel.Rooms.Games
{
    class GameManager
    {
        private Room Room;
        private int[] teamPoints;
        private List<RoomItem>[] teamItems;
        private RoomItem Chronometer;

        private bool GameStarted;

        private Banzai BattleBanzai;
        private Freeze Freeze;

        public event TeamScoreChangedDelegate OnScoreChanged;
        public event RoomEventDelegate OnGameStart;
        public event RoomEventDelegate OnGameEnd;

        public GameManager(Room room)
        {
            Room = room;
            teamPoints = new int[5];
            teamItems = new List<RoomItem>[5];
            for (int i = 1; i < 5; i++)
            {
                teamItems[i] = new List<RoomItem>();
            }

            GameStarted = false;

            BattleBanzai = new Banzai(room);
            Freeze = new Freeze();
        }

        public int GetTeamPoints(int Team)
        {
            return teamPoints[Team];
        }

        public Banzai GetBanzai()
        {
            return BattleBanzai;
        }

        public Freeze GetFreeze()
        {
            return Freeze;
        }

        public bool IsSameChronometer(RoomItem Item)
        {
            return Chronometer == Item;
        }
        
        public bool IsGameStarted()
        {
            return GameStarted;
        }

        public bool IsGamePaused()
        {
            if (Chronometer == null)
                return false;

            return Chronometer.ScoreboardIsPaused;
        }

        public void UpdatePauseGame()
        {
            Chronometer.ScoreboardIsPaused = !Chronometer.ScoreboardIsPaused;
        }

        public void AddTeamItem(RoomItem Item, Team Team)
        {
            int TeamId = (int)Team;
            if (!teamItems[TeamId].Contains(Item))
                teamItems[TeamId].Add(Item);
        }

        public void RemoveTeamItem(RoomItem Item, Team Team)
        {
            int TeamId = (int)Team;
            if (teamItems[TeamId].Contains(Item))
                teamItems[TeamId].Remove(Item);
        }

        public List<RoomItem> GetTeamItems(Team Team)
        {
            int TeamId = (int)Team;
            return teamItems[TeamId];
        }

        public void SetChronometer(RoomItem Item)
        {
            Chronometer = Item;
            StartGame();
        }

        private void StartGame()
        {
            ModifyGates(0);
            ResetScores();
            StartGameEvent();

            BattleBanzai.PrepareGame();
            Freeze.PrepareGame(Room);

            Freeze.UpdateExitTile("1");

            if (Freeze.FreezeEnable())
                CalculateFreezePoints();

            GameStarted = true;
        }

        public void EndGame()
        {
            Room.GetWiredHandler().RefreshClassifications();
            ModifyGates(1);
            StopGameEvent();

            Team winners = getWinningTeam();

            GetBanzai().EndGame(winners, Room);
            GetFreeze().EndGame(winners, Room);

            Freeze.UpdateExitTile("0");

            GameStarted = false;
            Chronometer.ScoreboardIsPaused = false;
        }

        /// <summary>
        /// Devuelve el equipo que más puntos ha conseguido
        /// </summary>
        /// <returns>Devuelve el equipo con más puntos. En caso de empate, devuelve none.</returns>
        public Team getWinningTeam()
        {
            int winning = 0;
            int highestScore = 0;

            for(int i = 1; i < 5; i++)
            {
                if(teamPoints[i] > highestScore)
                {
                    highestScore = teamPoints[i];
                    winning = i;
                }
                else if (teamPoints[i] == highestScore)
                {
                    winning = 0;
                }
            }

            return (Team)winning;
        }

        /// <summary>
        /// Lock o Unlock los gates de la sala.
        /// </summary>
        /// <param name="State">State = 0 (Lock). State = 1 (Unlock)</param>
        public void ModifyGates(int State)
        {
            for (int i = 1; i < 5; i++)
            {
                List<RoomItem> Items = teamItems[i];
                foreach (RoomItem Item in Items)
                {
                    if(IsGate(Item.GetBaseItem().InteractionType))
                    {
                        List<RoomUser> usersInGate = Room.GetGameMap().GetRoomUsers(new Point(Item.GetX, Item.GetY));
                        foreach(RoomUser user in usersInGate)
                        {
                            user.SqState = (byte)State;
                        }

                        Room.GetGameMap().GameMap[Item.GetX, Item.GetY] = (byte)State;
                    }
                }
            }
        }

        /// <summary>
        /// Añade o resta puntos a un equipo.
        /// </summary>
        /// <param name="Team">Color del equipo.</param>
        /// <param name="Points">Puntos que va a dar/quitar.</param>
        public void AddPointsToTeam(Team Team, int Points, RoomUser User)
        {
            int TeamId = (int)Team;
            teamPoints[TeamId] += Points;

            if (teamPoints[TeamId] < 0)
                teamPoints[TeamId] = 0;

            List<RoomItem> Items = teamItems[TeamId];
            foreach (RoomItem Item in Items)
            {
                if (IsScoreboard(Item.GetBaseItem().InteractionType))
                {
                    Item.ExtraData = teamPoints[TeamId].ToString();
                    Item.UpdateState();
                }
            }

            if (OnScoreChanged != null)
                OnScoreChanged(null, new TeamScoreChangedArgs(teamPoints[TeamId], Team, User));
        }

        /// <summary>
        /// Resetea los contadores de puntos a 0.
        /// </summary>
        private void ResetScores()
        {
            for (int i = 1; i < 5; i++)
            {
                List<RoomItem> Items = teamItems[i];
                foreach (RoomItem Item in Items)
                {
                    if (IsScoreboard(Item.GetBaseItem().InteractionType))
                    {
                        Item.ExtraData = "0";
                        Item.UpdateState();

                        teamPoints[i] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Calcula los puntos que se asignarán al jugar una partida de Freeze.
        /// </summary>
        private void CalculateFreezePoints()
        {
            foreach (RoomUser User in Room.GetRoomUserManager().UserList.Values)
            {
                if (User.team != Team.none)
                {
                    AddPointsToTeam(User.team, 40, null);

                    User.banzaiPowerUp = FreezePowerUp.None;
                    User.FreezeLives = 3;
                    User.shieldActive = false;
                    User.shieldCounter = 11;

                    var message = new ServerMessage(Outgoing.UpdateFreezeLives);
                    message.AppendInt32(User.InternalRoomID);
                    message.AppendInt32(User.FreezeLives);
                    User.GetClient().SendMessage(message);
                }
            }
        }

        /// <summary>
        /// Comprueba si el item seleccionado es un Gate.
        /// </summary>
        /// <param name="Interaction">Interacción del Item.</param>
        /// <returns></returns>
        public bool IsGate(InteractionType Interaction)
        {
            switch (Interaction)
            {
                case InteractionType.freezebluegate:
                case InteractionType.freezegreengate:
                case InteractionType.freezeredgate:
                case InteractionType.freezeyellowgate:
                case InteractionType.banzaigateblue:
                case InteractionType.banzaigategreen:
                case InteractionType.banzaigatered:
                case InteractionType.banzaigateyellow:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Comprueba si el item seleccionado es un Scoreboard.
        /// </summary>
        /// <param name="Interaction">Interacción del Item.</param>
        /// <returns></returns>
        private bool IsScoreboard(InteractionType Interaction)
        {
            switch(Interaction)
            {
                case InteractionType.banzaiscoreblue:
                case InteractionType.banzaiscoregreen:
                case InteractionType.banzaiscorered:
                case InteractionType.banzaiscoreyellow:
                case InteractionType.freezebluecounter:
                case InteractionType.freezegreencounter:
                case InteractionType.freezeredcounter:
                case InteractionType.freezeyellowcounter:
                case InteractionType.footballcounterblue:
                case InteractionType.footballcountergreen:
                case InteractionType.footballcounterred:
                case InteractionType.footballcounteryellow:
                    return true;
            }
            return false;
        }

        private void StopGameEvent()
        {
            if (OnGameEnd != null)
                OnGameEnd(null, null);

            Room.lastTimerReset = DateTime.Now;
        }

        private void StartGameEvent()
        {
            if (OnGameStart != null)
                OnGameStart(null, null);
        }

        public void Destroy()
        {
            Room = null;

            Array.Clear(teamPoints, 0, 5);
            Array.Clear(teamItems, 0, 5);

            BattleBanzai.Destroy();
            Freeze.Destroy();

            OnGameStart = null;
            OnGameEnd = null;
        }
    }
}
