using Butterfly.HabboHotel.Items;
using Database_Manager.Database.Session_Details.Interfaces;

namespace Butterfly.HabboHotel.Rooms.Wired
{
    class WiredUtillity
    {
        internal static bool NeedsFurnitures(InteractionType Type)
        {
            switch (Type)
            {
                case InteractionType.triggerwalkonfurni:
                case InteractionType.triggerwalkofffurni:
                case InteractionType.triggerstatechanged:
                case InteractionType.triggerbotreachedstf:

                case InteractionType.actionposreset:
                case InteractionType.actiontogglestate:
                case InteractionType.actionmoverotate:
                case InteractionType.actionteleportto:
                case InteractionType.actionchase:
                case InteractionType.actionescape:
                case InteractionType.actioncallstacks:
                case InteractionType.actionmovetodir:
                case InteractionType.actionbotteleport:
                case InteractionType.actionbotmove:
                case InteractionType.actionmovetofurni:

                case InteractionType.conditionfurnishaveusers:
                case InteractionType.conditiontriggeronfurni:
                case InteractionType.conditionhasfurnion:
                case InteractionType.conditionstatepos:
                case InteractionType.conditionstuffis:
                case InteractionType.conditionnotfurnishaveusers:
                case InteractionType.conditionnottriggeronfurni:
                case InteractionType.conditionnotfurnion:
                case InteractionType.conditionnotstatepos:
                case InteractionType.conditionnotstuffis:

                    return true;
            }

            return false;
        }

        internal static bool HaveSettings(InteractionType Type)
        {
            switch (Type)
            {
                case InteractionType.triggerscoreachieved:
                case InteractionType.triggerroomenter:
                case InteractionType.triggerrepeater:
                case InteractionType.triggertimer:
                case InteractionType.triggeronusersay:
                case InteractionType.triggerlongperiodic:
                case InteractionType.triggerbotreachedavtr:
                case InteractionType.triggerbotreachedstf:

                case InteractionType.actiongivescore:
                case InteractionType.actionmoverotate:
                case InteractionType.actiontogglestate:
                case InteractionType.actionposreset:
                case InteractionType.actionshowmessage:
                case InteractionType.actionhandiitemcustom:
                case InteractionType.actioneffectcustom:
                case InteractionType.actiondiamantescustom:
                case InteractionType.actiondancecustom:
                case InteractionType.actionfreezecustom:
                case InteractionType.actionfastwalk:
                case InteractionType.actionresettimer:
                case InteractionType.actiongivereward:
                case InteractionType.actionkickuser:
                case InteractionType.actionjointoteam:
                case InteractionType.actiongiveteamscore:
                case InteractionType.actionmovetodir:
                case InteractionType.actionbotteleport:
                case InteractionType.actionbotmove:
                case InteractionType.actionbotwhisper:
                case InteractionType.actionbotclothes:
                case InteractionType.actionbottalk:
                case InteractionType.actionbothanditem:
                case InteractionType.actionbotfollowavt:
                case InteractionType.actionmutetriggerer:
                case InteractionType.actionmovetofurni:

                case InteractionType.conditionhasfurnion:
                case InteractionType.conditionstatepos:
                case InteractionType.conditiontimelessthan:
                case InteractionType.conditiontimemorethan:
                case InteractionType.conditionactorinteam:
                case InteractionType.conditionusercountin:
                case InteractionType.conditionnotfurnion:
                case InteractionType.conditionnotstatepos:
                case InteractionType.conditionnotinteam:
                case InteractionType.conditionnotusercount:
                case InteractionType.conditionwearingeffect:
                case InteractionType.conditionnotwearingeffect:
                case InteractionType.conditiondaterange:
                case InteractionType.conditionwearingbadge:
                case InteractionType.conditionnotwearingbadge:
                case InteractionType.conditionhandleitemid:

                    return true;
            }

            return false;
        }

        internal static bool HaveLocations(InteractionType Type)
        {
            switch (Type)
            {
                case InteractionType.actionposreset:
                case InteractionType.conditionstatepos:
                case InteractionType.conditionnotstatepos:

                    return true;
            }

            return false;
        }

        internal static bool TypeIsWired(InteractionType type)
        {
            switch (type)
            {
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
                case InteractionType.actiongivereward:
                case InteractionType.actionchase:
                case InteractionType.actionkickuser:
                case InteractionType.actionescape:
                case InteractionType.actionjointoteam:
                case InteractionType.actionleaveteam:
                case InteractionType.actiongiveteamscore:
                case InteractionType.actioncallstacks:
                case InteractionType.actionmovetodir:
                case InteractionType.actionbotteleport:
                case InteractionType.actionbotmove:
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
                case InteractionType.specialrandom:
                case InteractionType.specialunseen:
                case InteractionType.wiredClassification:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool TypeIsWiredCondition(InteractionType type)
        {
            switch (type)
            {
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
                    return true;
                default:
                    return false;
            }
        }
    }
}
