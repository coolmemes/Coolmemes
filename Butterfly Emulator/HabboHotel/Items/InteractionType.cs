using System;

namespace Butterfly.HabboHotel.Items
{
    enum InteractionType
    {
        none, //None == default
        gate,
        postit,
        roomeffect,
        dimmer,
        trophy,
        bed,
        shower,
        scoreboard,
        vendingmachine,
        alert,
        onewaygate,
        loveshuffler,
        habbowheel,
        dice,
        bottle,
        teleport,
        saltasalas,
        rentals,
        pet,
        pool,
        roller,
        fbgate,
        iceskates,
        normslaskates,
        snowboard,
        lowpool,
        haloweenpool,
        haloweenpool15,
        football,
        footballgoalgreen,
        footballgoalyellow,
        footballgoalblue,
        footballgoalred,
        footballcountergreen,
        footballcounteryellow,
        footballcounterblue,
        footballcounterred,
        banzaigateblue,
        banzaigatered,
        banzaigateyellow,
        banzaigategreen,
        banzaifloor,
        banzaiscoreblue,
        banzaiscorered,
        banzaiscoreyellow,
        banzaiscoregreen,
        banzaitele,
        banzaipuck,
        banzaipyramid,
        freezeexit,
        freezeredcounter,
        freezebluecounter,
        freezeyellowcounter,
        freezegreencounter,
        freezeyellowgate,
        freezeredgate,
        freezegreengate,
        freezebluegate,
        freezetileblock,
        freezetile,
        jukebox,
        musicdisc,
        puzzlebox,
        balloon15,
        craftable,
        horsejump,
        seed,
        fxprovider,

        //Wired:
        triggertimer,
        triggerroomenter,
        triggergameend,
        triggergamestart,
        triggerrepeater,
        triggeronusersay,
        triggerscoreachieved,
        triggerstatechanged,
        triggerwalkonfurni,
        triggerwalkofffurni,
        triggercollision,
        triggerlongperiodic,
        triggerbotreachedavtr,
        triggerbotreachedstf,

        actiongivescore,
        actionposreset,
        actionmoverotate,
        actionresettimer,
        actionshowmessage,
        actionhandiitemcustom,
        actioneffectcustom,
        actiondiamantescustom,
        actiondancecustom,
        actionfastwalk,
        actionfreezecustom,
        actionteleportto,
        actiontogglestate,
        actiongivereward,
        actionchase,
        actionkickuser,
        actionescape,
        actionjointoteam,
        actionleaveteam,
        actiongiveteamscore,
        actioncallstacks,
        actionmovetodir,
        actionbotteleport,
        actionbotmove,
        actionbotwhisper,
        actionbotclothes,
        actionbotfollowavt,
        actionbothanditem,
        actionbottalk,
        actionmutetriggerer,
        actionmovetofurni,

        conditionfurnishaveusers,
        conditionstatepos,
        conditiontimelessthan,
        conditiontimemorethan,
        conditiontriggeronfurni,
        conditionhasfurnion,
        conditionactoringroup,
        conditionactorinteam,
        conditionusercountin,
        conditionstuffis,
        conditionwearingeffect,
        conditiondaterange,
        conditionwearingbadge,
        conditionhandleitemid,

        conditionnotfurnion,
        conditionnotfurnishaveusers,
        conditionnotingroup,
        conditionnotinteam,
        conditionnotstatepos,
        conditionnotstuffis,
        conditionnottriggeronfurni,
        conditionnotusercount,
        conditionnotwearingeffect,
        conditionnotdaterange,
        conditionnotwearingbadge,

        wiredClassification,

        arrowplate,
        preassureplate,
        ringplate,
        colortile,
        colorwheel,
        floorswitch1,
        floorswitch2,
        firegate,
        glassfoor,

        specialrandom,
        specialunseen,

        gift,
        maniqui,
        changeBackgrounds,
        bot,
        ads_mpu,
        badge_display,
        yttv,
        piñata,
        dalia,
        mutesignal,
        guildforum,
        piratecannon,
        waterbowl,
        pethomes,
        petfood,
        gnomebox,
        breedingpet,
        userslock,
        vikinghouse,
        guildgate,
        fxbox,
        photo,
        trampoline,
        treadmill,
        crosstrainer,
        tent,
        bigtent,
        wobench
    }

    class InterractionTypes
    {
        internal static InteractionType GetTypeFromString(string pType)
        {
            switch (pType)
            {
                case "":
                case "default":
                    return InteractionType.none;
                case "gate":
                    return InteractionType.gate;
                case "postit":
                    return InteractionType.postit;
                case "roomeffect":
                    return InteractionType.roomeffect;
                case "dimmer":
                    return InteractionType.dimmer;
                case "trophy":
                    return InteractionType.trophy;
                case "bed":
                    return InteractionType.bed;
                case "shower":
                    return InteractionType.shower;
                case "scoreboard":
                    return InteractionType.scoreboard;
                case "vendingmachine":
                    return InteractionType.vendingmachine;
                case "alert":
                    return InteractionType.alert;
                case "onewaygate":
                    return InteractionType.onewaygate;
                case "loveshuffler":
                    return InteractionType.loveshuffler;
                case "habbowheel":
                    return InteractionType.habbowheel;
                case "dice":
                    return InteractionType.dice;
                case "bottle":
                    return InteractionType.bottle;
                case "teleport":
                    return InteractionType.teleport;
                case "saltasalas":
                    return InteractionType.saltasalas;
                case "rentals":
                    return InteractionType.rentals;
                case "pet":
                    return InteractionType.pet;
                case "pool":
                    return InteractionType.pool;
                case "roller":
                    return InteractionType.roller;
                case "fbgate":
                    return InteractionType.fbgate;
                case "iceskates":
                    return InteractionType.iceskates;
                case "normalskates":
                    return InteractionType.normslaskates;
                case "snowboard":
                    return InteractionType.snowboard;
                case "lowpool":
                    return InteractionType.lowpool;
                case "haloweenpool":
                    return InteractionType.haloweenpool;
                case "haloweenpool15":
                    return InteractionType.haloweenpool15;
                case "football":
                    return InteractionType.football;
                case "balloon15":
                    return InteractionType.balloon15;
                case "craftable":
                    return InteractionType.craftable;
                case "horsejump":
                    return InteractionType.horsejump;
                case "seed":
                    return InteractionType.seed;
                case "fxprovider":
                    return InteractionType.fxprovider;

                case "footballgoalgreen":
                    return InteractionType.footballgoalgreen;
                case "footballgoalyellow":
                    return InteractionType.footballgoalyellow;
                case "footballgoalred":
                    return InteractionType.footballgoalred;
                case "footballgoalblue":
                    return InteractionType.footballgoalblue;

                case "footballcountergreen":
                    return InteractionType.footballcountergreen;
                case "footballcounteryellow":
                    return InteractionType.footballcounteryellow;
                case "footballcounterblue":
                    return InteractionType.footballcounterblue;
                case "footballcountered":
                    return InteractionType.footballcounterred;

                case "banzaigateblue":
                    return InteractionType.banzaigateblue;
                case "banzaigatered":
                    return InteractionType.banzaigatered;
                case "banzaigateyellow":
                    return InteractionType.banzaigateyellow;
                case "banzaigategreen":
                    return InteractionType.banzaigategreen;
                case "banzaifloor":
                    return InteractionType.banzaifloor;

                case "banzaiscoreblue":
                    return InteractionType.banzaiscoreblue;
                case "banzaiscorered":
                    return InteractionType.banzaiscorered;
                case "banzaiscoreyellow":
                    return InteractionType.banzaiscoreyellow;
                case "banzaiscoregreen":
                    return InteractionType.banzaiscoregreen;

                case "banzaitele":
                    return InteractionType.banzaitele;
                case "banzaipuck":
                    return InteractionType.banzaipuck;
                case "banzaipyramid":
                    return InteractionType.banzaipyramid;

                case "freezeexit":
                    return InteractionType.freezeexit;
                case "freezeredcounter":
                    return InteractionType.freezeredcounter;
                case "freezebluecounter":
                    return InteractionType.freezebluecounter;
                case "freezeyellowcounter":
                    return InteractionType.freezeyellowcounter;
                case "freezegreencounter":
                    return InteractionType.freezegreencounter;
                case "freezeyellowgate":
                    return InteractionType.freezeyellowgate;
                case "freezeredgate":
                    return InteractionType.freezeredgate;
                case "freezegreengate":
                    return InteractionType.freezegreengate;
                case "freezebluegate":
                    return InteractionType.freezebluegate;
                case "freezetileblock":
                    return InteractionType.freezetileblock;
                case "freezetile":
                    return InteractionType.freezetile;
                case "jukebox":
                    return InteractionType.jukebox;
                case "musicdisc":
                    return InteractionType.musicdisc;

                case "triggertimer":
                    return InteractionType.triggertimer;
                case "triggerroomenter":
                    return InteractionType.triggerroomenter;
                case "triggergameend":
                    return InteractionType.triggergameend;
                case "triggergamestart":
                    return InteractionType.triggergamestart;
                case "triggerrepeater":
                    return InteractionType.triggerrepeater;
                case "triggeronusersay":
                    return InteractionType.triggeronusersay;
                case "triggerscoreachieved":
                    return InteractionType.triggerscoreachieved;
                case "triggerstatechanged":
                    return InteractionType.triggerstatechanged;
                case "triggerwalkonfurni":
                    return InteractionType.triggerwalkonfurni;
                case "triggerwalkofffurni":
                    return InteractionType.triggerwalkofffurni;
                case "triggercollision":
                    return InteractionType.triggercollision;
                case "triggerlongperiodic":
                    return InteractionType.triggerlongperiodic;
                case "triggerbotreachedavtr":
                    return InteractionType.triggerbotreachedavtr;
                case "triggerbotreachedstf":
                    return InteractionType.triggerbotreachedstf;
                case "actiongivescore":
                    return InteractionType.actiongivescore;
                case "actionposreset":
                    return InteractionType.actionposreset;
                case "actionmoverotate":
                    return InteractionType.actionmoverotate;
                case "actionresettimer":
                    return InteractionType.actionresettimer;
                case "actionshowmessage":
                    return InteractionType.actionshowmessage;
                case "actionhandiitemcustom":
                    return InteractionType.actionhandiitemcustom;
                case "actioneffectcustom":
                    return InteractionType.actioneffectcustom;
                case "actiondiamantescustom":
                    return InteractionType.actiondiamantescustom;
                case "actiondancecustom":
                    return InteractionType.actiondancecustom;
                case "actionfastwalk":
                    return InteractionType.actionfastwalk;
                case "actionfreezecustom":
                    return InteractionType.actionfreezecustom;
                case "actionteleportto":
                    return InteractionType.actionteleportto;
                case "actiontogglestate":
                    return InteractionType.actiontogglestate;
                case "actiongivereward":
                    return InteractionType.actiongivereward;
                case "actionchase":
                    return InteractionType.actionchase;
                case "actionkickuser":
                    return InteractionType.actionkickuser;
                case "actionescape":
                    return InteractionType.actionescape;
                case "actionjointoteam":
                    return InteractionType.actionjointoteam;
                case "actionleaveteam":
                    return InteractionType.actionleaveteam;
                case "actiongiveteamscore":
                    return InteractionType.actiongiveteamscore;
                case "actioncallstacks":
                    return InteractionType.actioncallstacks;
                case "actionmovetodir":
                    return InteractionType.actionmovetodir;
                case "actionbotteleport":
                    return InteractionType.actionbotteleport;
                case "actionbotmove":
                    return InteractionType.actionbotmove;
                case "actionbotwhisper":
                    return InteractionType.actionbotwhisper;
                case "actionbotclothes":
                    return InteractionType.actionbotclothes;
                case "actionbotfollowavt":
                    return InteractionType.actionbotfollowavt;
                case "actionbothanditem":
                    return InteractionType.actionbothanditem;
                case "actionbottalk":
                    return InteractionType.actionbottalk;
                case "actionmutetriggerer":
                    return InteractionType.actionmutetriggerer;
                case "actionmovetofurni":
                    return InteractionType.actionmovetofurni;
                case "conditionfurnishaveusers":
                    return InteractionType.conditionfurnishaveusers;
                case "conditionstatepos":
                    return InteractionType.conditionstatepos;
                case "conditiontimelessthan":
                    return InteractionType.conditiontimelessthan;
                case "conditiontimemorethan":
                    return InteractionType.conditiontimemorethan;
                case "conditiontriggeronfurni":
                    return InteractionType.conditiontriggeronfurni;
                case "conditionhasfurnion":
                    return InteractionType.conditionhasfurnion;
                case "conditionactoringroup":
                    return InteractionType.conditionactoringroup;
                case "conditionactorinteam":
                    return InteractionType.conditionactorinteam;
                case "conditionusercountin":
                    return InteractionType.conditionusercountin;
                case "conditionstuffis":
                    return InteractionType.conditionstuffis;
                case "conditionwearingeffect":
                    return InteractionType.conditionwearingeffect;
                case "conditiondaterange":
                    return InteractionType.conditiondaterange;
                case "conditionhandleitemid":
                    return InteractionType.conditionhandleitemid;
                case "conditionnotfurnion":
                    return InteractionType.conditionnotfurnion;
                case "conditionnotfurnishaveusers":
                    return InteractionType.conditionnotfurnishaveusers;
                case "conditionnotingroup":
                    return InteractionType.conditionnotingroup;
                case "conditionnotinteam":
                    return InteractionType.conditionnotinteam;
                case "conditionnotstatepos":
                    return InteractionType.conditionnotstatepos;
                case "conditionnotstuffis":
                    return InteractionType.conditionnotstuffis;
                case "conditionnottriggeronfurni":
                    return InteractionType.conditionnottriggeronfurni;
                case "conditionnotusercount":
                    return InteractionType.conditionnotusercount;
                case "conditionnotwearingeffect":
                    return InteractionType.conditionnotwearingeffect;
                case "conditionnotdaterange":
                    return InteractionType.conditionnotdaterange;
                case "conditionwearingbadge":
                    return InteractionType.conditionwearingbadge;
                case "conditionnotwearingbadge":
                    return InteractionType.conditionnotwearingbadge;
                case "arrowplate":
                    return InteractionType.arrowplate;
                case "ringplate":
                    return InteractionType.ringplate;
                case "colortile":
                    return InteractionType.colortile;
                case "colorwheel":
                    return InteractionType.colorwheel;
                case "floorswitch1":
                    return InteractionType.floorswitch1;
                case "floorswitch2":
                    return InteractionType.floorswitch2;
                case "firegate":
                    return InteractionType.firegate;
                case "glassfoor":
                    return InteractionType.glassfoor;
                case "specialrandom":
                    return InteractionType.specialrandom;
                case "specialunseen":
                    return InteractionType.specialunseen;
                case "wiredClassification":
                    return InteractionType.wiredClassification;
                case "puzzlebox":
                    return InteractionType.puzzlebox;
                case "gift":
                    return InteractionType.gift;
                case "maniqui":
                    return InteractionType.maniqui;
                case "bgupdater":
                    return InteractionType.changeBackgrounds;
                case "bot":
                    return InteractionType.bot;
                case "ads_mpu":
                    return InteractionType.ads_mpu;
                case "badge_display":
                    return InteractionType.badge_display;
                case "yttv":
                    return InteractionType.yttv;
                case "piñata":
                    return InteractionType.piñata;
                case "dalia":
                    return InteractionType.dalia;
                case "mutesignal":
                    return InteractionType.mutesignal;
                case "guildforum":
                    return InteractionType.guildforum;
                case "piratecannon":
                    return InteractionType.piratecannon;
                case "waterbowl":
                    return InteractionType.waterbowl;
                case "pethomes":
                    return InteractionType.pethomes;
                case "breedingpet":
                    return InteractionType.breedingpet;
                case "petfood":
                    return InteractionType.petfood;
                case "userslock":
                    return InteractionType.userslock;
                case "vikinghouse":
                    return InteractionType.vikinghouse;
                case "guildgate":
                    return InteractionType.guildgate;
                case "gnomebox":
                    return InteractionType.gnomebox;
                case "fxbox":
                    return InteractionType.fxbox;
                case "photo":
                    return InteractionType.photo;
                case "trampoline":
                    return InteractionType.trampoline;
                case "treadmill":
                    return InteractionType.treadmill;
                case "crosstrainer":
                    return InteractionType.crosstrainer;
                case "tent":
                    return InteractionType.tent;
                case "bigtent":
                    return InteractionType.bigtent;
                case "wobench":
                    return InteractionType.wobench;
                case "preassureplate":
                    return InteractionType.preassureplate;
                default:
                    {
                        Console.WriteLine("Unknown interaction type in parse code: " + pType);
                        return InteractionType.none;
                    }
            }
        }
    }
}