using System;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;
using ButterStorm;
using Butterfly.Core;
using Butterfly.HabboHotel.Filter;

namespace Butterfly.HabboHotel.RoomBots
{
    class GenericBot : BotAI
    {
        private int SpeechTimer;
        private int ActionTimer;
        private int FollowTimer;

        internal GenericBot(int VirtualId)
        {
            SpeechTimer = new Random((VirtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 40);
            ActionTimer = new Random((VirtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 30);
            FollowTimer = 0;
        }

        internal override void OnSelfEnterRoom()
        {

        }

        internal override void OnSelfLeaveRoom(bool Kicked)
        {

        }

        internal override void OnUserEnterRoom(RoomUser User)
        {

        }

        internal override void OnUserLeaveRoom(GameClient Client)
        {

        }

        internal override void OnUserSay(RoomUser User, string Message)
        {
            if (GetBotData() == null || GetBotData().AiType != AIType.Waiter)
                return;

            #region Switch Message
            switch (Message.ToLower())
            {
                case "rosa":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1000);
                        break;
                    }

                case "rosa negra":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1001);
                        break;
                    }

                case "girasol":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1002);
                        break;
                    }

                case "libro rojo":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1003);
                        break;
                    }

                case "libro azul":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1004);
                        break;
                    }

                case "libro verde":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1005);
                        break;
                    }

                case "flor de regalo":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1006);
                        break;
                    }

                case "estramonio":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1007);
                        break;
                    }

                case "placer amarillo":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1008);
                        break;
                    }

                case "pandemia rosa":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1009);
                        break;
                    }

                case "sujetapapeles":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1011);
                        break;
                    }

                case "pildoras":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1013);
                        break;
                    }

                case "jeringuilla":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1014);
                        break;
                    }

                case "bolsa de residuos toxicos":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1015);
                        break;
                    }

                case "flor bolly":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1019);
                        break;
                    }

                case "jacinto 1":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1021);
                        break;
                    }

                case "jacinto 2":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1022);
                        break;
                    }

                case "flor de pascua":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1023);
                        break;
                    }

                case "pudding":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1024);
                        break;
                    }

                case "baston de caramelo":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1025);
                        break;
                    }

                case "regalo":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1026);
                        break;
                    }

                case "vela":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1027);
                        break;
                    }

                case "pavo":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(105);
                        break;
                    }

                case "tostada":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(106);
                        break;
                    }

                case "chicle azul":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(92);
                        break;
                    }

                case "chicle rojo":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(93);
                        break;
                    }

                case "chicle verde":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(94);
                        break;
                    }

                case "hipad":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1030);
                        break;
                    }

                case "antorcha habbo-lympix":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1031);
                        break;
                    }

                case "comandante tom":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1032);
                        break;
                    }

                case "ovni":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1033);
                        break;
                    }

                case "cosa alienigena":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1034);
                        break;
                    }

                case "llave inglesa":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1035);
                        break;
                    }

                case "patito de goma":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1036);
                        break;
                    }

                case "serpiente":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1037);
                        break;
                    }

                case "palo":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1038);
                        break;
                    }

                case "mano cortada":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1039);
                        break;
                    }

                case "corazon":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1040);
                        break;
                    }

                case "calamar":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1041);
                        break;
                    }

                case "excremento de murcielago":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1042);
                        break;
                    }

                case "gusano":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1043);
                        break;
                    }

                case "rata muerta":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1044);
                        break;
                    }

                case "dentaduras":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1045);
                        break;
                    }

                case "martillo":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1049);
                        break;
                    }

                case "pincel":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1051);
                        break;
                    }

                case "globo naranja":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1054);
                        break;
                    }

                case "globo verde":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1055);
                        break;
                    }

                case "globo azul":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1056);
                        break;
                    }

                case "globo rosa":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(1057);
                        break;
                    }

                case "te":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(10);
                        break;
                    }

                case "mocha":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(11);
                        break;
                    }

                case "helado de fresa":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(128);
                        break;
                    }

                case "helado de menta choc":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(129);
                        break;
                    }

                case "macchiato":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(12);
                        break;
                    }

                case "helado de chocolate":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(130);
                        break;
                    }

                case "espresso":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(13);
                        break;
                    }

                case "huevo de pascua luminiscente":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(141);
                        break;
                    }

                case "filtro":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(14);
                        break;
                    }

                case "chocolate caliente":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(15);
                        break;
                    }

                case "cappuccino":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(16);
                        break;
                    }

                case "java":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(17);
                        break;
                    }

                case "grifo":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(18);
                        break;
                    }

                case "habbo cola":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(19);
                        break;
                    }

                case "camara":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(20);
                        break;
                    }

                case "hamburguesa":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(21);
                        break;
                    }

                case "habbo soda de lima":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(22);
                        break;
                    }

                case "habbo soda de remolacha":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(23);
                        break;
                    }

                case "refresco burbujeante de 1978":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(24);
                        break;
                    }

                case "brebaje del amor":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(25);
                        break;
                    }

                case "calippo":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(26);
                        break;
                    }

                case "sake":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(28);
                        break;
                    }

                case "zumo de tomate":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(29);
                        break;
                    }

                case "zumo":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(2);
                        break;
                    }

                case "liquido radioactivo":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(30);
                        break;
                    }

                case "champin":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(31);
                        break;
                    }

                case "pescado fresco":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(34);
                        break;
                    }

                case "pera":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(36);
                        break;
                    }

                case "manzana":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(37);
                        break;
                    }

                case "naranja":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(38);
                        break;
                    }

                case "rodaja de piña":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(39);
                        break;
                    }

                case "zanahoria":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(3);
                        break;
                    }

                case "sumppi-kuppi":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(41);
                        break;
                    }

                case "agua galactica":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(44);
                        break;
                    }

                case "brebaje malhumor":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(46);
                        break;
                    }

                case "chupa chups":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(48);
                        break;
                    }

                case "helado de vainilla":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(4);
                        break;
                    }

                case "pipas g":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(51);
                        break;
                    }

                case "cheetos":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(52);
                        break;
                    }

                case "chocapic":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(54);
                        break;
                    }

                case "botella de pepsi":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(55);
                        break;
                    }

                case "zumo de uva":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(57);
                        break;
                    }

                case "copa de sangre":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(58);
                        break;
                    }

                case "leche":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(5);
                        break;
                    }

                case "castañas":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(60);
                        break;
                    }

                case "sunny":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(61);
                        break;
                    }

                case "palomitas":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(63);
                        break;
                    }

                case "batido de banana":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(66);
                        break;
                    }

                case "grosella":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(6);
                        break;
                    }

                case "muslo de pollo":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(70);
                        break;
                    }

                case "ponche de huevo":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(73);
                        break;
                    }

                case "copa de brindis":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(74);
                        break;
                    }

                case "helado de anis":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(76);
                        break;
                    }

                case "algodon de azucar rosa":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(79);
                        break;
                    }

                case "agua":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(7);
                        break;
                    }

                case "algodon de azucar azul":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(80);
                        break;
                    }

                case "perrito caliente":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(81);
                        break;
                    }

                case "manzana envenenada":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(83);
                        break;
                    }

                case "galleta de jengibre":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(84);
                        break;
                    }

                case "cafe solo":
                    {
                        string BotMessage = "¡Por supuesto!";
                        if (new Random().Next(0, 2) == 1)
                            BotMessage = "Por ti, " + User.GetUsername() + ", lo haré.";

                        GetRoomUser().Chat(null, BotMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);

                        GetRoomUser().MoveTo(User.SquareInFront);

                        User.CarryItem(8);
                        break;
                    }
            }
            #endregion
        }

        internal override void OnUserShout(RoomUser User, string Message)
        {

        }

        internal override void OnTimerTick()
        {
            if (GetBotData() == null)
                return;

            if (SpeechTimer <= 0)
            {
                if (GetBotData().RandomSpeech.Count > 0 && GetBotData().ChatEnabled)
                {
                    RandomSpeech Speech = GetBotData().GetRandomSpeech();
                    string specialMessage = BlackWordsManager.SpecialReplace(Speech.Message, GetRoomUser());

                    GetRoomUser().Chat(null, specialMessage, OtanixEnvironment.GetGame().GetRoomRankConfig().BOTS_DEFAULT_COLOR, false);
                }

                try
                {
                    SpeechTimer = GetBotData().ChatSeconds * 2;
                }
                catch (Exception e)
                {
                    Logging.LogThreadException(e.ToString(), "Error in SpeechTimer Bot: (" + GetBotData().ChatSeconds + "): ");
                    SpeechTimer = 30;
                }
            }
            else
            {
                SpeechTimer--;
            }

            if (ActionTimer <= 0)
            {
                if (GetBotData().WalkingEnabled && GetBotData().followingUser == null)
                {
                    var nextCoord = GetRoom().GetGameMap().getRandomWalkableSquare();
                    GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
                    GetRoomUser().needSqlUpdate = true;
                }

                ActionTimer = new Random().Next(1, 30);
            }
            else
            {
                ActionTimer--;
            }

            if (FollowTimer <= 0)
            {
                if (GetBotData().followingUser != null)
                {
                    RoomUser user = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(GetBotData().followingUser.HabboId);
                    if (user != null)
                    {
                        if (GetRoom().GetGameMap().tileIsWalkable(user.SquareInFront.X, user.SquareInFront.Y, true))
                        {
                            GetRoomUser().MoveTo(GetBotData().followingUser.SquareInFront);
                        }
                        else if(GetRoomUser().Coordinate != GetBotData().followingUser.SquareInFront)
                        {
                            GetRoomUser().MoveTo(GetBotData().followingUser.SquareBehind);
                        }

                        //GetRoomUser().MoveTo(GetBotData().followingUser.SquareInFront);
                        GetRoomUser().needSqlUpdate = true;
                    }
                }
                else
                {
                    GetBotData().followingUser = null;
                }

                FollowTimer = 3;
            }
            else
            {
                FollowTimer--;
            }
        }
    }
}