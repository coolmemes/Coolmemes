using System.Collections.Generic;
using System.IO;
using System.Text;
using Butterfly.HabboHotel.Misc;
using Butterfly.HabboHotel.GameClients;
using System;

namespace Butterfly.Messages
{
    class ChatCommandRegister
    {
        private static Dictionary<string, ChatCommand> commandRegisterInvokeable;

        internal static void Init()
        {
            commandRegisterInvokeable = new Dictionary<string, ChatCommand>();

            var texts = File.ReadAllLines(@"System/commands.ini");
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].StartsWith("#") || texts[i].Length < 1)
                    continue;

                if (texts[i].Contains("[") && texts[i].Contains("]")) // New Command
                {
                    foreach (string command in texts[i + 3].Split('=')[1].Split(','))
                    {
                        commandRegisterInvokeable.Add(command, new ChatCommand(command, int.Parse(texts[i].Split('=')[1].Replace("]","")), int.Parse(texts[i + 1].Split('=')[1]), texts[i + 2].Split('=')[1]));
                    }

                    i = i + 3;
                }
            }
        }

        internal static bool IsChatCommand(string command)
        {
            return commandRegisterInvokeable.ContainsKey(command);
        }

        internal static ChatCommand GetCommand(string command)
        {
            return commandRegisterInvokeable[command];
        }

        internal static string GenerateCommandList(GameClient client)
        {
            StringBuilder comandosRanks = new StringBuilder();
            StringBuilder comandosDonoQuarto = new StringBuilder();
            StringBuilder comandosDireitosQuarto = new StringBuilder();

            comandosDonoQuarto.Append("Comandos por ser dono do quarto: \r\r");
            comandosDireitosQuarto.Append("\r\rComandos por ter direitos no quarto: \r\r");
            comandosRanks.Append("\rComandos por ter rank necessário: \r\r");
            foreach (var command in commandRegisterInvokeable.Values)
            {
                if (command.UserGotAuthorization(client))
                {
                    //\r----------------------------------------------------------------------------
                    if (command.AuthorizedRanks == -2)
                        comandosDonoQuarto.Append(":" + command.Command + " -> " + command.Description + "\r·····················································································\r");
                    else if (command.AuthorizedRanks == -1)
                        comandosDireitosQuarto.Append(":" + command.Command + " -> " + command.Description + "\r·····················································································\r");
                    else
                        comandosRanks.Append(":" + command.Command + " -> " + command.Description + "\r·····················································································\r");
                }
            }

            return new StringBuilder().Append(comandosDonoQuarto).Append(comandosDireitosQuarto).Append(comandosRanks).ToString();
        }

        internal static void InvokeCommand(ChatCommandHandler commandHandler, int commandId)
        {
            try
            {
                switch (commandId)
                {
                    case 1:
                        {
                            commandHandler.pickall();
                            break;
                        }

                    case 2:
                        {
                            commandHandler.pickpets();
                            break;
                        }

                    case 3:
                        {
                            commandHandler.pickbots();
                            break;
                        }

                    case 4:
                        {
                            commandHandler.ejectall();
                            break;
                        }

                    case 6:
                        {
                            commandHandler.teleport();
                            break;
                        }

                    case 7:
                        {
                            commandHandler.rkick();
                            break;
                        }

                    case 8:
                        {
                            commandHandler.reload();
                            break;
                        }

                    case 9:
                        {
                            commandHandler.setmax();
                            break;
                        }

                    case 10:
                        {
                            commandHandler.fixroom();
                            break;
                        }

                    case 11:
                        {
                            commandHandler.room();
                            break;
                        }

                    case 12:
                        {
                            commandHandler.setspeed();
                            break;
                        }

                    case 14:
                        {
                            commandHandler.mutepets();
                            break;
                        }

                    case 15:
                        {
                            commandHandler.mutebots();
                            break;
                        }

                    case 16:
                        {
                            commandHandler.sit();
                            break;
                        }

                    case 17:
                        {
                            commandHandler.lay();
                            break;
                        }
                    case 19:
                        {
                            commandHandler.dance();
                            break;
                        }

                    case 21:
                        {
                            commandHandler.stand();
                            break;
                        }

                    case 22:
                        {
                            commandHandler.handitem();
                            break;
                        }

                    case 23:
                        {
                            commandHandler.enable();
                            break;
                        }

                    case 24:
                        {
                            commandHandler.commands();
                            break;
                        }

                    case 25:
                        {
                            commandHandler.empty();
                            break;
                        }

                    case 26:
                        {
                            commandHandler.emptypets();
                            break;
                        }

                    case 27:
                        {
                            commandHandler.emptybots();
                            break;
                        }

                    case 28:
                        {
                            commandHandler.follow();
                            break;
                        }

                    case 29:
                        {
                            commandHandler.moonwalk();
                            break;
                        }

                    case 30:
                        {
                            commandHandler.copylook();
                            break;
                        }

                    case 31:
                        {
                            commandHandler.convertToPet();
                            break;
                        }

                    case 32:
                        {
                            commandHandler.ClearConsole();
                            break;
                        }

                    case 33:
                        {
                            commandHandler.push();
                            break;
                        }

                    case 34:
                        {
                            commandHandler.pull();
                            break;
                        }

                    case 35:
                        {
                            commandHandler.kill();
                            break;
                        }

                    case 36:
                        {
                            commandHandler.personal();
                            break;
                        }

                    case 38:
                        {
                            commandHandler.userinfo();
                            break;
                        }

                    case 39:
                        {
                            commandHandler.ban();
                            break;
                        }

                    case 41:
                        {
                            commandHandler.superban();
                            break;
                        }

                    case 42:
                        {
                            commandHandler.muteuser();
                            break;
                        }

                    case 43:
                        {
                            commandHandler.unmuteuser();
                            break;
                        }

                    case 44:
                        {
                            commandHandler.roomalert();
                            break;
                        }

                    case 45:
                        {
                            commandHandler.roommute();
                            break;
                        }

                    case 46:
                        {
                            commandHandler.roomkick();
                            break;
                        }

                    case 47:
                        {
                            commandHandler.disconnect();
                            break;
                        }

                    case 48:
                        {
                            commandHandler.come();
                            break;
                        }

                    case 49:
                        {
                            commandHandler.staffmessages();
                            break;
                        }

                    case 50:
                        {
                            commandHandler.openroom();
                            break;
                        }

                    case 51:
                        {
                            commandHandler.overridee();
                            break;
                        }

                    case 52:
                        {
                            commandHandler.Fly();
                            break;
                        }

                    case 53:
                        {
                            commandHandler.giveBadge();
                            break;
                        }

                    case 54:
                        {
                            commandHandler.removeBadge();
                            break;
                        }

                    case 55:
                        {
                            commandHandler.roombadge();
                            break;
                        }

                    case 56:
                        {
                            commandHandler.eha();
                            break;
                        }

                    case 57:
                        {
                            commandHandler.eventha();
                            break;
                        }

                    case 58:
                        {
                            commandHandler.staffDiamonds();
                            break;
                        }

                    case 59:
                        {
                            commandHandler.hotelalert();
                            break;
                        }

                    case 60:
                        {
                            commandHandler.pha();
                            break;
                        }

                    case 61:
                        {
                            commandHandler.visa();
                            break;
                        }

                    case 62:
                        {
                            commandHandler.giveDiamonds();
                            break;
                        }

                    case 63:
                        {
                            commandHandler.roomDiamonds();
                            break;
                        }

                    case 64:
                        {
                            commandHandler.roomaction();
                            break;
                        }

                    case 65:
                        {
                            commandHandler.massbadge();
                            break;
                        }

                    case 66:
                        {
                            commandHandler.unban();
                            break;
                        }

                    case 67:
                        {
                            commandHandler.staffinfo();
                            break;
                        }

                    case 68:
                        {
                            commandHandler.linkAlert();
                            break;
                        }

                    case 69:
                        {
                            commandHandler.QuickPoll();
                            break;
                        }

                    case 70:
                        {
                            commandHandler.viewinventary();
                            break;
                        }

                    case 71:
                        {
                            commandHandler.addFilter();
                            break;
                        }

                    case 72:
                        {
                            commandHandler.say();
                            break;
                        }

                    case 73:
                        {
                            commandHandler.massfurni();
                            break;
                        }

                    case 74:
                        {
                            commandHandler.globalDiamonds();
                            break;
                        }

                    case 75:
                        {
                            commandHandler.invisible();
                            break;
                        }

                    case 76:
                        {
                            commandHandler.getStaffs();
                            break;
                        }

                    case 77:
                        {
                            commandHandler.usersOnRooms();
                            break;
                        }

                    case 78:
                        {
                            commandHandler.developerFurnis();
                            break;
                        }

                    case 79:
                        {
                            commandHandler.refresh();
                            break;
                        }

                    case 80:
                        {
                            commandHandler.info();
                            break;
                        }

                    case 83:
                        {
                            commandHandler.notification();
                            break;
                        }

                    case 84:
                        {
                            commandHandler.goevent();
                            break;
                        }

                    case 85:
                        {
                            commandHandler.spyuser();
                            break;
                        }
                    case 86:
                        {
                            commandHandler.ganhouEvento();
                            break;
                        }

                    case 87:
                        {
                            commandHandler.explicaEvento();
                            break;
                        }

                    case 88:
                        {
                            commandHandler.updates();
                            break;
                        }
                    case 89:
                        {
                            commandHandler.shutdown();
                            break;
                        }
                    case 90:
                        {
                            commandHandler.freezeuser();
                            break;
                        }
                    case 91:
                        {
                            commandHandler.fastwalk();
                            break;
                        }
                    case 92:
                        {
                            commandHandler.premium();
                            break;
                        }
                    case 93:
                        {
                            commandHandler.pegaGeral();
                            break;
                        }
                    case 94:
                        {
                            commandHandler.prisao();
                            break;
                        }
                    case 95:
                        {
                            commandHandler.globalMoedas();
                            break;
                        }
                    case 96:
                        {
                            commandHandler.darMoedas();
                            break;
                        }
                    case 97:
                        {
                            commandHandler.roomMoedas();
                            break;
                        }
                    case 98:
                        {
                            commandHandler.bundle();
                            break;
                        }
                    case 99:
                        {
                            commandHandler.cor();
                            break;
                        }
                    case 100:
                        {
                            commandHandler.givePiruleta();
                            break;
                        }

                    case 101:
                        {
                            commandHandler.giveRoomPiruleta();
                            break;
                        }
                    case 102:
                        {
                            commandHandler.tamanhoChao();
                            break;
                        }
                }
            }
            catch (Exception e){ commandHandler.GetClient().SendNotif("Comando mal formado ou com erros." + e.GetBaseException()); }
        }
    }
}
