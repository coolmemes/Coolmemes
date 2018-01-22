using System;
using System.Linq;
using Butterfly.HabboHotel.Items;
using System.Collections.Concurrent;
using ButterStorm;
using Butterfly.Scripts;
using System.Collections.Generic;

namespace Butterfly.HabboHotel.Rooms
{
    class GameItemHandler
    {
        private ConcurrentDictionary<uint, RoomItem> banzaiTeleports;
        private ConcurrentDictionary<uint, RoomItem> banzaiPyramids;
        private Room room;

        public GameItemHandler(Room _room)
        {
            room = _room;
            banzaiPyramids = new ConcurrentDictionary<uint, RoomItem>();
            banzaiTeleports = new ConcurrentDictionary<uint, RoomItem>();
        }

        internal void OnCycle()
        {
            CyclePyramids();
        }

        private void CyclePyramids()
        {
            var rnd = new Random();

            foreach (var item in banzaiPyramids.Values)
            {
                if (item.interactionCountHelper == 0 && item.ExtraData == "1")
                {
                    room.GetGameMap().RemoveFromMap(item, false);
                    item.interactionCountHelper = 1;
                }

                if (string.IsNullOrEmpty(item.ExtraData))
                    item.ExtraData = "0";

                var randomNumber = rnd.Next(0, 30);
                if (randomNumber > 26)
                {
                    if (item.ExtraData == "0")
                    {
                        item.ExtraData = "1";
                        item.UpdateState();
                        room.GetGameMap().RemoveFromMap(item, false);
                    }
                    else
                    {
                        if (room.GetGameMap().itemCanBePlacedHere(item.GetX, item.GetY))
                        {
                            item.ExtraData = "0";
                            item.UpdateState();
                            room.GetGameMap().AddItemToMap(item, false);
                        }
                    }
                }
            }
        }

        internal void AddPyramid(RoomItem item, uint itemID)
        {
            if (banzaiPyramids.ContainsKey(itemID))
                banzaiPyramids[itemID] = item;
            else
                banzaiPyramids.TryAdd(itemID, item);
        }

        internal void RemovePyramid(uint itemID)
        {
            RoomItem junk;
            banzaiPyramids.TryRemove(itemID, out junk);
        }

        internal void AddTeleport(RoomItem item, uint itemID)
        {
            if (banzaiTeleports.ContainsKey(itemID))
                banzaiTeleports[itemID] = item;
            else
                banzaiTeleports.TryAdd(itemID, item);
        }

        internal void RemoveTeleport(uint itemID)
        {
            RoomItem junk;
            banzaiTeleports.TryRemove(itemID, out junk);
        }

        internal void onTeleportRoomUserEnter(RoomUser User, RoomItem Item)
        {
            SorteiaDeNovo:
            IEnumerable<RoomItem> items = banzaiTeleports.Values.Where(p => p.Id != Item.Id); // pega todos teleportesbanzai que são diferente do meu
            
            int count = items.Count(); // aqui a quantidade de teles banzai que tem no quarto
            if (count == 0)
                return;

            
            int countID = new Random().Next(0, count); // gera um número aleatório entre 0 e a quantidade de teles banzai no quarto
            int countAmount = 0;

            
            foreach (RoomItem item in items)
            {
                if (item == null) // se o tele não existe, continua
                    continue;

                    if (countAmount == countID) // se o tele atual for o mesmo sorteado ali em cima, ele irá teleportar p esse tele
                     {
                        List<RoomUser> usuarios = item.GetRoom().GetGameMap().GetUsersOnItem(item);
                        if (usuarios.Count > 1)
                        {
                             foreach(RoomUser userSolo in usuarios)
                             {
                                goto SorteiaDeNovo;
                             }
                        }
                    room.GetGameMap().TeleportToItem(User, item); // teleporta o user pro tele selecionado
                        User.lastTeleBanzai = OtanixEnvironment.GetUnixTimestampInMili();
                        item.ExtraData = "1";
                        item.UpdateNeeded = true;
                        item.UpdateState();

                    }

                countAmount++;
            }
        }

        internal void Destroy()
        {
            if (banzaiTeleports != null)
                banzaiTeleports.Clear();
            if (banzaiPyramids != null)
                banzaiPyramids.Clear();
            banzaiPyramids = null;
            banzaiTeleports = null;
            room = null;
        }
    }
}
