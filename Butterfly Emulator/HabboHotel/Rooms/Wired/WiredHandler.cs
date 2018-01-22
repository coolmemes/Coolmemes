using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.Rooms.Games;
using Butterfly.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Butterfly.Messages;
using HabboEvents;
using System;

namespace Butterfly.HabboHotel.Rooms.Wired
{
    public class WiredHandler
    {
        #region Declares
        private Hashtable actionItems; //interactionType, List<RoomItem>
        private readonly Hashtable actionStacks; //point, List<RoomItem>
        private Hashtable complementItems;
        private Hashtable seenedItems;
        private List<RoomItem> classificationItems;
        private Dictionary<Point, Dictionary<InteractionType, int>> triggersOnTile;

        private readonly Queue requestedTriggers;
        private readonly Queue requestingUpdates;

        private Room room;
        internal ConditionHandler conditionHandler;
        private bool doCleanup = false;
        internal int needTimersReset = 0;
        #endregion

        #region Constructor
        public WiredHandler(Room room)
        {
            actionItems = new Hashtable();
            actionStacks = new Hashtable();
            complementItems = new Hashtable();
            seenedItems = new Hashtable();
            requestedTriggers = new Queue();
            requestingUpdates = new Queue();
            classificationItems = new List<RoomItem>();
            triggersOnTile = new Dictionary<Point, Dictionary<InteractionType, int>>();

            this.room = room;
            conditionHandler = new ConditionHandler();
        }
        #endregion

        #region Furniture add/remove
        internal void AddClassFurniture(RoomItem Item)
        {
            if (!classificationItems.Contains(Item))
                classificationItems.Add(Item);
        }

        internal void RemoveClassFurniture(RoomItem Item)
        {
            if (classificationItems.Contains(Item))
                classificationItems.Remove(Item);
        }

        internal List<RoomItem> ClassificationItems
        {
            get
            {
                return this.classificationItems;
            }
        }

        internal void RefreshClassifications()
        {
            if (classificationItems.Count > 0)
            {
                foreach (RoomItem Item in classificationItems)
                {
                    if (Item.wiredPuntuation != null)
                        Item.wiredPuntuation.AddPointsToClass();
                }
            }
        }

        internal void AddFurniture(RoomItem item)
        {
            if (item.GetBaseItem().InteractionType == InteractionType.specialrandom || item.GetBaseItem().InteractionType == InteractionType.specialunseen)
            {
                AddComplementToItems(item);
            }
            else if (item.GetBaseItem().InteractionType == InteractionType.wiredClassification)
            {
                AddClassFurniture(item);
            }
            else
            {
                AddFurnitureToItems(item);
                AddFurnitureToItemStack(item);
            }
        }

        internal void RemoveFurniture(RoomItem item)
        {
            if (item.GetBaseItem().InteractionType == InteractionType.specialrandom || item.GetBaseItem().InteractionType == InteractionType.specialunseen)
            {
                RemoveComplementToItems(item);
            }
            else if (item.GetBaseItem().InteractionType == InteractionType.wiredClassification)
            {
                RemoveClassFurniture(item);
            }
            else
            {
                RemoveFurnitureFromItems(item);
                RemoveFurnitureFromStack(item);
            }
        }

        private void AddFurnitureToItems(RoomItem item)
        {
            var type = item.GetBaseItem().InteractionType;
            if (!WiredUtillity.TypeIsWired(type))
                return;

            if (actionItems.ContainsKey(type))
                ((List<RoomItem>)actionItems[type]).Add(item);
            else
            {
                var stack = new List<RoomItem> { item };

                actionItems.Add(type, stack);
            }

        }

        private void RemoveFurnitureFromItems(RoomItem item)
        {
            var type = item.GetBaseItem().InteractionType;
            if (actionItems.ContainsKey(type))
                ((List<RoomItem>)actionItems[type]).Remove(item);
        }

        private void AddFurnitureToItemStack(RoomItem item)
        {
            var itemCoord = item.Coordinate;

            if (actionStacks.ContainsKey(itemCoord))
                ((List<RoomItem>)actionStacks[itemCoord]).Add(item);
            else
            {
                var stack = new List<RoomItem> { item };

                actionStacks.Add(itemCoord, stack);
            }
        }

        private void RemoveFurnitureFromStack(RoomItem item)
        {
            var itemCoord = item.Coordinate;
            if (actionStacks.ContainsKey(itemCoord))
                ((List<RoomItem>)actionStacks[itemCoord]).Remove(item);
        }

        private void AddComplementToItems(RoomItem item)
        {
            var itemCoord = item.Coordinate;
            if (complementItems.ContainsKey(itemCoord))
                ((List<RoomItem>)complementItems[itemCoord]).Add(item);
            else
                complementItems.Add(itemCoord, new List<RoomItem> { item });
        }

        private void RemoveComplementToItems(RoomItem item)
        {
            Point itemCoord = item.Coordinate;
            if (complementItems.ContainsKey(itemCoord))
            {
                List<RoomItem> listItems = ((List<RoomItem>)complementItems[itemCoord]);
                listItems.Remove(item);

                if (listItems.Count == 0)
                    complementItems.Remove(itemCoord);
            }
        }
        private IWiredEffect getRandomTeleport(Point coord)
        {
            List<RoomItem> aList = ((List<RoomItem>)actionStacks[coord]).FindAll(item => item.GetBaseItem().InteractionType == InteractionType.actionteleportto); // pega todos os wired de teleport da cordenada
            Random rand = new Random(DateTime.Now.Millisecond);
            RoomItem stackItem = aList[rand.Next(aList.Count)]; // Pega um deles aleatoriamente
            
            // essa parte ele executa o efeito
            var effect = stackItem.wiredHandler as IWiredEffect;
            if (effect != null)
            {
                return effect;
            }

            return null;
        }

        private IWiredEffect getUnseenedItem(Point coord)
        {
            List<RoomItem> aList = ((List<RoomItem>)actionStacks[coord]).FindAll( item => item.GetBaseItem().Name.StartsWith("wf_act_")); // pega todos os efeitos que estão na coordenada (Point coord)
            Int32 aCount = aList.Count; // quantidade de wired de efeitos da coordenada

            if (!seenedItems.ContainsKey(coord)) // se este item ainda não foi visto
                seenedItems.Add(coord, -1); // adiciona ele
            else if ((Int32)seenedItems[coord]+1 >= aCount) // se a quantidade de efeitos vistos for maior que o total
                seenedItems[coord] = -1; // seta como -1 (-1 é o inicio)

            seenedItems[coord] = (Int32)seenedItems[coord] + 1; /// incrementa em 1 o efeito visto para esta coordenada (ex: tem 10, vai aumentando de 1 em 1)
            RoomItem stackItem = aList[(Int32)seenedItems[coord]]; // Pega o item de acordo com a posição dele nos itens vistos pela coordenada
           
            // essa parte ele executa o efeito
            var effect = stackItem.wiredHandler as IWiredEffect;
            if (effect != null)
            {
                return effect;
            }
            
            return null;
        }
        #endregion

        #region Room cycle management
        internal void OnCycle()
        {
            if (doCleanup)
            {
                foreach (List<RoomItem> list in actionStacks.Values)
                {
                    foreach (var item in list)
                    {
                        if (item.wiredCondition != null)
                        {
                            item.wiredCondition.Dispose();
                            item.wiredCondition = null;
                        }
                        if (item.wiredHandler != null)
                        {
                            item.wiredHandler.Dispose();
                            item.wiredHandler = null;
                        }
                    }
                }
                actionStacks.Clear();
                actionItems.Clear();
                requestedTriggers.Clear();
                requestingUpdates.Clear();

                doCleanup = false;
                return;
            }

            if (requestingUpdates.Count > 0)
            {
                var toAdd = new List<IWiredCycleable>();
                lock (requestingUpdates.SyncRoot)
                {
                    while (requestingUpdates.Count > 0)
                    {
                        var handler = (IWiredCycleable)requestingUpdates.Dequeue();
                        if (handler.Disposed())
                        {
                            continue;
                        }

                        if (handler.OnCycle())
                        {
                            toAdd.Add(handler);
                        }

                        if (needTimersReset == 2)
                        {
                            handler.ResetTimer();
                        }
                    }

                    foreach (var cycle in toAdd)
                    {
                        requestingUpdates.Enqueue(cycle);
                    }
                }
            }

            triggersOnTile.Clear();

            if (needTimersReset == 1)
                needTimersReset = 2;
            else if (needTimersReset == 2)
                needTimersReset = 0;
        }
        #endregion

        #region Functions
        internal void OnPickall()
        {
            doCleanup = true;
        }

        internal List<int> InvalidWired(RoomItem Item)
        {
            if (!actionStacks.ContainsKey(Item.Coordinate))
                return new List<int>();

            var WiredsOnTile = new List<RoomItem>((List<RoomItem>)actionStacks[Item.Coordinate]);

            return (from zItem in WiredsOnTile where zItem != Item where !Item.GetBaseItem().Name.ToLower().StartsWith("wf_trg") || !zItem.GetBaseItem().Name.ToLower().StartsWith("wf_trg") where !AreWiredsCompatible(Item.GetBaseItem().InteractionType, zItem.GetBaseItem().InteractionType) select zItem.GetBaseItem().SpriteId).ToList();
        }

        internal bool AreWiredsCompatible(InteractionType iType, InteractionType jType)
        {
            #region Item One
            switch (iType)
            {
                case (InteractionType.triggergameend):
                    {
                        if (jType == InteractionType.actiongivescore)
                            return false;
                        else if (jType == InteractionType.actionshowmessage)
                            return false;
                        else if (jType == InteractionType.actionhandiitemcustom)
                            return false;
                        else if (jType == InteractionType.actioneffectcustom)
                            return false;
                        else if (jType == InteractionType.actiondiamantescustom)
                            return false;
                        else if (jType == InteractionType.actiondancecustom)
                            return false;
                        else if (jType == InteractionType.actionfastwalk)
                            return false;
                        else if (jType == InteractionType.actionfreezecustom)
                            return false;
                        else if (jType == InteractionType.actiongivescore)
                            return false;
                        else if (jType == InteractionType.actionkickuser)
                            return false;
                        else if (jType == InteractionType.actionteleportto)
                            return false;
                        else if (jType == InteractionType.actionjointoteam)
                            return false;
                        else if (jType == InteractionType.actionleaveteam)
                            return false;
                        break;
                    }
                case InteractionType.triggergamestart:
                    {
                        if (jType == InteractionType.actiongivescore)
                            return false;
                        else if (jType == InteractionType.actionshowmessage)
                            return false;
                        else if (jType == InteractionType.actionhandiitemcustom)
                            return false;
                        else if (jType == InteractionType.actioneffectcustom)
                            return false;
                        else if (jType == InteractionType.actiondiamantescustom)
                            return false;
                        else if (jType == InteractionType.actiondancecustom)
                            return false;
                        else if (jType == InteractionType.actionfastwalk)
                            return false;
                        else if (jType == InteractionType.actionfreezecustom)
                            return false;
                        else if (jType == InteractionType.actionteleportto)
                            return false;
                        else if (jType == InteractionType.actionjointoteam)
                            return false;
                        else if (jType == InteractionType.actionleaveteam)
                            return false;
                        else if (jType == InteractionType.actiongivescore)
                            return false;
                        else if (jType == InteractionType.actionkickuser)
                            return false;
                        break;
                    }
                case InteractionType.triggerrepeater:
                    {
                        if (jType == InteractionType.actionteleportto)
                            return false;
                        else if (jType == InteractionType.actionshowmessage)
                            return false;
                        else if (jType == InteractionType.actionhandiitemcustom)
                            return false;
                        else if (jType == InteractionType.actioneffectcustom)
                            return false;
                        else if (jType == InteractionType.actiondiamantescustom)
                            return false;
                        else if (jType == InteractionType.actiondancecustom)
                            return false;
                        else if (jType == InteractionType.actionfastwalk)
                            return false;
                        else if (jType == InteractionType.actionfreezecustom)
                            return false;
                        else if (jType == InteractionType.actiongivescore)
                            return false;
                        else if (jType == InteractionType.actionjointoteam)
                            return false;
                        else if (jType == InteractionType.actionleaveteam)
                            return false;
                        else if (jType == InteractionType.actionkickuser)
                            return false;
                        break;
                    }
                case InteractionType.triggertimer:
                    {
                        if (jType == InteractionType.actiongivescore)
                            return false;
                        else if (jType == InteractionType.actionteleportto)
                            return false;
                        else if (jType == InteractionType.actionshowmessage)
                            return false;
                        else if (jType == InteractionType.actionhandiitemcustom)
                            return false;
                        else if (jType == InteractionType.actioneffectcustom)
                            return false;
                        else if (jType == InteractionType.actiondiamantescustom)
                            return false;
                        else if (jType == InteractionType.actiondancecustom)
                            return false;
                        else if (jType == InteractionType.actionfastwalk)
                            return false;
                        else if (jType == InteractionType.actionfreezecustom)
                            return false;
                        else if (jType == InteractionType.actionjointoteam)
                            return false;
                        else if (jType == InteractionType.actionleaveteam)
                            return false;
                        else if (jType == InteractionType.actionkickuser)
                            return false;
                        break;
                    }
                case InteractionType.triggerlongperiodic:
                    {
                        if (jType == InteractionType.actionteleportto)
                            return false;
                        else if (jType == InteractionType.actionshowmessage)
                            return false;
                        else if (jType == InteractionType.actionhandiitemcustom)
                            return false;
                        else if (jType == InteractionType.actioneffectcustom)
                            return false;
                        else if (jType == InteractionType.actiondiamantescustom)
                            return false;
                        else if (jType == InteractionType.actiondancecustom)
                            return false;
                        else if (jType == InteractionType.actionfastwalk)
                            return false;
                        else if (jType == InteractionType.actionfreezecustom)
                            return false;
                        else if (jType == InteractionType.actiongivescore)
                            return false;
                        else if (jType == InteractionType.actionjointoteam)
                            return false;
                        else if (jType == InteractionType.actionleaveteam)
                            return false;
                        else if (jType == InteractionType.actionkickuser)
                            return false;
                        break;
                    }
            }
            #endregion
            #region Item Two
            switch (jType)
            {
                case (InteractionType.triggergameend):
                    {
                        if (iType == InteractionType.actiongivescore)
                            return false;
                        else if (iType == InteractionType.actionshowmessage)
                            return false;
                        else if (jType == InteractionType.actionhandiitemcustom)
                            return false;
                        else if (jType == InteractionType.actioneffectcustom)
                            return false;
                        else if (jType == InteractionType.actiondiamantescustom)
                            return false;
                        else if (jType == InteractionType.actiondancecustom)
                            return false;
                        else if (jType == InteractionType.actionfastwalk)
                            return false;
                        else if (jType == InteractionType.actionfreezecustom)
                            return false;
                        else if (iType == InteractionType.actionteleportto)
                            return false;
                        else if (iType == InteractionType.actionjointoteam)
                            return false;
                        else if (iType == InteractionType.actionleaveteam)
                            return false;
                        else if (iType == InteractionType.actiongivescore)
                            return false;
                        else if (iType == InteractionType.actionkickuser)
                            return false;
                        break;
                    }
                case InteractionType.triggergamestart:
                    {
                        if (iType == InteractionType.actiongivescore)
                            return false;
                        else if (iType == InteractionType.actionshowmessage)
                            return false;
                        else if (jType == InteractionType.actionhandiitemcustom)
                            return false;
                        else if (jType == InteractionType.actioneffectcustom)
                            return false;
                        else if (jType == InteractionType.actiondiamantescustom)
                            return false;
                        else if (jType == InteractionType.actiondancecustom)
                            return false;
                        else if (jType == InteractionType.actionfastwalk)
                            return false;
                        else if (jType == InteractionType.actionfreezecustom)
                            return false;
                        else if (iType == InteractionType.actionteleportto)
                            return false;
                        else if (iType == InteractionType.actionjointoteam)
                            return false;
                        else if (iType == InteractionType.actionleaveteam)
                            return false;
                        else if (iType == InteractionType.actiongivescore)
                            return false;
                        else if (iType == InteractionType.actionkickuser)
                            return false;
                        break;
                    }
                case InteractionType.triggerrepeater:
                    {
                        if (iType == InteractionType.actionteleportto)
                            return false;
                        else if (iType == InteractionType.actionshowmessage)
                            return false;
                        else if (jType == InteractionType.actionhandiitemcustom)
                            return false;
                        else if (jType == InteractionType.actioneffectcustom)
                            return false;
                        else if (jType == InteractionType.actiondancecustom)
                            return false;
                        else if (jType == InteractionType.actiondiamantescustom)
                            return false;
                        else if (jType == InteractionType.actionfastwalk)
                            return false;
                        else if (jType == InteractionType.actionfreezecustom)
                            return false;
                        else if (iType == InteractionType.actiongivescore)
                            return false;
                        else if (iType == InteractionType.actionjointoteam)
                            return false;
                        else if (iType == InteractionType.actionleaveteam)
                            return false;
                        else if (iType == InteractionType.actionkickuser)
                            return false;
                        break;
                    }
                case InteractionType.triggertimer:
                    {
                        if (iType == InteractionType.actiongivescore)
                            return false;
                        else if (iType == InteractionType.actionteleportto)
                            return false;
                        else if (iType == InteractionType.actionshowmessage)
                            return false;
                        else if (jType == InteractionType.actionhandiitemcustom)
                            return false;
                        else if (jType == InteractionType.actiondancecustom)
                            return false;
                        else if (jType == InteractionType.actioneffectcustom)
                            return false;
                        else if (jType == InteractionType.actionfastwalk)
                            return false;
                        else if (jType == InteractionType.actionfreezecustom)
                            return false;
                        else if (jType == InteractionType.actiondiamantescustom)
                            return false;
                        else if (iType == InteractionType.actionjointoteam)
                            return false;
                        else if (iType == InteractionType.actionleaveteam)
                            return false;
                        else if (iType == InteractionType.actionkickuser)
                            return false;
                        break;
                    }
                case InteractionType.triggerlongperiodic:
                    {
                        if (iType == InteractionType.actiongivescore)
                            return false;
                        else if (iType == InteractionType.actionteleportto)
                            return false;
                        else if (iType == InteractionType.actionshowmessage)
                            return false;
                        else if (jType == InteractionType.actiondancecustom)
                            return false;
                        else if (jType == InteractionType.actionhandiitemcustom)
                            return false;
                        else if (jType == InteractionType.actioneffectcustom)
                            return false;
                        else if (jType == InteractionType.actiondiamantescustom)
                            return false;
                        else if (jType == InteractionType.actionfastwalk)
                            return false;
                        else if (jType == InteractionType.actionfreezecustom)
                            return false;
                        else if (iType == InteractionType.actionjointoteam)
                            return false;
                        else if (iType == InteractionType.actionleaveteam)
                            return false;
                        else if (iType == InteractionType.actionkickuser)
                            return false;
                        break;
                    }
            }
            #endregion

            return true;
        }

        private bool MultipleTriggersOnTile(RoomItem Item)
        {
            if(triggersOnTile.ContainsKey(Item.Coordinate))
            {
                Dictionary<InteractionType, int> ii = triggersOnTile[Item.Coordinate];
                if(ii.ContainsKey(Item.GetBaseItem().InteractionType))
                {
                    int count = ii[Item.GetBaseItem().InteractionType];
                    if (count > 1)
                        return true;
                    else
                        ii[Item.GetBaseItem().InteractionType]++;
                }
                else
                {
                    ii.Add(Item.GetBaseItem().InteractionType, 1);
                }
            }
            else
            {
                Dictionary<InteractionType, int> ii = new Dictionary<InteractionType,int>();
                ii.Add(Item.GetBaseItem().InteractionType, 1);

                triggersOnTile.Add(Item.Coordinate, ii);
            }

            return false;
        }

        internal List<RoomItem> GetWiredsInteractor(InteractionType Type)
        {
            return ((List<RoomItem>)actionItems[Type]);
        }
        #endregion

        #region Requests
        internal bool RequestStackHandle(RoomItem trigger, RoomItem item, RoomUser user, Team team)
        {
            try
            {
                bool ExecutedTrigger = false;
                if (!room.IsRoomLoaded) // así evitamos que cuando iniciamos la sala, el condición tarda 1 cyclo más en iniciarse, y por lo tanto no funcionan en 0.5 s.
                    return false;

                Point coordinate = trigger.Coordinate;

                if (MultipleTriggersOnTile(trigger))
                    return false;

                if (actionStacks.ContainsKey(coordinate) && conditionHandler.AllowsHandling(coordinate, user))
                {
                    List<RoomItem> items = (List<RoomItem>)actionStacks[coordinate];
                    if (complementItems.ContainsKey(coordinate) && items.Count > 0)
                    {
                        List<RoomItem> complements = (List<RoomItem>)complementItems[coordinate];
                        foreach (RoomItem complement in complements)
                        {
                            if (complement.GetBaseItem().InteractionType == InteractionType.specialrandom)
                            {
                                Refaz:
                                RoomItem stackItem = items[new Random().Next(0, items.Count)];
                                var effect = stackItem.wiredHandler as IWiredEffect;
                                if (effect != null)
                                {
                                    effect.Handle(user, team, item);
                                    ExecutedTrigger = true;
                                }
                                else goto Refaz;
                            }
                            else if (complement.GetBaseItem().InteractionType == InteractionType.specialunseen)
                            {
                                IWiredEffect effect = getUnseenedItem(coordinate);
                                if (effect != null)
                                {
                                    effect.Handle(user, team, item);
                                    ExecutedTrigger = true;
                                }
                            }
                            
                        }
                    }
                    else if (items.Count > 0)
                    {
                        foreach (var stackItem in items)
                        {
                            if (!AreWiredsCompatible(trigger.GetBaseItem().InteractionType, stackItem.GetBaseItem().InteractionType))
                                continue;

                            if (stackItem.GetBaseItem().InteractionType == InteractionType.actionteleportto)
                            {
                                IWiredEffect effect2 = getRandomTeleport(coordinate);
                                if (effect2 != null)
                                {
                                    effect2.Handle(user, team, item);
                                    ExecutedTrigger = true;
                                }
                            }
                            else
                            {
                                var effect = stackItem.wiredHandler as IWiredEffect;
                                if (effect != null)
                                {
                                    effect.Handle(user, team, item);
                                    ExecutedTrigger = true;
                                }
                            }
                        }
                    }
                }

                return ExecutedTrigger;
            }
            catch { return false; }
        }

        internal void RequestStackHandleEffects(Point coordinate, RoomItem item, RoomUser user, Team team)
        {
            if (actionStacks.ContainsKey(coordinate))
            {
                List<RoomItem> items = (List<RoomItem>)actionStacks[coordinate];
                foreach (var stackItem in items)
                {
                    if (!stackItem.GetBaseItem().Name.StartsWith("wf_act_") || stackItem.GetBaseItem().InteractionType == InteractionType.actioncallstacks)
                    {
                        continue;
                    }

                    var effect = stackItem.wiredHandler as IWiredEffect;
                    if (effect != null)
                    {
                        effect.Handle(user, team, item);
                    }
                }
            }
        }

        internal void RequestCycle(IWiredCycleable handler)
        {
            lock (requestingUpdates.SyncRoot)
            {
                requestingUpdates.Enqueue(handler);
            }
        }
        #endregion

        #region Return values
        internal Room GetRoom()
        {
            return room;
        }

        #endregion

        #region Unloading
        internal void Destroy()
        {
            if (actionItems != null)
                actionItems.Clear();
            actionItems = null;
            if (actionStacks != null)
                actionStacks.Clear();
            if (complementItems != null)
                complementItems.Clear();
            complementItems = null;
            if (seenedItems != null)
                seenedItems.Clear();
            seenedItems = null;
            requestedTriggers.Clear();
            requestingUpdates.Clear();
            room = null;
        }
        #endregion
    }
}
