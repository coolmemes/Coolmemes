using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;
using Butterfly.Messages;
using Butterfly.HabboHotel.Users.UserDataManagement;
using ButterStorm;
using HabboEvents;

namespace Butterfly.HabboHotel.Users.Inventory
{
    class AvatarEffectsInventoryComponent
    {
        private readonly uint UserId;
        internal int CurrentEffect;
        private GameClient mClient;
        internal Dictionary<int, AvatarEffect> AvatarEffects;
        internal int BackupEffect;

        internal AvatarEffectsInventoryComponent(uint UserId, GameClient pClient, UserData data)
        {
            this.mClient = pClient;
            this.UserId = UserId;
            this.CurrentEffect = -1;
            this.AvatarEffects = new Dictionary<int, AvatarEffect>();
            var QueryBuilder = new StringBuilder();
            foreach (var effect in data.effects)
            {
                if (!effect.HasExpired)
                {
                    AvatarEffects.Add(effect.EffectId, effect);
                }
                else
                {
                    if (effect.EffectCount <= 1)
                        QueryBuilder.Append("DELETE FROM user_effects WHERE user_id = " + UserId + " AND effect_id = " + effect.EffectId + "; ");
                    else
                        QueryBuilder.Append("UPDATE user_effects SET effect_count = effect_count - 1, is_activated = '0', activated_stamp = '0' WHERE user_id = " + UserId + " AND effect_id = " + effect.EffectId + "; ");
                }
            }

            if (QueryBuilder.Length > 0)
            {
                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    dbClient.runFastQuery(QueryBuilder.ToString());
            }
        }
        internal void Dispose()
        {
            AvatarEffects.Clear();
            mClient = null;
        }

        internal void AddEffect(int EffectId, int Duration)
        {
            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                if (!AvatarEffects.ContainsKey(EffectId))
                {
                    dbClient.runFastQuery("INSERT INTO user_effects (user_id,effect_id,total_duration,is_activated,activated_stamp, effect_count) VALUES (" + UserId + "," + EffectId + "," + Duration + ",0,0,1)");
                    AvatarEffects.Add(EffectId, new AvatarEffect(EffectId, Duration, false, 0, 1));
                }
                else
                {
                    dbClient.runFastQuery("UPDATE user_effects SET effect_count = effect_count + 1 WHERE user_id = '" + GetClient().GetHabbo().Id + "' AND effect_id = '" + EffectId + "'");
                    AvatarEffects[EffectId].EffectCount++;
                }
            }

            GetClient().GetMessageHandler().GetResponse().Init(Outgoing.AddEffectToInventary);
            GetClient().GetMessageHandler().GetResponse().AppendInt32(EffectId);
            GetClient().GetMessageHandler().GetResponse().AppendInt32(0);
            GetClient().GetMessageHandler().GetResponse().AppendInt32(Duration == -1 ? int.MaxValue : Duration);
            GetClient().GetMessageHandler().GetResponse().AppendBoolean(Duration == -1); //(true == permanent effect; false == dayly)
            GetClient().GetMessageHandler().SendResponse();
        }

        internal void StopEffect(int EffectId)
        {
            var Effect = GetEffect(EffectId, true);

            if (Effect == null || !Effect.HasExpired)
            {
                return;
            }

            if (Effect.TotalDuration != -1)
            {
                if (Effect.EffectCount == 1)
                {
                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        dbClient.runFastQuery("DELETE FROM user_effects WHERE user_id = " + UserId + " AND effect_id = " + EffectId);
                    }
                    AvatarEffects.Remove(EffectId);
                }
                else
                {
                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        dbClient.runFastQuery("UPDATE user_effects SET effect_count = effect_count - 1, is_activated = '0' WHERE user_id = " + UserId + " AND effect_id = " + EffectId);
                    }
                    AvatarEffects[EffectId].EffectCount--;
                }
            }


            GetClient().GetMessageHandler().GetResponse().Init(Outgoing.StopEffect);
            GetClient().GetMessageHandler().GetResponse().AppendInt32(EffectId);
            GetClient().GetMessageHandler().SendResponse();

            if (CurrentEffect >= 0)
            {
                ApplyEffect(0);
            }
        }

        internal void ApplyCustomEffect(int EffectId)
        {
            var Room = GetUserRoom();

            if (Room == null || Room.GetRoomUserManager() == null)
            {
                return;
            }

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(GetClient().GetHabbo().Id);

            if (User == null)
            {
                return;
            }

            if(User.montandoBol == true && EffectId != 77 && EffectId != 103) // Quiero ser un caballo pero no puedo :(
            {
                return;
            }
            
            // No tengo suficiente rango para ponerme estos efectos:
            if ((EffectId == 187 && !GetClient().GetHabbo().HasFuse("fuse_badge_bot")) || (EffectId == 178 && !GetClient().GetHabbo().HasFuse("fuse_ambassador")) || (EffectId == 102 && !GetClient().GetHabbo().HasFuse("fuse_badge_staff")))
            {
                User.WhisperComposer("No tienes rango, por lo tanto no intentes colocarte esto...");
                return;
            }
            else if (User.GetClient().GetHabbo().showingStaffBadge == false && (EffectId == 178 || EffectId == 102 || EffectId == 187))
            {
                User.WhisperComposer("¡Tienes la habilitación de efectos staff desactivada, actívala!");
                return;
            }

            if (EffectId == 0)
            {
                if (!GetClient().GetHabbo().HasFuse("fuse_hide_staff") && GetClient().GetHabbo().showingStaffBadge == true)
                {
                    if (GetClient().GetHabbo().HasFuse("fuse_badge_staff"))
                    {
                        EffectId = 102;
                    }
                    else if (GetClient().GetHabbo().HasFuse("fuse_ambassador"))
                    {
                        EffectId = 178;
                    }
                    else if (GetClient().GetHabbo().HasFuse("fuse_badge_bot"))
                    {
                        EffectId = 187;
                    }
                }
            }

            BackupEffect = CurrentEffect;
            CurrentEffect = EffectId;

            var Message = new ServerMessage(Outgoing.ApplyEffects);
            Message.AppendInt32(User.VirtualId);
            Message.AppendInt32(EffectId);
            Message.AppendInt32(0);
            Room.SendMessage(Message);
        }

        internal void ApplyEffect(int EffectId)
        {
            if (!HasEffect(EffectId))
            {
                return;
            }

            Room Room = GetUserRoom();

            if (Room == null || Room.GetRoomUserManager() == null)
            {
                return;
            }

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(GetClient().GetHabbo().Id);

            if (User == null)
            {
                return;
            }

            if (User.montandoBol == true && EffectId != 77 && EffectId != 103)
            {
                return;
            }
            else if ((EffectId == 187 && !GetClient().GetHabbo().HasFuse("fuse_badge_bot")) || (EffectId == 178 && !GetClient().GetHabbo().HasFuse("fuse_ambassador")) || (EffectId == 102 && !GetClient().GetHabbo().HasFuse("fuse_badge_staff")))
            {
                User.GetClient().SendNotif("No tienes rango, por lo tanto no intentes colocarte esto...");
                return;
            }
            if (User.GetClient().GetHabbo().showingStaffBadge == false && (EffectId == 178 || EffectId == 102 || EffectId == 187))
            {
                User.GetClient().SendNotif("¡Tienes la habilitación de efectos staff desactivada, actívala!");
                return;
            }

            if (EffectId == 0)
            {
                if (!GetClient().GetHabbo().HasFuse("fuse_hide_staff"))
                {
                    if (GetClient().GetHabbo().HasFuse("fuse_badge_staff") && GetClient().GetHabbo().showingStaffBadge)
                    {
                        EffectId = 102;
                    }
                    else if (GetClient().GetHabbo().HasFuse("fuse_ambassador") && GetClient().GetHabbo().showingStaffBadge)
                    {
                        EffectId = 178;
                    }
                    else if (GetClient().GetHabbo().HasFuse("fuse_badge_bot") && GetClient().GetHabbo().showingStaffBadge)
                    {
                        EffectId = 187;
                    }
                }
            }

            CurrentEffect = EffectId;

            var Message = new ServerMessage(Outgoing.ApplyEffects);
            Message.AppendInt32(User.VirtualId);
            Message.AppendInt32(EffectId);
            Message.AppendInt32(0);
            Room.SendMessage(Message);
        }

        internal void EnableEffect(int EffectId)
        {
            var Effect = GetEffect(EffectId, false);

            if (Effect == null || ((Effect.HasExpired || Effect.Activated) && Effect.TotalDuration != -1))
            {
                return;
            }

            if (Effect.TotalDuration != -1)
            {
                using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.runFastQuery("UPDATE user_effects SET is_activated = '1', activated_stamp = " + OtanixEnvironment.GetUnixTimestamp() + " WHERE user_id = " + UserId + " AND effect_id = " + EffectId + "");
                }
            }

            Effect.Activate();

            GetClient().GetMessageHandler().GetResponse().Init(Outgoing.EnableEffect);
            GetClient().GetMessageHandler().GetResponse().AppendInt32(Effect.EffectId);
            GetClient().GetMessageHandler().GetResponse().AppendInt32(Effect.TotalDuration);
            GetClient().GetMessageHandler().GetResponse().AppendBoolean(Effect.TotalDuration == -1);
            GetClient().GetMessageHandler().SendResponse();
        }

        internal bool HasEffect(int EffectId)
        {
            if (EffectId == -1)
                return true;

            if(AvatarEffects.ContainsKey(EffectId))
            {
                AvatarEffect effect = AvatarEffects[EffectId];
                if (effect.TotalDuration == -1)
                    return true;

                if (effect.Activated == false)
                    return true;

                if (effect.Activated && !effect.HasExpired)
                    return true;
            }

            return false;
        }

        internal AvatarEffect GetEffect(int EffectId, bool IfEnabledOnly)
        {
            return AvatarEffects.Values.Where(Effect => !IfEnabledOnly || Effect.Activated).FirstOrDefault(Effect => Effect.EffectId == EffectId);
        }

        internal ServerMessage Serialize()
        {
            var Message = new ServerMessage(Outgoing.SerializeCurrentEffects);
            Message.AppendInt32(AvatarEffects.Count);

            foreach (var Effect in AvatarEffects.Values)
            {
                Message.AppendInt32(Effect.EffectId);
                Message.AppendInt32(0);
                Message.AppendInt32(Effect.TotalDuration);
                Message.AppendInt32((Effect.TimeLeft == -1) ? Effect.EffectCount : Effect.EffectCount - 1);
                Message.AppendInt32(!Effect.Activated ? -1 : Effect.TimeLeft);
                Message.AppendBoolean(Effect.TotalDuration == -1); // true: ${avatareditor.effects.active.permanent}
            }
            return Message;
        }

        internal void CheckExpired()
        {
            var toRemove = new Queue();
            if (AvatarEffects.Count <= 0)
                return;

            foreach (var Effect in AvatarEffects.Values)
            {
                if (Effect.HasExpired)
                    toRemove.Enqueue(Effect.EffectId);
            }

            if (toRemove.Count > 0)
            {
                int EffectID;
                do
                {
                    EffectID = (int)toRemove.Dequeue();
                    StopEffect(EffectID);
                } while (toRemove.Count > 0);
            }
        }

        private GameClient GetClient()
        {
            return mClient;
        }

        private Room GetUserRoom()
        {
            return mClient.GetHabbo().CurrentRoom;
        }

        internal void OnRoomExit()
        {
            CurrentEffect = 0;
        }

        internal static void ExecuteEffect(int EffectId, int VirtualId, Room Room)
        {
            ServerMessage Message = new ServerMessage(Outgoing.ApplyEffects);
            Message.AppendInt32(VirtualId);
            Message.AppendInt32(EffectId);
            Message.AppendInt32(0);
            Room.SendMessage(Message);
        }
    }
}
