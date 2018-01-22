using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Butterfly.Core;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;
using Butterfly.HabboHotel.Rooms.RoomIvokedItems;
using Butterfly.Messages;
using ButterStorm;
using System.Drawing;
using Butterfly.Util;
using HabboEvents;
using Database_Manager.Database.Session_Details.Interfaces;
using Butterfly.HabboHotel.Users;
using System.Data;
using Butterfly.HabboHotel.Support;
using Butterfly.HabboHotel.Pets;
using Butterfly.HabboHotel.Filter;
using Butterfly.HabboHotel.Rooms.Polls;
using Butterfly.HabboHotel.Items;
using Butterfly.HabboHotel.RoomBots;
using Butterfly.HabboHotel.SoundMachine;
using Butterfly.HabboHotel.ChatMessageStorage;
using System.Net;
using System.IO;
using Butterfly.HabboHotel.Premiums;
using Butterfly.HabboHotel.Users.Prisao;
using Butterfly.HabboHotel.Navigators.Bonus;
using Plus.HabboHotel.Catalog.Bundles;

namespace Butterfly.HabboHotel.Misc
{
    class ChatCommandHandler
    {
        private Room TargetRoom;
        private RoomUser TargetRoomUser;
        private GameClient Session;
        private readonly string[] Params;

        public ChatCommandHandler(string[] input, GameClient session, Room Room, RoomUser RoomUser)
        {
            Params = input;
            Session = session;
            TargetRoom = Room;
            TargetRoomUser = RoomUser;
        }

        internal GameClient GetClient()
        {
            return Session;
        }

        public bool WasExecuted()
        {
            // Cargamos el Manager del comando.
            ChatCommand command = ChatCommandRegister.GetCommand(Params[0].Substring(1).ToLower());

            // Si este comando está desactivado sobre la sala.
            if (TargetRoom.RoomData.DisabledCommands.Contains(command.CommandId))
            {
                // Informamos al usuario sobre la desactivación del comando.
                TargetRoomUser.WhisperComposer("[Otanix] @ Alerta: O comando " + command.Command + " está desativado.");
                return true;
            }

            // Si el usuario puede ejecutar el comando.
            if (command.UserGotAuthorization(Session))
            {
                ChatCommandRegister.InvokeCommand(this, command.CommandId);
                Dispose();
                return true;
            }
            else
            {
                Dispose();
                return false;
            }
        }

        public void Dispose()
        {
            // Limpiamos memoria.
            Session = null;
            Array.Clear(Params, 0, Params.Length);
        }

        internal static string MergeParams(string[] Params, int Start, string Character = " ")
        {
            StringBuilder MergedParams = new StringBuilder();

            for (int i = 0; i < Params.Length; i++)
            {
                if (i < Start)
                    continue;

                if (i > Start)
                    MergedParams.Append(Character);

                MergedParams.Append(Params[i]);
            }

            return MergedParams.ToString();
        }

        #region Commands
        public void pickall()
        {
            // Si se ha cargado, el usuario tiene derechos de dueño y la sala no está en la lista de constantes.
            if (OtanixEnvironment.GetGame().GetRoomRankConfig().ROOMS_TO_MODIFY.Contains((int)TargetRoom.Id))
                return;

            // Quita todos los furnis de la sala.
            TargetRoom.GetRoomItemHandler().RemoveAllFurniture(Session);

            // Si es un Staff, entonces se loguea el comando.
            if (Session.GetHabbo().HasFuse("fuse_any_room_rights"))
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, TargetRoom.RoomData.Name + "<" + TargetRoom.RoomData.Id + ">", "Pickall", "Pickall Room <" + TargetRoom.Id + ">");
        }

        public void pickpets()
        {
            // Obtenemos la lista de mascotas en sala.
            List<Pet> Pets = TargetRoom.GetRoomUserManager().GetPets();

            foreach (Pet Pet in Pets)
            {
                // Si no es su dueño, se queda.
                if (Pet.OwnerId != Session.GetHabbo().Id)
                    continue;

                // Marcamos que necesita actualización.
                Pet.RoomId = 0;
                Pet.DBState = DatabaseUpdateState.NeedsUpdate;
            }

            // Guardamos en SQL.
            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
            {
                TargetRoom.GetRoomUserManager().SavePets(dbClient);
            }

            // Quitamos a los usuarios de la sala.
            foreach (Pet Pet in Pets)
            {
                // Si no es su dueño, se queda.
                if (Pet.OwnerId != Session.GetHabbo().Id)
                    continue;

                // Añadimos la mascota al inventario.
                Session.GetHabbo().GetInventoryComponent().AddPet(Pet);

                // Quitamos la mascota de la sala.
                TargetRoom.GetRoomUserManager().RemoveBot(Pet.VirtualId, false);
            }

            // Actualizamos el inventario.
            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
        }

        public void shutdown()
        {
            OtanixEnvironment.PreformShutDown();
            OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Shutdown", "Desligou o servidor");
        }

        public void pickbots()
        {
            // Obtenemos la lista de mascotas en sala.
            List<RoomUser> Bots = TargetRoom.GetRoomUserManager().GetBots;

            foreach (RoomUser Bot in Bots)
            {
                // Si no es su dueño, se queda.
                if (Bot.BotData.OwnerId != Session.GetHabbo().Id)
                    continue;

                RoomBot BotData = Bot.BotData;
                if (BotData == null)
                    continue;

                // Actualizamos variables.
                BotData.RoomId = 0;
                BotData.X = 0;
                BotData.Y = 0;

                // Guardamos en SQL.
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor()) // Super Stable Method [RELEASE 135]
                {
                    dbClient.runFastQuery("REPLACE INTO items_users VALUES (" + BotData.BotId + "," + BotData.OwnerId + ")");
                    dbClient.runFastQuery("DELETE FROM items_rooms WHERE item_id = " + BotData.BotId);

                    dbClient.setQuery("UPDATE bots SET name = @botname, is_dancing = '" + ((Bot.IsDancing) ? "1" : "0") + "', walk_enabled = '" + ((BotData.WalkingEnabled) ? "1" : "0") + "', chat_enabled = '" + ((BotData.ChatEnabled) ? "1" : "0") + "', chat_text = @chttext, chat_seconds = '" + BotData.ChatSeconds + "', look = @look, gender = @gender, x = " + BotData.X + ", y = " + BotData.Y + " WHERE id = " + BotData.BotId);
                    dbClient.addParameter("look", BotData.Look);
                    dbClient.addParameter("gender", BotData.Gender);
                    dbClient.addParameter("chttext", BotData.ChatText);
                    dbClient.addParameter("botname", BotData.Name);
                    dbClient.runQuery();
                }

                // Añadimos el bot al inventario.
                Session.GetHabbo().GetInventoryComponent().AddBot(Bot.BotData);

                // Quitamos la mascota de la sala.
                TargetRoom.GetRoomUserManager().RemoveBot(Bot.VirtualId, false);
            }

            // Actualizamos el inventario.
            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializeBotInventory());
        }

        public void ejectall()
        {
            // Si se ha cargado, el usuario tiene derechos de dueño y la sala no está en la lista de constantes.
            if (OtanixEnvironment.GetGame().GetRoomRankConfig().ROOMS_TO_MODIFY.Contains((int)TargetRoom.Id))
                return;

            // Quita todos los furnis de la sala.
            TargetRoom.GetRoomItemHandler().RemoveUserFurniture(Session);

            // Si es un Staff, entonces se loguea el comando.
            if (Session.GetHabbo().HasFuse("fuse_any_room_rights"))
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, TargetRoom.RoomData.Name + "<" + TargetRoom.RoomData.Id + ">", "Ejectall", "Ejectall Room <" + TargetRoom.Id + ">");
        }

        public void rkick()
        {
            // Comprueba si hay un mensaje al expulsar.
            string ModMsg = "";

            // Creamos la variable de Kick.
            RoomKick kick = new RoomKick(ModMsg, (int)Session.GetHabbo().Rank);

            // Echamos a los usuarios.
            TargetRoom.QueueRoomKick(kick);

            // Si es un Staff, entonces se loguea el comando.
            if (Session.GetHabbo().HasFuse("fuse_any_room_rights"))
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Room kick", "Kicked the whole room");
        }

        public void roomkick()
        {
            // Comprueba si hay un mensaje al expulsar.
            string ModMsg = ModMsg = MergeParams(Params, 1);

            // Creamos la variable de Kick.
            RoomKick kick = new RoomKick(ModMsg, (int)Session.GetHabbo().Rank);

            // Echamos a los usuarios.
            TargetRoom.QueueRoomKick(kick);

            // Si es un Staff, entonces se loguea el comando.
            if (Session.GetHabbo().HasFuse("fuse_any_room_rights"))
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Room kick", "Kicked the whole room");
        }

        public void reload()
        {

            RoomUser UserFinal = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (UserFinal != null && UserFinal.montandoBol == true || UserFinal.montandoID != 0)
            {
                UserFinal.montandoBol = false;
                UserFinal.montandoID = 0;
            }

            // Creamos una copia de los usuarios de sala.
            List<RoomUser> users = new List<RoomUser>(TargetRoom.GetRoomUserManager().UserList.Values);

            // Deslogueamos la sala.
            OtanixEnvironment.GetGame().GetRoomManager().UnloadRoom(TargetRoom);

            // Introducimos a los usuarios de nuevo.
            if (users != null && users.Count > 0)
            {
                foreach (RoomUser user in users)
                {
                    if (user != null && user.GetClient() != null)
                        user.GetClient().GetMessageHandler().enterOnRoom3(TargetRoom);
                }
            }

            // Limpiamos la variable de la caché.
            users.Clear();
            users = null;
            OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, TargetRoom.RoomData.Name + "<" + TargetRoom.RoomData.Id + ">", "Reload", "Recarregou o quarto");
        }

        public void setmax()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Creamos la variable que contiene el número de usuarios de la sala.
                uint MaxUsers = 0;

                // Comprobamos que el valor sea numérico.
                if (!uint.TryParse(Params[1], out MaxUsers))
                {
                    Session.SendNotif(LanguageLocale.GetValue("input.intonly"));
                    return;
                }

                // Si son 0 usuarios, no permitimos modificarlo.
                if (MaxUsers == 0)
                    return;

                // Establecemos los límites de usuarios en sala.
                if ((MaxUsers > 100 && Session.GetHabbo().Rank == 1) || MaxUsers > 300)
                {
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("setmax.maxusersreached"));
                }
                else
                {
                    // Ampliamos la sala.
                    TargetRoom.SetMaxUsers(MaxUsers);
                    OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, TargetRoom.RoomData.Name + "<" + TargetRoom.RoomData.Id + ">", "Setmax", "Aumentou a capacidade do quarto");
                }
            }
        }

        public void fixroom()
        {
            // Fixeamos la sala.
            TargetRoom.FixGameMap();     
        }

        public void goevent()
        {
            // Se tem um evento no hotel
            if (OtanixEnvironment.GetGame().RoomIdEvent > 0)
            {
                // Verificamos se o usuário já está no evento
                if (Session.GetHabbo().CurrentRoomId == OtanixEnvironment.GetGame().RoomIdEvent)
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("eventos.jaestanasala"));
                else // Enviamos o usuario para a sala.
                Session.GetMessageHandler().PrepareRoomForUser(OtanixEnvironment.GetGame().RoomIdEvent, "");
            }
            else
            {
                // Notificamos al usuario.
                TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("eventos.finalizado"));
            }
        }

        public void room()
        {
            // Si hay otro parámetro.
            if (Params.Length > 1)
            {
                string CommandOrder = Params[1];
                if (CommandOrder == "room")
                    return;

                switch (CommandOrder)
                {
                    case "setTile":
                        {
                            var point = new Point(TargetRoomUser.X, TargetRoomUser.Y);
                            TargetRoom.GetGameManager().GetBanzai().HandleBanzaiTiles(point, TargetRoomUser.team, TargetRoomUser);
                            TargetRoomUser.WhisperComposer("Setou o piso com sucesso");
                            break;
                        }
                    case "diagonal":
                        {
                            // Activa/Desactiva el movimiento diagonal en la sala.
                            TargetRoom.RoomData.AllowDiagonalEnabled = !TargetRoom.RoomData.AllowDiagonalEnabled;
                            TargetRoom.RoomData.roomNeedSqlUpdate = true;

                            // El usuario recibe una notificación mediante susurro.
                            TargetRoomUser.WhisperComposer("Você " + (TargetRoom.RoomData.AllowDiagonalEnabled ? "ativou" : "desativou") + " a diagonal no quarto.");

                            break;
                        }

                    case "push":
                    case "pull":
                    case "kill":
                    case "moonwalk":
                    case "enable":
                    case "handitem":
                    case "copy":
                    case "sit":
                    case "lay":
                    case "pet":
                        {
                            // Comprobamos que el comando exista.
                            if (ChatCommandRegister.IsChatCommand(CommandOrder))
                            {
                                // Cargamos la información del comando.
                                ChatCommand Comando = ChatCommandRegister.GetCommand(CommandOrder);

                                // Si el comando es para usuarios con rango, retorna.
                                /*if (Comando.MinRank > 1)
                                {
                                    TargetRoomUser.WhisperComposer("No está permitido desactivar comandos con rango.");
                                    return;
                                }*/

                                // Añadimos/Eliminamos el comando de la sala.
                                if (!TargetRoom.RoomData.DisabledCommands.Contains(Comando.CommandId))
                                {
                                    TargetRoom.RoomData.DisabledCommands.Add(Comando.CommandId);

                                    // El usuario recibe una notificación mediante susurro.
                                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("comandos.desativar1") + CommandOrder + LanguageLocale.GetValue("comandos.desativar2"));
                                }
                                else
                                {
                                    TargetRoom.RoomData.DisabledCommands.Remove(Comando.CommandId);

                                    // El usuario recibe una notificación mediante susurro.
                                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("comandos.desativar3") + CommandOrder + LanguageLocale.GetValue("comandos.desativar2"));
                                }

                                // Notificamos al emu que esta sala necesita actualizar el SQL.
                                TargetRoom.RoomData.roomNeedSqlUpdate = true;
                            }
                            else
                            {
                                // El usuario recibe una notificación mediante susurro.
                                TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("comandos.inválido"));
                            }

                            break;
                        }

                    default:
                        {
                            Session.SendWindowManagerAlert(LanguageLocale.GetValue("comandos.quartos.errado") + ":room\":\n\r - diagonal\n - push\n - pull\n - kill\n - moonwalk\n - enable\n - handitem\n - copy\n - sit\n - lay\n - pet");

                            break;
                        }
                }
            }
        }

        public void prisao()
        {
            // Verifica se tem 2 parametros
            if (Params.Length > 1)
            {
                // Comando.
                string CommandOrder = Params[1];

                switch (CommandOrder)
                {
                    case "add":
                        {
                            if (Params[2] == null || Params[3] == null)
                                return;
                            // Pegamos o usuário
                            GameClient Usuario = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[2]);

                            if (Usuario == null || Usuario.GetHabbo() == null)
                            {
                                TargetRoomUser.WhisperComposer("Usuário não existe ou não está conectado.");
                                return;
                            }


                            int quantidadeHorasFinais;
                            bool qtdCorreta = int.TryParse(Params[3], out quantidadeHorasFinais);

                            if (!qtdCorreta)
                            {
                                TargetRoomUser.WhisperComposer("Coloque apenas números");
                                return;
                            }

                            if (quantidadeHorasFinais < 1 || quantidadeHorasFinais > 1000)
                                quantidadeHorasFinais = 1000;

                            double quantidadeHoras = (quantidadeHorasFinais * 3600) + OtanixEnvironment.GetUnixTimestamp();
                            OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, Usuario.GetHabbo().Username, "Prisao", "Prendeu o usuário por <" + quantidadeHorasFinais + "> horas");

                            PrisaoManager prisao = OtanixEnvironment.GetGame().GetPrisaoManager();
                            prisao.prenderUsuario(Usuario.GetHabbo().Id, quantidadeHoras);
                            Usuario.Disconnect();
                            TargetRoomUser.WhisperComposer("Sucesso!");
                            break;
                        }
                    case "remove":
                        {
                            if (Params[2] == null)
                                return;

                            // Pegamos o usuário
                            GameClient Usuario = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[2]);

                            if (Usuario == null || Usuario.GetHabbo() == null)
                            {
                                TargetRoomUser.WhisperComposer("Usuário não existe ou não está conectado.");
                                return;
                            }

                            OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, Usuario.GetHabbo().Username, "Prisao", "Desprendeu o usuário");
                            PrisaoManager prisao = OtanixEnvironment.GetGame().GetPrisaoManager();
                            prisao.removePrisao(Usuario.GetHabbo().Id);

                            Usuario.Disconnect();
                            TargetRoomUser.WhisperComposer("Sucesso!");

                            break;
                        }

                    default:
                        {
                            Session.SendWindowManagerAlert(LanguageLocale.GetValue("comandos.quartos.errado") + ":prisao\":\n\r - add\n - remove");

                            break;
                        }
                }
            }
        }


        public void premium()
        {
            // Se tem outros parametros
            if (Params.Length > 1)
            {
                string CommandOrder = Params[1];
                if (CommandOrder == "premium")
                    return;

                switch (CommandOrder)
                {
                    case "add":
                        {
                            // Pega o usuário
                            string usuario = Params[2];

                            // Verifica se existe este parametro
                            if (usuario == null)
                                return;

                            GameClient user = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(usuario);

                            // Verifica se o usuário não é nulo
                            if (user == null || user.GetHabbo() == null)
                            {
                                TargetRoomUser.WhisperComposer("Usuário não está conectado.");
                                return;
                            }

                            // Pega a quantidade de mobis
                            int quantidadeMobis = int.Parse(Params[3]);

                            if (quantidadeMobis < 0 || quantidadeMobis > 5000)
                            {
                                TargetRoomUser.WhisperComposer("A quantidade de mobis deve ser entro 0 - 5000");
                                return;
                            }

                            int quantidadeDeDias = OtanixEnvironment.GetUnixTimestamp() + (int.Parse(Params[4]) * 86400);

                            if (quantidadeDeDias < 0 || quantidadeDeDias > (OtanixEnvironment.GetUnixTimestamp() + (86400 * 365)))
                            {
                                TargetRoomUser.WhisperComposer("A quantidade máxima de dias é de 365.");
                                return;
                            }

                            if (PremiumManager.UserIsSubscribed(user.GetHabbo().Id))
                            {
                                TargetRoomUser.WhisperComposer("Este usuário já é premium.");
                                return;
                            }

                            using (IQueryAdapter dbclient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                dbclient.setQuery("INSERT INTO user_premiums (user_id, unix_start, unix_end, max_items) VALUES (" + user.GetHabbo().Id + ", " + OtanixEnvironment.GetUnixTimestamp() + ", @fim, @quantidademobis)");
                                dbclient.addParameter("fim", quantidadeDeDias);
                                dbclient.addParameter("quantidademobis", quantidadeMobis);
                                dbclient.runQuery();
                                TargetRoomUser.WhisperComposer("Premium dado com sucesso!");                             
                            }
                            OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, usuario, "Premium", "Deu premium [" + quantidadeDeDias + " dias, "+quantidadeMobis+" mobis]");

                            break;
                        }

                    case "remove":
                            // Pega o usuário
                            string usuario2 = Params[2];

                            // Verifica se existe este parametro
                            if (usuario2 == null)
                                return;

                            GameClient user2 = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(usuario2);

                        // Verifica se o usuário não é nulo
                        if (user2 == null || user2.GetHabbo() == null)
                        {
                            TargetRoomUser.WhisperComposer("Usuário não está conectado.");
                            return;
                        }

                            if (user2.GetHabbo().IsPremium())
                            {
                                using (IQueryAdapter dbclient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                                {
                                    dbclient.runFastQuery("DELETE FROM user_premiums WHERE user_id = " + user2.GetHabbo().Id);
                                    TargetRoomUser.WhisperComposer("Premium retirado com sucesso!");
                                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, usuario2, "Premium", "Removeu o premium do usuario");

                            }
                        }
                        break;

                    case "altera":
                        // Pega o usuário
                        string usuario3 = Params[2];

                        // Verifica se existe este parametro
                        if (usuario3 == null)
                            return;

                        GameClient user3 = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(usuario3);

                        // Verifica se o usuário não é nulo
                        if (user3 == null || user3.GetHabbo() == null)
                        {
                            TargetRoomUser.WhisperComposer("Usuário não está conectado.");
                            return;
                        }

                        if (!PremiumManager.UserIsSubscribed(user3.GetHabbo().Id))
                        {
                            TargetRoomUser.WhisperComposer("Usuário não é premium.");
                            return;
                        }

                        // Pega a quantidade de mobis
                        int quantidadeMobis2 = int.Parse(Params[3]);

                        if (quantidadeMobis2 < 0 || quantidadeMobis2 > 5000)
                        {
                            TargetRoomUser.WhisperComposer("A quantidade de mobis deve ser entro 0 - 5000");
                            return;
                        }

                        using (IQueryAdapter dbclient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbclient.setQuery("UPDATE user_premiums SET max_items = @qtdmobis WHERE user_id = " + user3.GetHabbo().Id);
                            dbclient.addParameter("qtdmobis", quantidadeMobis2);
                            dbclient.runQuery();
                            TargetRoomUser.WhisperComposer("Quantidade de mobis alterada com sucesso!");
                            OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, usuario3, "Premium", "Alterou a quantidade de mobis do premium [" + quantidadeMobis2 + "]");

                        }
                        break;
                    default:
                        {
                            Session.SendWindowManagerAlert(LanguageLocale.GetValue("comandos.quartos.errado") + " \":premium\":\n\r - add <usuario> <Quantidade de mobis> <Quantidade de dias>\n\n - remover <usuario>\n\n - alterar <usuario> <Nova quantidade de mobis>");

                            break;
                        }
                }
            }
        }

        public void setspeed()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Creamos la variable que contiene la velocidad de roller en sala.
                uint speed = 4;

                // Comprobamos que el valor sea numérico.
                if (!uint.TryParse(Params[1], out speed))
                {
                    // El usuario recibe una notificación mediante susurro.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.intonly"));
                    return;
                }

                // Asignamos la nueva velocidad.
                Session.GetHabbo().CurrentRoom.GetRoomItemHandler().SetSpeed(speed);
            }
        }

        public void mutepets()
        {
            // Actualizamos la variable.
            TargetRoomUser.IgnorePets = !TargetRoomUser.IgnorePets;

            // Notificamos al usuario.
            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue(TargetRoomUser.IgnoreChat ? "ignore.pets.enabled" : "ignore.pets.disabled"));
        }

        public void mutebots()
        {
            // Actualizamos la variable.
            TargetRoomUser.IgnoreBots = !TargetRoomUser.IgnoreBots;

            // Notificamos al usuario.
            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue(TargetRoomUser.IgnoreChat ? "ignore.bots.enabled" : "ignore.bots.disabled"));
        }

        public void sit()
        {
            // Si el usuario está acostado.
            if (TargetRoomUser.acostadoBol == true)
            {
                // Quitamos el acostado.
                TargetRoomUser.acostadoBol = false;
                TargetRoomUser.RemoveStatus("lay");
            }

            // Si no está sentado.
            if (!TargetRoomUser.Statusses.ContainsKey("sit"))
            {
                // Si el usuario está en diagonal, ajustamos su rotación.
                TargetRoomUser.SetDiagonalRotation();

                // Obtenemos los items que hay en la baldosa.
                List<RoomItem> ItemsOnSquare = TargetRoom.GetGameMap().GetCoordinatedItems(new Point(TargetRoomUser.X, TargetRoomUser.Y));

                // Sentamos al usuario.
                TargetRoomUser.AddStatus("sit", "0.55");
                TargetRoomUser.Z = TargetRoom.GetGameMap().SqAbsoluteHeight(TargetRoomUser.X, TargetRoomUser.Y, ItemsOnSquare);
                TargetRoomUser.sentadoBol = true;
                TargetRoomUser.UpdateNeeded = true;
            }
        }

        public void lay()
        {
            // Si el usuario está sentado.
            if (TargetRoomUser.sentadoBol == true)
            {
                // Quitamos el sentado.
                TargetRoomUser.sentadoBol = false;
                TargetRoomUser.RemoveStatus("sit");
            }

            // Si no está acostado.
            if (!TargetRoomUser.Statusses.ContainsKey("lay"))
            {
                // Si el usuario está en diagonal, ajustamos su rotación.
                TargetRoomUser.SetDiagonalRotation();

                // Acostamos al usuario.
                TargetRoomUser.AddStatus("lay", Convert.ToString(TargetRoom.GetGameMap().Model.SqFloorHeight[TargetRoomUser.X, TargetRoomUser.Y] + 0.55).Replace(",", "."));
                TargetRoomUser.acostadoBol = true;
                TargetRoomUser.UpdateNeeded = true;
            }
        }

        public void stand()
        {
            // Si el usuario está sentado o acostado.
            if (TargetRoomUser.acostadoBol || TargetRoomUser.sentadoBol)
            {
                // Levantamos al usuario.
                TargetRoomUser.Statusses.Remove("lay");
                TargetRoomUser.Statusses.Remove("sit");

                TargetRoomUser.acostadoBol = false;
                TargetRoomUser.sentadoBol = false;
                TargetRoomUser.UpdateNeeded = true;
            }
        }

        public void dance()
        {
            int DanceId = -1;

            if (Params.Length == 2)
                int.TryParse(Params[1], out DanceId);

            // Tipo de dança.
            if (DanceId == -1)
                goto ParaDanca;

            if (DanceId < 1 || DanceId > 4 && DanceId != -1)
                return;

            ParaDanca:
                TargetRoomUser.DanceId = DanceId;

                // Packet con la acción.
                ServerMessage DanceMessage = new ServerMessage(Outgoing.Dance);
                DanceMessage.AppendInt32(TargetRoomUser.VirtualId);
                DanceMessage.AppendInt32(DanceId);
                TargetRoom.SendMessage(DanceMessage);
        }

        public void handitem()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Creamos la variable que llevará el Item.
                int HandItem = 0;
                if (!int.TryParse(Params[1], out HandItem))
                    Session.SendNotif(LanguageLocale.GetValue("input.intonly"));

                // Entregamos el Item al usuario.
                TargetRoomUser.CarryItem(HandItem);
            }
        }

        public void enable()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Creamos la variable que llevará el Item.
                int EffectID = 0;
                if (!int.TryParse(Params[1], out EffectID))
                    Session.SendNotif(LanguageLocale.GetValue("input.intonly"));

                // Activamos el efecto al usuario.
                Session.GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(EffectID);
            }
        }

        public void commands()
        {
            // Generamos una alerta con los comandos disponibles.
            Session.SendScrollNotif(ChatCommandRegister.GenerateCommandList(Session));
        }

        public void empty()
        {
            // Si somos Administradores y queremos borrar el inventario de un usuario.
            if (Params.Length > 1 && Session.GetHabbo().HasFuse("fuse_sysadmin"))
            {
                // GameClient del usuario.
                GameClient Client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

                // Si el usuario está conectado.
                if (Client != null)
                {
                    // Vaciamos el inventario del usuario.
                    Client.GetHabbo().GetInventoryComponent().ClearItems();

                    // Notificamos al Administrador.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("empty.dbcleared"));
                    OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, Client.GetHabbo().Username, "Empty", "Esvaziou o inventário do usuário");

                }
                else
                {
                    using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        // Obtenemos el UserId de la DB.
                        uint UserID = UsersCache.getIdByUsername(Params[1]);

                        if (UserID > 0)
                        {
                            // Eliminamos los items de la DB.
                            dbClient.runFastQuery("DELETE FROM items_users WHERE user_id = " + UserID);

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("empty.cachecleared"));
                            OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, Convert.ToString(UserID), "Empty", "Esvaziou o inventário do usuário");
                        }
                    }
                }
            }
            else
            {
                // Vaciamos el inventario del usuario.
                Session.GetHabbo().GetInventoryComponent().ClearItems();

                // Notificamos al usuario.
                TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("empty.cleared"));
            }
        }

        public void cor()
        {
            string Corsemnada = Params[1];
            if (Corsemnada == "")
            {
                Session.SendNotif("Digite :cor exp para ver todas as suas cores!");
            }

            if (Params.Length == 2)
            {
                int cor;

                if (int.TryParse(Params[1], out cor))
                {
                    if (Session.GetHabbo().tenhoCor(cor))
                    {
                        Session.GetHabbo().corAtual = cor;
                        TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("cor.atualizada"));
                    }
                    else TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("cor.npossui"));
                    
                }
                else
                {
                    Session.SendNotif(OtanixEnvironment.GetGame().CorManager().gerarCorList(Session.GetHabbo().minhasCores(), Session.GetHabbo().Username).ToString());
                }
            }
        }

        public void emptypets()
        {
            // Si somos Administradores y queremos borrar el inventario de un usuario.
            if (Params.Length > 1 && Session.GetHabbo().HasFuse("fuse_sysadmin"))
            {
                // GameClient del usuario.
                GameClient Client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

                // Si el usuario está conectado.
                if (Client != null)
                {
                    // Vaciamos el inventario del usuario.
                    Client.GetHabbo().GetInventoryComponent().ClearPets();

                    // Notificamos al Administrador.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("empty.dbcleared"));
                }
                else
                {
                    using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        // Obtenemos el UserId de la DB.
                        uint UserID = UsersCache.getIdByUsername(Params[1]);

                        if (UserID > 0)
                        {
                            // Eliminamos los items de la DB.
                            dbClient.runFastQuery("DELETE FROM user_pets WHERE user_id = " + UserID + " AND room_id = 0");

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("empty.cachecleared"));
                        }
                    }
                }
            }
            else
            {
                // Vaciamos el inventario del usuario.
                Session.GetHabbo().GetInventoryComponent().ClearPets();

                // Notificamos al usuario.
                TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("empty.cleared"));
            }
        }

        public void emptybots()
        {
            // Si somos Administradores y queremos borrar el inventario de un usuario.
            if (Params.Length > 1 && Session.GetHabbo().HasFuse("fuse_sysadmin"))
            {
                // GameClient del usuario.
                GameClient Client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

                // Si el usuario está conectado.
                if (Client != null)
                {
                    // Vaciamos el inventario del usuario.
                    Client.GetHabbo().GetInventoryComponent().ClearBots();

                    // Notificamos al Administrador.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("empty.dbcleared"));
                }
                else
                {
                    using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        // Obtenemos el UserId de la DB.
                        uint UserID = UsersCache.getIdByUsername(Params[1]);

                        if (UserID > 0)
                        {
                            // Eliminamos los items de la DB.
                            dbClient.setQuery("SELECT items.item_id FROM items JOIN items_users ON items_users.item_id = items.item_id WHERE items_users.user_id = " + UserID);
                            DataTable dTable = dbClient.getTable();

                            if (dTable != null)
                            {
                                foreach (DataRow dRow in dTable.Rows)
                                {
                                    int ItemId = int.Parse(dRow["item_id"].ToString());

                                    dbClient.runFastQuery("DELETE FROM bots WHERE id = " + ItemId);
                                    dbClient.runFastQuery("DELETE FROM items WHERE item_id = " + ItemId);
                                    dbClient.runFastQuery("DELETE FROM items_users WHERE item_id = " + ItemId);
                                }
                            }

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("empty.cachecleared"));
                        }
                    }
                }
            }
            else
            {
                // Vaciamos el inventario del usuario.
                Session.GetHabbo().GetInventoryComponent().ClearBots();

                // Notificamos al usuario.
                TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("empty.cleared"));
            }
        }
        public void info()
        {
            // Tiempo que lleva el serivodr online.
            TimeSpan TimeUsed = DateTime.Now - OtanixEnvironment.ServerStarted;

            // Paquete de la alerta.
            ServerMessage Message = new ServerMessage(Outgoing.GeneratingNotification);
            Message.AppendString("Otanix");
            Message.AppendInt32(3);
            Message.AppendString("title");
            Message.AppendString("Informações do servidor");
            Message.AppendString("message");
            Message.AppendString("<b><font color=\"#45c2e6\" size=\"16\">Otanix Emulador</font></b>\r\r" +
                                "O melhor dos melhores emuladores!\r\r"+
                                "<b><font color=\"#45c2e6\" size=\"16\">Estatísticas</font></b>\r\r" +
                                "<b>Update</b> " + TimeUsed.Days + " dias, " + TimeUsed.Hours + " horas, " + TimeUsed.Minutes + " minutos.\r"+
                                "<b>Onlines:</b> " + OtanixEnvironment.GetGame().GetClientManager().connectionCount + " usuários online.\r"+
                                "<b>Quartos:</b> " + OtanixEnvironment.GetGame().GetRoomManager().LoadedRoomsCount + " Quartos Carregados.\r\r"+
                                "<b>Licença:</b> <b><font color=\"#45c2e6\">" + LanguageLocale.GetValue("hotel.link") + "</font></b>\r\r"+
                                "<b><font color=\"#45c2e6\" size=\"16\">Créditos</font></b>\r" +
                                "Desenvolvido por <b> Thiago Araujo </b>\r" +
                                "Editado por <b> ... () </b>\r" +
                                "Butterfly developers\r");
            Message.AppendString("image");
            Message.AppendString(LanguageLocale.GetValue("info.image"));
            Session.SendMessage(Message);
        }

        public void personal()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Comando.
                string CommandOrder = Params[1];

                switch (CommandOrder)
                {
                    case "trade":
                        {
                            // Actualizamos la variable.
                            Session.GetHabbo().BlockTrade = !Session.GetHabbo().BlockTrade;

                            // Notificamos al usuario.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue(Session.GetHabbo().BlockTrade ? "trade.enable" : "trade.disable"));

                            break;
                        }
                    case "test":
                        {

                            ServerMessage ChatMessage = new ServerMessage(Outgoing.Talk);
                            ChatMessage.AppendInt32(TargetRoomUser.VirtualId);
                            ChatMessage.AppendString("Teste");
                            ChatMessage.AppendInt32(0);
                            ChatMessage.AppendInt32(33);
                            ChatMessage.AppendInt32(0);
                            ChatMessage.AppendInt32(-1);
                            TargetRoomUser.GetClient().SendMessage(ChatMessage);

                            break;
                        }
                    case "alertas":
                        {

                            // Atualiza a variavel dos alertas
                            if (Session.GetHabbo().alertasAtivados)
                                Session.GetHabbo().alertasAtivados = false;
                            else
                                Session.GetHabbo().alertasAtivados = true;

                            // Notifica o usuário
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue(Session.GetHabbo().alertasAtivados ? "alertas.ativado" : "alertas.desativados"));
                            break;
                        }
                    case "friends":
                        {
                            // Actualizamos la variable.
                            Session.GetHabbo().HasFriendRequestsDisabled = !Session.GetHabbo().HasFriendRequestsDisabled;

                            // Notificamos al usuario.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue(Session.GetHabbo().HasFriendRequestsDisabled ? "friends.enabled" : "friends.disabled"));

                            break;
                        }

                    case "follow":
                        {
                            // Actualizamos la variable.
                            Session.GetHabbo().FollowEnable = !Session.GetHabbo().FollowEnable;

                            // Actualizamos el estado de la consola.
                            OtanixEnvironment.GetGame().GetClientManager().QueueConsoleUpdate(Session);

                            // Notificamos al usuario.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue(Session.GetHabbo().HasFriendRequestsDisabled ? "follow.enabled" : "follow.disabled"));

                            break;
                        }

                    case "staff":
                        {
                            // Si tiene más rango que este fuse.
                            if (Session.GetHabbo().HasFuse("fuse_badge_bot"))
                            {
                                // Actualizamos la variable.
                                Session.GetHabbo().showingStaffBadge = !Session.GetHabbo().showingStaffBadge;

                                // Notificamos al usuario.
                                TargetRoomUser.WhisperComposer("Efeito staff foi " + ((Session.GetHabbo().showingStaffBadge) ? "ativado" : "desativado"));

                                // Si es true.
                                if (Session.GetHabbo().showingStaffBadge)
                                {
                                    // Mostramos la placa según su rango.
                                    if (Session.GetHabbo().HasFuse("fuse_badge_staff"))
                                    {
                                        Session.GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(102);
                                    }
                                    else if (Session.GetHabbo().HasFuse("fuse_ambassador"))
                                    {
                                        Session.GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(178);
                                    }
                                    else if (Session.GetHabbo().HasFuse("fuse_badge_bot"))
                                    {
                                        Session.GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(187);
                                    }
                                }
                                else
                                {
                                    // Desactivamos el efecto.
                                    Session.GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(-1);
                                }
                            }
                            break;
                        }

                    case "ignore":
                        {
                            // Actualizamos la variable.
                            TargetRoomUser.IgnoreChat = !TargetRoomUser.IgnoreChat;

                            // Notificamos al usuario.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue(TargetRoomUser.IgnoreChat ? "ignore.enabled" : "ignore.disabled"));

                            break;
                        }

                    default:
                        {
                            Session.SendWindowManagerAlert(LanguageLocale.GetValue("comandos.quartos.errado") + ":personal\":\n\r - trade\n - friends\n - follow\n - staff\n - ignore\n - alertas");

                            break;
                        }
                }
            }
        }


        public void follow()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // GameClient del usuario.
                GameClient TargetClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

                // Comprobamos que el usuario esté conectado y en alguna sala.
                if (TargetClient == null || TargetClient.GetHabbo() == null)
                {
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                    return;
                }
                else if(TargetClient.GetHabbo().CurrentRoom == null)
                {
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("user.notinroom"));
                    return;
                }
                else if(TargetClient.GetHabbo().CurrentRoomId == Session.GetHabbo().CurrentRoomId)
                {
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("user.insameroom"));
                    return;
                }

                // Enviamos al usuario a la sala.
                Session.GetMessageHandler().PrepareRoomForUser(TargetClient.GetHabbo().CurrentRoom.RoomId, "");
            }
        }

        public void moonwalk()
        { 
            // Actualizamos la variable de usuario.
            TargetRoomUser.moonwalkEnabled = !TargetRoomUser.moonwalkEnabled;

            // El usuario recibe una notificación mediante susurro.
            TargetRoomUser.WhisperComposer("Você " + (TargetRoomUser.moonwalkEnabled ? "ativou" : "desativou") + " o moonwalk.");
        }

        public void copylook()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Nombre de usuario a copiar.
                string copyTarget = Params[1];

                // Variables extra.
                bool findResult = false;
                string figure = null;
                string gender = null;

                // GameClient del usuario.
                GameClient userClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(copyTarget);

                // Si el usuario está online.
                if (userClient != null && userClient.GetHabbo() != null)
                {
                    figure = userClient.GetHabbo().Look;
                    gender = userClient.GetHabbo().Gender;
                    findResult = true;
                }
                else
                {
                    // Al no estar online, obtenemos los datos de la DB.
                    DataRow dRow;
                    using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        dbClient.setQuery("SELECT gender,look FROM users WHERE username = @username");
                        dbClient.addParameter("username", copyTarget);
                        dRow = dbClient.getRow();

                        if (dRow != null)
                        {
                            findResult = true;
                            gender = (string)dRow[0];
                            figure = (string)dRow[1];
                        }
                    }
                }

                if (findResult)
                {
                    /*if (!OtanixEnvironment.GetGame().GetUserLookManager().IsValidLook(Session.GetHabbo(), figure))
                    {
                        TargetRoomUser.WhisperComposer("El look que intentas ponerte no es válido para tu usuario.");
                        return;
                    }*/

                    // Actualizamos las variables de usuario.
                    Session.GetHabbo().Gender = gender;
                    Session.GetHabbo().Look = figure;

                    // Actualizamos la carita de la barra del look de usuario.
                    Session.GetMessageHandler().GetResponse().Init(Outgoing.UpdateUserInformation);
                    Session.GetMessageHandler().GetResponse().AppendInt32(-1);
                    Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Look);
                    Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Gender.ToLower());
                    Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Motto);
                    Session.GetMessageHandler().GetResponse().AppendUInt(Session.GetHabbo().AchievementPoints);
                    Session.GetMessageHandler().SendResponse();

                    if (Session.GetHabbo().InRoom)
                    {
                        // Actualizamos el look en sala.
                        ServerMessage RoomUpdate = new ServerMessage(Outgoing.UpdateUserInformation);
                        RoomUpdate.AppendInt32(TargetRoomUser.VirtualId);
                        RoomUpdate.AppendString(Session.GetHabbo().Look);
                        RoomUpdate.AppendString(Session.GetHabbo().Gender.ToLower());
                        RoomUpdate.AppendString(Session.GetHabbo().Motto);
                        RoomUpdate.AppendUInt(Session.GetHabbo().AchievementPoints);
                        TargetRoom.SendMessage(RoomUpdate);
                    }
                }
            }
        }
        
        public void fastwalk()
        {
            if (TargetRoomUser.fastWalk)
            {
                TargetRoomUser.fastWalk = false;
                TargetRoomUser.WhisperComposer("Você desativou o fastwalk.");
            }
            else
            {
                TargetRoomUser.fastWalk = true;
                TargetRoomUser.WhisperComposer("Você ativou o fastwalk.");
            }    
        }

        public void freezeuser()
        {

            if(Params.Length == 2)
            {
                string username = Params[1];
                RoomUser targetFuturoUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(username);

                if(targetFuturoUser != null)
                {
                    if (targetFuturoUser.CanWalk)
                    {
                        targetFuturoUser.CanWalk = false;
                        targetFuturoUser.comandoFreeze = true;
                        targetFuturoUser.ApplyEffect(12);
                        OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, targetFuturoUser.GetClient().GetHabbo().Username, "Freeze", "Congelou um usuário");
                    }
                    else
                    {
                        targetFuturoUser.CanWalk = true;
                        targetFuturoUser.comandoFreeze = false;
                        targetFuturoUser.ApplyEffect(targetFuturoUser.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().BackupEffect);
                        OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, targetFuturoUser.GetClient().GetHabbo().Username, "Freeze", "Descongelou um usuário");
                    }

                    return;
                }
            }
            return;
        }
        public void push()
        {
            // Creamos la variable nula para un futuro usuario.
            RoomUser roomuserTarget = null;

            // Si no hay parámetros.
            if (Params.Length == 1)
            {
                // Pusheamos al usuario de la casilla de enfrente.
                roomuserTarget = TargetRoom.GetRoomUserManager().GetUserForSquare(TargetRoomUser.SquareInFront.X, TargetRoomUser.SquareInFront.Y);

                // Si no existe usuario.
                if (roomuserTarget == null)
                    return;
            }
            else
            {
                // Si vamos a pushear a un usuario en concreto.
                roomuserTarget = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);

                // Si no existe usuario.
                if (roomuserTarget == null)
                    return;

                // Si el usuario no está enfrente.
                if (roomuserTarget.Coordinate != TargetRoomUser.SquareInFront)
                {
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("user.theuser") + Params[1] + LanguageLocale.GetValue("user.notbelong"));
                    return;
                }
            }

            // Obtenemos la baldosa donde será pusheado el usuario.
            Point squareInFrontOfUserInFront = CoordinationUtil.GetDoublePointInFront(TargetRoomUser.Coordinate, TargetRoomUser.RotBody);

            // Si la baldosa no es andable.
            if (!TargetRoom.GetGameMap().tileIsWalkable(squareInFrontOfUserInFront.X, squareInFrontOfUserInFront.Y, true))
                return;

            // No podemos pushear a un usuario que tenga más rango.
            if (roomuserTarget.GetClient().GetHabbo().Rank <= Session.GetHabbo().Rank)
            {
                // Notificamos el pusheo en la sala.
                TargetRoomUser.Chat(Session, LanguageLocale.GetValue("user.push") + roomuserTarget.GetUsername() + "*", 0, false);

                // Movemos al usuario.
                roomuserTarget.MoveTo(squareInFrontOfUserInFront);
            }
        }

        public void pull()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Nombre de usuario a copiar.
                string Username = Params[1];

                // RoomUser del usuario que vamos a pullear.
                RoomUser roomUserPulled = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(Username);
                if (roomUserPulled == null)
                    return;

                // Si no somos el mismo.
                if (TargetRoomUser.HabboId == roomUserPulled.HabboId)
                    return;

                // No podemos pullear a un usuario que tenga más rango.
                if (roomUserPulled.GetClient().GetHabbo().Rank <= Session.GetHabbo().Rank)
                {
                    // Notificamos el pusheo en la sala.
                    TargetRoomUser.Chat(Session, LanguageLocale.GetValue("user.pull") + roomUserPulled.GetUsername() + "*", 0, false);

                    // Movemos al usuario.
                    roomUserPulled.MoveTo(TargetRoomUser.SquareInFront);
                }
            }
        }

        public void kill()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Obtenemos el usuario en cuestión.
                string Username = Params[1];

                // Cargamos el RoomUser.
                RoomUser roomUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(Username);
                if (roomUser == null)
                    return;

                // Si el usuario no está justo enfrente.
                if (roomUser.Coordinate != TargetRoomUser.SquareInFront)
                {
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("user.theuser") + Username + LanguageLocale.GetValue("user.notbelong"));
                    return;
                }

                // Quitamos el estado de sentado.
                if (roomUser.sentadoBol == true)
                {
                    roomUser.sentadoBol = false;
                    roomUser.RemoveStatus("sit");
                }

                // Si el usuario no está acostado.
                if (!roomUser.Statusses.ContainsKey("lay"))
                {
                    // No está en rotación diagonal.
                    if ((roomUser.RotBody % 2) == 0)
                    {
                        // Acostamos al usuario.
                        roomUser.AddStatus("lay", Convert.ToString(TargetRoom.GetGameMap().Model.SqFloorHeight[roomUser.X, roomUser.Y] + 0.55).Replace(",", "."));
                        roomUser.acostadoBol = true;
                        roomUser.UpdateNeeded = true;

                        // El usuario recibe una notificación mediante susurro.
                        roomUser.WhisperComposer(LanguageLocale.GetValue("user.killedby") + TargetRoomUser.GetUsername());
                    }
                }
            }
        }

        public void convertToPet()
        {
            // Se puede cambiar de estado cada 5segundos.
            if ((OtanixEnvironment.GetUnixTimestamp() - Session.GetHabbo().LastChangePetTime) < 5)
            {
                // El usuario recibe una notificación mediante susurro.
                TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("change.converttopet.alert.time"));
                return;
            }

            // IdMascota a la que queremos transformarnos.
            string Message = MergeParams(Params, 1);

            // Si queremos transformarnos a alguna.
            if (Message.Length > 0)
            {
                // Usuario = -1
                int helper = -1;

                // Intentamos convertir.
                int.TryParse(Message, out helper);

                // Si no está en el rango de mascotas permitidas.
                if (helper < -1 || helper > 34)
                {
                    // El usuario recibe una notificación mediante susurro.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("converttopet.integer"));
                    return;
                }
                else if (helper == -1 || helper == 0 && Message != "0")
                {
                    helper = Array.IndexOf(PetRace.PetIdByName, Message.ToLower());
                }

                if (helper == -1)
                {
                    // El usuario recibe una notificación mediante susurro.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("converttopet.petname"));
                    return;
                }

                // Transformamos al usuario.
                Session.GetHabbo().ConvertedOnPet = true;
                Session.GetHabbo().PetType = helper;
            }
            else
            {
                // Volvemos a la normalidad.
                Session.GetHabbo().ConvertedOnPet = false;
                Session.GetHabbo().PetType = -1;
            }

            // Guardamos la hora de la última transformación.
            Session.GetHabbo().LastChangePetTime = OtanixEnvironment.GetUnixTimestamp();

            // El usuario recibe una notificación mediante susurro.
            if (Session.GetHabbo().ConvertedOnPet)
                TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("converttopet.pet"));
            else
                TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("converttopet.normal"));

            // Packet que elimina al usuario de sala.
            ServerMessage LeaveMessage = new ServerMessage(Outgoing.UserLeftRoom);
            LeaveMessage.AppendString(TargetRoomUser.VirtualId + String.Empty);
            TargetRoom.SendMessage(LeaveMessage);

            // Packet que coloca al usuario en sala.
            ServerMessage EnterMessage = new ServerMessage(Outgoing.UsersMessageParser);
            EnterMessage.AppendInt32(1);
            TargetRoomUser.Serialize(EnterMessage);
            TargetRoom.SendMessage(EnterMessage);
        }

        public void ClearConsole()
        {
            // Eliminamos los amigos de la consola.
            Session.GetHabbo().GetMessenger().ClearConsole();

            // El usuario recibe una notificación mediante susurro.
            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("console.empty"));
        }

        public void userinfo()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Obtenemos el usuario en cuestión.
                string Username = Params[1];

                // Obtenemos la información del usuario.
                Habbo User = UsersCache.getHabboCache(Username);

                // Si no existe el usuario.
                if (User == null)
                {
                    // El usuario recibe una notificación mediante susurro.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                    return;
                }

                // Variable información de sala.
                StringBuilder RoomInformation = new StringBuilder();

                // Si el usuario está en sala.
                if (User.CurrentRoom != null)
                {
                    RoomInformation.Append(" - " + LanguageLocale.GetValue("roominfo.title") + " [" + User.CurrentRoom.RoomId + "] - \r");
                    RoomInformation.Append(LanguageLocale.GetValue("userinfo.owner") + User.CurrentRoom.RoomData.Owner + "\r");
                    RoomInformation.Append(LanguageLocale.GetValue("userinfo.roomname") + User.CurrentRoom.RoomData.Name + "\r");
                    RoomInformation.Append(LanguageLocale.GetValue("userinfo.usercount") + User.CurrentRoom.UserCount + "/" + User.CurrentRoom.RoomData.UsersMax);
                }

                // Alerta final.
                Session.SendNotif(LanguageLocale.GetValue("userinfo.userinfotitle") + Username + ":\r" +
                                  LanguageLocale.GetValue("userinfo.rank") + User.Rank + " \r" +
                                  LanguageLocale.GetValue("userinfo.isonline") + (OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(User.Id) != null) + " \r" +
                                  LanguageLocale.GetValue("userinfo.userid") + User.Id + " \r" +
                                  LanguageLocale.GetValue("userinfo.visitingroom") + User.CurrentRoomId + " \r" +
                                  LanguageLocale.GetValue("userinfo.motto") + User.Motto + " \r" +
                                  LanguageLocale.GetValue("userinfo.diamonds") + User.Diamonds + " \r" +
                                  LanguageLocale.GetValue("userinfo.ismuted") + OtanixEnvironment.GetGame().GetMuteManager().UserIsMuted(User.Id) + "\r" +
                                  "\r\r" +
                                  RoomInformation);

                // Limpiamos la variable.
                RoomInformation.Clear();
                RoomInformation = null;
            }
        }

        public void ban()
        {
            // Verificamos que haya >= 2 parámetros en la formulación del comando.
            if (Params.Length >= 2)
            {
                // Obtenemos la información de usuario.
                Habbo TargetHabbo = UsersCache.getHabboCache(Params[1]);
                if (TargetHabbo == null)
                {
                    // El usuario recibe una notificación mediante susurro.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                    return;
                }

                // No se puede banear a un usuario con más rango que tú.
                if (TargetHabbo.Rank >= Session.GetHabbo().Rank)
                {
                    // El usuario recibe una notificación mediante susurro.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("ban.notallowed"));
                    return;
                }

                // Variable del tiempo baneado.
                int BanTime = 86400;

                // Intentamos coger el tiempo establecido por el staff.
                if (Params.Length >= 3)
                    int.TryParse(Params[2], out BanTime);

                if (BanTime <= 600)
                {
                    // El usuario recibe una notificación mediante susurro.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("ban.toolesstime"));
                }
                else
                {
                    // Obtenemos la sesión del usuario baneado.
                    GameClient TargetClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(TargetHabbo.Username);

                    // Baneamos al usuario.
                    OtanixEnvironment.GetGame().GetBanManager().BanUser(TargetClient, TargetHabbo.Username, "", MergeParams(Params, 3), BanTime, Session);
                    
                    // Guardamos en Logs.
                    OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, TargetHabbo.Username, "UserBan", "Long ban for " + BanTime + " seconds");

                }
            }
        }

        public void superban()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Obtenemos la información de usuario.
                Habbo TargetHabbo = UsersCache.getHabboCache(Params[1]);

                // Si el usuario no existe.
                if (TargetHabbo == null)
                {
                    // El usuario recibe una notificación mediante susurro.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                    return;
                }

                // No se puede banear a un usuario con más rango que tú.
                if (TargetHabbo.Rank >= Session.GetHabbo().Rank)
                {
                    // El usuario recibe una notificación mediante susurro.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("ban.notallowed"));
                    return;
                }

                // Guardamos en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, TargetHabbo.Username, "SuperBan", "Super ban!");

                // Obtenemos la sesión del usuario baneado.
                GameClient TargetClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(TargetHabbo.Username);

                // Baneamos al usuario.
                OtanixEnvironment.GetGame().GetBanManager().SuperBan(TargetClient, TargetHabbo.Username, TargetHabbo.MachineId, MergeParams(Params, 2), 315360000, Session);
            }
        }

        public void muteuser()
        {
            // Verificamos que haya 2 o más parámetros en la formulación del comando.
            if (Params.Length >= 2)
            {
                // Obtenemos la información de usuario.
                Habbo TargetHabbo = UsersCache.getHabboCache(Params[1]);

                // Si el usuario no existe.
                if (TargetHabbo == null)
                {
                    // El usuario recibe una notificación mediante susurro.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                    return;
                }

                // No se puede mutear a un usuario con más rango que tú.
                if (TargetHabbo.Rank >= Session.GetHabbo().Rank)
                {
                    // El usuario recibe una notificación mediante susurro.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("ban.notallowed"));
                    return;
                }

                // Variable del tiempo baneado.
                int BanLenght = 2;

                // Intentamos coger el tiempo establecido por el staff.
                if (Params.Length >= 3)
                    int.TryParse(Params[2], out BanLenght);

                // Guardamos en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, TargetHabbo.Username, "Mute", "Muted user");

                // Mutea al usuario.
                ModerationTool.MuteUser(Session, TargetHabbo, BanLenght, "");
            }
        }

        public void unmuteuser()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Nombre de Usuario.
                string TargetUser = Params[1];

                // Obtenemos la información de usuario.
                Habbo TargetHabbo = UsersCache.getHabboCache(TargetUser);

                // Si el usuario no existe.
                if (TargetHabbo == null)
                {
                    // El usuario recibe una notificación mediante susurro.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                    return;
                }

                // Si el usuario no está muteado.
                if (!OtanixEnvironment.GetGame().GetMuteManager().UserIsMuted(TargetHabbo.Id))
                {
                    // Notificamos al Administrador.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("user.theuser") + TargetUser + LanguageLocale.GetValue("user.notmutedglobal"));
                    return;
                }

                // Eliminamos el muteo.
                OtanixEnvironment.GetGame().GetMuteManager().RemoveUserMute(TargetHabbo.Id);

                // Enviamos alerta al usuario muteado.
                TargetHabbo.GetClient().SendNotif(LanguageLocale.GetValue("user.desmutedby") + Session.GetHabbo().Username);

                // Guardamos en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, TargetUser, "Mute", "Unmuted user");
            }
        }

        public void roomalert()
        {
            // Verificamos que haya >= 2 parámetros en la formulación del comando.
            if (Params.Length >= 2)
            {
                // Texto de la alerta.
                string Msg = MergeParams(Params, 1);

                // Packet de la alerta.
                ServerMessage nMessage = new ServerMessage(Outgoing.SendNotif);
                nMessage.AppendString(Msg);
                nMessage.AppendString("");
                TargetRoom.SendMessage(nMessage);

                // Guardamos en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Alert", "Room alert with message [" + Msg + "]");
            }
        }

        public void updates()
        {
            ServerMessage newm = new ServerMessage(Outgoing.GeneratingNotification);
            newm.AppendString("info." + EmuSettings.HOTEL_LINK);
            newm.AppendInt32(5);
            newm.AppendString("image");
            newm.AppendString(LanguageLocale.GetValue("att.imagem"));
            newm.AppendString("title");
            newm.AppendString(LanguageLocale.GetValue("att.title"));
            newm.AppendString("message");
            newm.AppendString(LanguageLocale.GetValue("att.msg"));
            newm.AppendString("linkTitle");
            newm.AppendString(LanguageLocale.GetValue("att.ok"));
            newm.AppendString("linkUrl");
            newm.AppendString("event:");
            Session.SendMessage(newm);
        }

        public void roommute()
        {
            // Valor de la variable mute.
            TargetRoom.RoomMuted = !TargetRoom.RoomMuted;

            // limpiamos los muteos actuales.
            TargetRoom.ClearMute();

            // Lista de usuarios.
            foreach (RoomUser user in TargetRoom.GetRoomUserManager().UserList.Values)
            {
                // Si es una mascota/bot o no está online.
                if (user == null || user.IsBot || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                    continue;

                // Notificamos al Administrador
                user.WhisperComposer(LanguageLocale.GetValue("room.theroom") + (TargetRoom.RoomMuted ? "mutado" : "desmutado"));

                // Si son rango, no se mutean.
                if (user.GetClient().GetHabbo().Rank > 3)
                    continue;

                // Añadimos al usuario al muteo.
                if (TargetRoom.RoomMuted)
                    TargetRoom.AddMute(user.HabboId, 900000);
                else
                    TargetRoom.ClearMute();
            }

            // Guardamos en Logs.
            OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Room Mute", "Room muted");
        }

        public void disconnect()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Obtenemos el GameClient del usuario.
                GameClient TargetClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

                // Si no está conectado.
                if (TargetClient == null)
                {
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                    return;
                }

                // Si tiene el rango igual o mayor no se puede desconectar.
                if (TargetClient.GetHabbo().Rank >= Session.GetHabbo().Rank)
                {
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("disconnect.notallwed"));
                    return;
                }

                // Guardamos en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, TargetClient.GetHabbo().Username, "Disconnect", "User disconnected by user");

                // Desconectamos al usuario.
                TargetClient.GetConnection().Dispose();
            }
        }

        public void come()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Obtenemos el usuario en cuestión.
                string[] Usernames = Params[1].Split(',');

                foreach (string Username in Usernames)
                {
                    // Obtenemos el GameClient del usuario.
                    GameClient client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);

                    // Si el usuario no está conectado.
                    if (client == null || client.GetHabbo() == null)
                    {
                        TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                        return;
                    }

                    // Lo traemos a la sala.
                    client.GetHabbo().comingRoom = TargetRoom.Id;
                    client.GetMessageHandler().enterOnRoom3(TargetRoom);
                }

                // Guardamos en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Get", "Command Get to: [" + Usernames + "]");
            }
        }

        public void pegaGeral()
        {

            foreach (GameClient client in OtanixEnvironment.GetGame().GetClientManager().clients.Values)
            {
                // Verifica se o usuário está conectado && Verifica se o usuário já está no mesmo quarto
                if (client == null || client.GetHabbo() == null || client.GetHabbo().CurrentRoomId == TargetRoom.Id)
                    continue;

                client.GetHabbo().comingRoom = TargetRoom.Id;
                client.GetMessageHandler().enterOnRoom3(TargetRoom);
            }

            // Salvamos os logs
            OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Get", "Puxou todos os usuários para o quarto [" + TargetRoom.Id + "]");
    }

        public void staffmessages()
        {
            // Comprobamos que hayan parámetros
            if (Params.Length > 1)
            {
                // Mensaje que aparecerá en la alerta.
                string Msg = MergeParams(Params, 1);

                // Packet de la alerta.
                ServerMessage notif = new ServerMessage(Outgoing.SendNotifWithScroll);
                notif.AppendInt32(1);
                notif.AppendString(LanguageLocale.GetValue("staffmessage.starting") + Session.GetHabbo().Username + LanguageLocale.GetValue("staffmessage.ending") + Msg);
                OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(notif, "fuse_sa", 0);

                // Guardamos en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Staff Message", "Sent staff message: " + Msg);
            }
        }

        public void openroom()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Variable con el Id de Sala
                uint RoomId = 0;

                // Comprobamos si es un Id válido.
                if (!uint.TryParse(Params[1], out RoomId))
                {
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.intonly"));
                    return;
                }

                // El usuario abre la sala.
                Session.GetMessageHandler().PrepareRoomForUser(RoomId, "");
            }
        }

        public void teleport()
        {
            // Activamos/Desactivamos la variable teleport.
            TargetRoomUser.TeleportEnabled = !TargetRoomUser.TeleportEnabled;

            // Notificamos al usuario.
            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("teleport.status") + (TargetRoomUser.TeleportEnabled ? "ativado" : "desativado"));
        }

        public void overridee()
        {
            // Activamos/Desactivamos la variable teleport.
            TargetRoomUser.AllowOverride = !TargetRoomUser.AllowOverride;

            // Notificamos al usuario.
            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("override.status") + (TargetRoomUser.AllowOverride ? "ativado" : "desativado"));
        }

        public void Fly()
        {
            // Si tiene el fly activado.
            if (TargetRoomUser.isFlying)
            {
                TargetRoomUser.isFlying = false;
                TargetRoomUser.AllowOverride = false;
            }
            // Si está el fly desactivado
            else if (!TargetRoomUser.isFlying)
            {
                TargetRoomUser.isFlying = true;
                TargetRoomUser.AllowOverride = true;
            }

            // Notificamos al usuario.
            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("fly.status") + (TargetRoomUser.isFlying ? "ativado" : "desativado"));
        }

        public void giveBadge()
        {
            // Verificamos que haya 3 parámetros en la formulación del comando.
            if (Params.Length == 3)
            {
                // Nombre de Usuario.
                string Username = Params[1];

                // Código de Placa.
                string BadgeCode = Params[2];

                // GameClient del Usuario de placa.
                GameClient TargetClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);

                // Si está desconectado.
                if (TargetClient == null)
                {
                    // Obtenemos el Id de Usuario.
                    uint UserId = UsersCache.getIdByUsername(Username);

                    //Si este usuario existe.
                    if (UserId > 0)
                    {
                        // Insertamos la placa por SQL.
                        using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.setQuery("REPLACE INTO user_badges VALUES ('" + UserId + "',@badge,'0','0')");
                            dbClient.addParameter("badge", BadgeCode);
                            dbClient.runQuery();
                        }
                    }
                    else
                    {
                        // Notificamos del que el usuario no se ha encontrado.
                        TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                        return;
                    }
                }
                else
                {
                    // Damos la placa al usuario.
                    TargetClient.GetHabbo().GetBadgeComponent().GiveBadge(OtanixEnvironment.FilterInjectionChars(BadgeCode));

                    // Notificamos al usuario.
                    TargetClient.SendWindowManagerAlert(LanguageLocale.GetValue("badge.receive1") + BadgeCode + LanguageLocale.GetValue("badge.receive2"));
                }

                // Guardamos la acción en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, Username, "Badge", "Badge given to user [" + BadgeCode + "]");
            }
        }

        public void removeBadge()
        {
            // Verificamos que haya 3 parámetros en la formulación del comando.
            if (Params.Length == 3)
            {
                // Nombre de Usuario.
                string Username = Params[1];

                // Código de Placa.
                string BadgeCode = Params[2];

                // GameClient del Usuario de placa.
                GameClient TargetClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);

                // Si está desconectado.
                if (TargetClient == null)
                {
                    // Obtenemos el Id de Usuario.
                    uint UserId = UsersCache.getIdByUsername(Username);

                    //Si este usuario existe.
                    if (UserId > 0)
                    {
                        // Quitamos la placa al usuario.
                        using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.setQuery("DELETE FROM user_badges WHERE user_id = @userid AND badge_id = @badge");
                            dbClient.addParameter("userid", UserId);
                            dbClient.addParameter("badge", BadgeCode);
                            dbClient.runQuery();
                        }
                    }
                    else
                    {
                        // Notificamos del que el usuario no se ha encontrado.
                        TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                        return;
                    }
                }
                else
                {
                    // Quitamos la placa.
                    TargetClient.GetHabbo().GetBadgeComponent().RemoveBadge(OtanixEnvironment.FilterInjectionChars(BadgeCode));

                    // Notificamos al usuario.
                    TargetClient.SendNotif(LanguageLocale.GetValue("badge.remove1") + BadgeCode + LanguageLocale.GetValue("badge.remove2"));
                }

                // Guardamos la acción en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, Username, "Badge", "Badge removed to user [" + BadgeCode + "]");
            }
        }

        public void roombadge()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Código de la placa.
                string BadgeCode = Params[1];

                // Insertamos la placa a los usuarios de sala.
                TargetRoom.QueueRoomBadge(BadgeCode);

                // Alerta.
                ServerMessage nMessage = new ServerMessage(Outgoing.WindowManagerAlert);
                nMessage.AppendInt32(0);
                nMessage.AppendString(LanguageLocale.GetValue("badge.receive1") + BadgeCode + LanguageLocale.GetValue("badge.receive2"));
                TargetRoom.SendMessage(nMessage);

                // Guardamos la acción en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Badge", "Roombadge in room [" + TargetRoom.RoomId + "] with badge [" + BadgeCode + "]");
            }
        }

        public void linkAlert()
        {
            // Verificamos que haya más de 1 parámetros en la formulación del comando.
            if (Params.Length > 1)
            {
                // Link de la alerta al que redireccionará.
                string Link = Params[1];

                // Mensaje de la alerta.
                string Message = MergeParams(Params, 2);

                // Paquet de la alerta.
                ServerMessage nMessage = new ServerMessage(Outgoing.SendLinkNotif);
                nMessage.AppendString(LanguageLocale.GetValue("hotelallert.notice") + "\r\n" + Message + "\r\n-" + Session.GetHabbo().Username);
                nMessage.AppendString(Link);
                OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(nMessage);

                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Alertas", "Usou um linkalert, texto & link: "+Link+"/"+Message);

            }
        }

        public void eha()
        {
            // Verificamos que haya más de 1 parámetros en la formulación del comando.
            if (Params.Length > 1)
            {
                // Mensaje de la alerta.
                string Message = MergeParams(Params, 1);
                    // Paquete.
                    ServerMessage newm = new ServerMessage(Outgoing.GeneratingNotification);
                    newm.AppendString("info." + EmuSettings.HOTEL_LINK);
                    newm.AppendInt32(5);
                    newm.AppendString("image");
                    newm.AppendString(LanguageLocale.GetValue("eha.image"));
                    newm.AppendString("title");
                    newm.AppendString(LanguageLocale.GetValue("alert.title"));
                    newm.AppendString("message");
                    newm.AppendString(LanguageLocale.GetValue("eha.message").Replace("#Username#", "<font color=\"#2E9AFE\">" + Session.GetHabbo().Username + "</font>").Replace("#Message#", Message));
                    newm.AppendString("linkTitle");
                    newm.AppendString(LanguageLocale.GetValue("alert.eha.title"));
                    newm.AppendString("linkUrl");
                    newm.AppendString("event:navigator/goto/" + Session.GetHabbo().CurrentRoomId);
                    OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(newm);

                    // Guardamos la acción en Logs.
                    OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "EventHotelAlert", "EventHotel alert [" + Message + "]");
            }
        }

        public void pha()
        {
            // Verificamos que haya más de 1 parámetros en la formulación del comando.
            if (Params.Length > 1)
            {
                // Mensaje de la alerta.
                string Message = MergeParams(Params, 1);

                    // Paquete.
                    ServerMessage newm = new ServerMessage(Outgoing.GeneratingNotification);
                    newm.AppendString("info." + LanguageLocale.GetValue("hotel.link"));
                    newm.AppendInt32(5);
                    newm.AppendString("image");
                    newm.AppendString(LanguageLocale.GetValue("pha.image"));
                    newm.AppendString("title");
                    newm.AppendString(LanguageLocale.GetValue("alert.title"));
                    newm.AppendString("message");
                    newm.AppendString(LanguageLocale.GetValue("pha.message").Replace("#Username#", "<font color=\"#2E9AFE\">" + Session.GetHabbo().Username + "</font>").Replace("#Message#", Message));
                    newm.AppendString("linkTitle");
                    newm.AppendString(LanguageLocale.GetValue("alert.pha.title"));
                    newm.AppendString("linkUrl");
                    newm.AppendString("event:navigator/goto/" + Session.GetHabbo().CurrentRoomId);
                    OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(newm);

                    // Guardamos la acción en Logs.
                    OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "EventHotelAlert", "EventHotel alert [" + Message + "]");                
           }
        }

        public void eventha()
        {
            // Verificamos que haya más de 1 parámetros en la formulación del comando.
            if (Params.Length > 1)
            {
                // Mensaje de la alerta.
                string Message = MergeParams(Params, 1);

                    // Paquete de la notificación.
                    ServerMessage Alert = new ServerMessage(Outgoing.CustomAlert);
                    Alert.AppendString("furni_placement_error");
                    Alert.AppendInt32(2);
                    Alert.AppendString("message");
                    Alert.AppendString(Message + LanguageLocale.GetValue("eventha.ir"));
                    Alert.AppendString("image");
                    Alert.AppendString("${image.library.url}notifications/" + EmuSettings.EVENTHA_ICON + ".png");
                    OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(Alert);

                    // Asignamos el RoomId del evento.
                    OtanixEnvironment.GetGame().RoomIdEvent = Session.GetHabbo().CurrentRoomId;

                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, Convert.ToString(TargetRoom.Id), "Alertas", "Usou o eventha, mensagem: " + Message);

            }
            else
            {
                // No hay evento en ninguna sala del hotel.
                OtanixEnvironment.GetGame().RoomIdEvent = 0;
            }
        }

        public void ganhouEvento()
        {
            // Verificamos que tenha 2 parametros
            if (Params.Length == 2)
            {
                // Nome do usuário.
                string Usuario = Params[1];

                 // GameClient do usuario
                GameClient TargetClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Usuario);

                // Se não está conectado
                if (TargetClient == null || TargetClient.GetHabbo() == null)
                {
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                    return;
                }
                // enviamos as moedas pro ganhador
                TargetClient.GetHabbo().GiveUserDiamonds(2);
                // avisamos ele
                TargetClient.SendNotif("Você acaba de receber 2 diamantes e 1 ponto de eventos!");

                    if (TargetClient.GetHabbo().Rank >= 0)
                    {
                        using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.setQuery("UPDATE users SET premiar = premiar + '1' WHERE username = @username");
                            dbClient.addParameter("username", Usuario);
                            dbClient.runQuery();
                        }
                    }
                    else
                    {
                        TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                        return;
                    }



                    uint UserId = UsersCache.getIdByUsername(Usuario);

                        string niveldoemblema = null;
                        DataRow nivel;
                        using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.setQuery("SELECT premiar FROM users WHERE username = '" + Usuario + "'");
                            nivel = dbClient.getRow();
                            niveldoemblema = Convert.ToString(nivel["Premiar"]);

                           if (TargetClient.GetHabbo().Rank >= 0)
                           {
                                string BadgeCode = "NV" + niveldoemblema;
                                using (IQueryAdapter dbClsient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                                {
                                    dbClsient.setQuery("REPLACE INTO user_badges VALUES ('" + UserId + "',@badge,'0','0')");
                                    dbClsient.addParameter("badge", BadgeCode);
                                    dbClsient.runQuery();
                                }
                                TargetClient.GetHabbo().GetBadgeComponent().GiveBadge(OtanixEnvironment.FilterInjectionChars(BadgeCode));
                                TargetClient.SendWindowManagerAlert(LanguageLocale.GetValue("badge.receive1") + BadgeCode + LanguageLocale.GetValue("badge.receive2"));
                           }
                           else
                           {
                           TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("ganhouevento.erro1"));
                           }
                        }

                // Pega o visual do usuário
                string Imagem = TargetClient.GetHabbo().Look;
                string avatarGerado = "";
                WebClient wc = new WebClient();
                try
                {
                    avatarGerado = wc.DownloadString(LanguageLocale.GetValue("api.base.url") + Imagem + "&usuario=" + TargetClient.GetHabbo().Username);
                    avatarGerado = "thiago";
                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine("Ganhou Evento erro #1: " + e.Message);
                    avatarGerado = "erro";
                }
                catch (WebException e)
                {
                    Console.WriteLine("Ganhou Evento erro #2: " + e.Message);
                    avatarGerado = "erro";
                }
                catch (NotSupportedException e)
                {
                    Console.WriteLine("Ganhou Evento erro #3: " + e.Message);
                    avatarGerado = "erro";
                }

                if (avatarGerado == "erro")
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("ganhouevento.erro1"));
                else if(avatarGerado == "thiago"){

                    Random rnd = new Random();
                    int rndd = rnd.Next(1,999);
                    ServerMessage Alert = new ServerMessage(Outgoing.CustomAlert);
                    Alert.AppendString("furni_placement_error");
                    Alert.AppendInt32(2);
                    Alert.AppendString("message");
                    Alert.AppendString(LanguageLocale.GetValue("user.theuser") + Usuario + LanguageLocale.GetValue("eventos.win"));
                    Alert.AppendString("image");
                    Alert.AppendString(LanguageLocale.GetValue("api.base.url.avatares") + "premiar" + ".png");
                    OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(Alert);

                    OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, TargetClient.GetHabbo().Username, "Eventos", "Alertou que o usuário ganhou o evento");

                }
            }
        }

        public void notification()
        {
            // Verificamos que haya más de 2 parámetros en la formulación del comando.
            if (Params.Length > 2)
            {
                // Nombre de imagen.
                string Image = Params[1];

                // Mensaje de la alerta.
                string Message = MergeParams(Params, 2);

                // Paquete de la notificación.
                ServerMessage Alert = new ServerMessage(Outgoing.CustomAlert);
                Alert.AppendString("furni_placement_error");
                Alert.AppendInt32(2);
                Alert.AppendString("message");
                Alert.AppendString(Message);
                Alert.AppendString("image");
                Alert.AppendString("${image.library.url}notifications/" + Image + ".png");
                OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(Alert);
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Alertas", "Alertou o hotel, mensagem: " + Message);

            }
        }

        public void explicaEvento()
        {
            // Verificamos que possui 2 parametros no comando
            if (Params.Length == 2)
            {
                // ID do evento.
                string Evento = Params[1];

                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("SELECT frases FROM otanix_emulador_eventos WHERE id = @id OR tipo = @id");
                    dbClient.addParameter("id", Evento);
                    var TotalFrases = dbClient.getRow();

                    if (TotalFrases == null)
                        return;

                    string frases = (string)TotalFrases["frases"];

                    foreach(string frase in frases.Split('/'))
                    {
                        if (frase.Length > 100)
                            continue;
                        
                        TargetRoomUser.Chat(TargetRoomUser.GetClient(), frase, 33, true);
                        OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Eventos", "Usou o explicaevento, tipo: " + Evento);

                    }
                }
            }
        }


        public void staffDiamonds()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Nombre de usuario.
                string Username = Params[1];

                // GameClient del usuario.
                GameClient TargetClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);

                // Si no está conectado.
                if (TargetClient == null)
                {
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                    return;
                }

                // Damos los diamantes al usuario.
                TargetClient.GetHabbo().GiveUserDiamonds(10);

                // Notificamos al usuario.
                TargetClient.SendNotif(LanguageLocale.GetValue("diamonds.give"));

                // Guardamos la acción en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, TargetClient.GetHabbo().Username, "StaffDiamonds", "StaffDiamonds amount [10]");
            }
        }

        public void hotelalert()
        {
            // Verificamos que haya más de 1 parámetros en la formulación del comando.
            if (Params.Length > 1)
            {
                // Mensaje de la alerta.
                string Notice = MergeParams(Params, 1);

                // Paquete de la notificación.
                ServerMessage newm = new ServerMessage(Outgoing.GeneratingNotification);
                newm.AppendString("info." + EmuSettings.HOTEL_LINK);
                newm.AppendInt32(5);
                newm.AppendString("image");
                newm.AppendString(LanguageLocale.GetValue("ha.imagem"));
                newm.AppendString("title");
                newm.AppendString(LanguageLocale.GetValue("ha.title"));
                newm.AppendString("message");
                newm.AppendString("<i>" +Notice+ "<br><br>• <b>"+ Session.GetHabbo().Username + "</b></i>");
                newm.AppendString("linkTitle");
                newm.AppendString(LanguageLocale.GetValue("ha.button"));
                newm.AppendString("linkUrl");
                newm.AppendString("event:");
                OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(newm);

                // Guardamos la acción en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Alertas", "Hotel alert [" + Notice + "]");
            }
        }

        public void visa()
        {
            // Verificamos que haya más de 1 parámetros en la formulación del comando.
            if (Params.Length > 1)
            {
                // Mensaje de la alerta.
                string Message = MergeParams(Params, 1);

                // Paquete de la alerta.
                ServerMessage newm = new ServerMessage(Outgoing.GeneratingNotification);
                newm.AppendString("info." + LanguageLocale.GetValue("hotel.link"));
                newm.AppendInt32(5);
                newm.AppendString("image");
                newm.AppendString(LanguageLocale.GetValue("visa.image"));
                newm.AppendString("title");
                newm.AppendString(LanguageLocale.GetValue("visa.title"));
                newm.AppendString("message");
                newm.AppendString(LanguageLocale.GetValue("visa.body1") + Session.GetHabbo().Username + LanguageLocale.GetValue("visa.body2") + Message + LanguageLocale.GetValue("visa.body3"));
                newm.AppendString("linkTitle");
                newm.AppendString(LanguageLocale.GetValue("visa.button"));
                newm.AppendString("linkUrl");
                newm.AppendString("event:navigator/goto/" + Session.GetHabbo().CurrentRoomId);
                OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(newm);

                // Guardamos la acción en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "VisaHotelAlert", "VisaHotel alert [" + Message + "]");
            }
        }

        public void darMoedas()
        {
            // Verificamos que haya 3 parámetros en la formulación del comando.
            if (Params.Length == 3)
            {
                // Nombre de Usuario.
                string Username = Params[1];

                // Variable diamantes.
                int Diamonds = 0;

                // Obtenemos el valor establecido por el Staff.
                if (!int.TryParse(Params[2], out Diamonds))
                {
                    // Notificamos al usuario.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.intonly"));
                    return;
                }

                // GameClient del Usuario.
                GameClient TargetClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);

                // Si está desconectado.
                if (TargetClient == null)
                {
                    // Si existe el usuario.
                    if (UsersCache.getIdByUsername(Username) > 0)
                    {
                        // Actualizamos los diamantes en la DB.
                        using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.setQuery("UPDATE users SET moedas = moedas + '" + Diamonds + "' WHERE username = @username");
                            dbClient.addParameter("username", Username);
                            dbClient.runQuery();
                        }
                    }
                    // Si no existe el usuario.
                    else
                    {
                        // Notificamos al usuario.
                        TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                        return;
                    }
                }
                else
                {
                    // Entregamos los diamantes al usuario conectado.
                    TargetClient.GetHabbo().darMoedas(Diamonds);

                    // Notificamos al usuario.
                    TargetClient.SendWindowManagerAlert(LanguageLocale.GetValue("user.receive.diamonds1") + Diamonds + LanguageLocale.GetValue("hotel.lucrativo.msg2"));
                }

                // Notificamos al administrador.
                TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("user.theuser") + Username + LanguageLocale.GetValue("user.diamonds.receives") + Diamonds + " moedas.");

                // Guardamos la acción en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, Username, "Moedas", "Deu moedas [" + Diamonds + "]");
            }
        }

        public void givePiruleta()
        {
            // Verificamos que haya 3 parámetros en la formulación del comando.
            if (Params.Length == 3)
            {
                // Nombre de Usuario.
                string Username = Params[1];

                // Variable piruleta.
                int piruleta = 0;

                // Obtenemos el valor establecido por el Staff.
                if (!int.TryParse(Params[2], out piruleta))
                {
                    // Notificamos al usuario.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.intonly"));
                    return;
                }

                // GameClient del Usuario.
                GameClient TargetClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);

                // Si está desconectado.
                if (TargetClient == null)
                {
                    // Si existe el usuario.
                    if (UsersCache.getIdByUsername(Username) > 0)
                    {
                        // Actualizamos los diamantes en la DB.
                        using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.setQuery("UPDATE users SET coins_purchased = coins_purchased + '" + piruleta + "' WHERE username = @username");
                            dbClient.addParameter("username", Username);
                            dbClient.runQuery();
                        }
                    }
                    // Si no existe el usuario.
                    else
                    {
                        // Notificamos al usuario.
                        TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                        return;
                    }
                }
                else
                {
                    // Entregamos los diamantes al usuario conectado.
                    TargetClient.GetHabbo().GiveUserPiruleta(piruleta);

                    // Notificamos al usuario.
                    TargetClient.SendWindowManagerAlert(LanguageLocale.GetValue("user.receive.diamonds1") + piruleta + LanguageLocale.GetValue("user.receibe.diamonds2") + Session.GetHabbo().Username);
                }

                // Notificamos al administrador.
                TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("user.theuser") + Username + LanguageLocale.GetValue("user.diamonds.receives") + piruleta + " piruletas.");

                // Guardamos la acción en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, Username, "Diamonds", "Diamonds amount [" + piruleta + "]");
            }
        }
        public void giveDiamonds()
        {
            // Verificamos que haya 3 parámetros en la formulación del comando.
            if (Params.Length == 3)
            {
                // Nombre de Usuario.
                string Username = Params[1];

                // Variable diamantes.
                int Diamonds = 0;

                // Obtenemos el valor establecido por el Staff.
                if (!int.TryParse(Params[2], out Diamonds))
                {
                    // Notificamos al usuario.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.intonly"));
                    return;
                }

                // GameClient del Usuario.
                GameClient TargetClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);

                // Si está desconectado.
                if (TargetClient == null)
                {
                    // Si existe el usuario.
                    if (UsersCache.getIdByUsername(Username) > 0)
                    {
                        // Actualizamos los diamantes en la DB.
                        using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                        {
                            dbClient.setQuery("UPDATE users SET diamonds = diamonds + '" + Diamonds + "' WHERE username = @username");
                            dbClient.addParameter("username", Username);
                            dbClient.runQuery();
                        }
                    }
                    // Si no existe el usuario.
                    else
                    {
                        // Notificamos al usuario.
                        TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                        return;
                    }
                }
                else
                {
                    // Entregamos los diamantes al usuario conectado.
                    TargetClient.GetHabbo().GiveUserDiamonds(Diamonds);

                    // Notificamos al usuario.
                    TargetClient.SendWindowManagerAlert(LanguageLocale.GetValue("user.receive.diamonds1") + Diamonds + LanguageLocale.GetValue("user.receibe.diamonds2") + Session.GetHabbo().Username);
                }

                // Notificamos al administrador.
                TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("user.theuser") + Username + LanguageLocale.GetValue("user.diamonds.receives") + Diamonds + " diamantes.");

                // Guardamos la acción en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, Username, "Diamonds", "Diamonds amount [" + Diamonds + "]");
            }
        }

        public void roomDiamonds()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Variable de diamantes.
                int DiamondsAmount = 0;

                // Obtenemos el valor establecido por el Staff.
                if (!int.TryParse(Params[1], out DiamondsAmount))
                {
                    // Notificamos al usuario.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.intonly"));
                    return;
                }

                // Encolamos los diamantes a los usuarios de sala.
                TargetRoom.QueueRoomDiamonds(DiamondsAmount);

                // Paquete de la alerta que recibirán los usuarios.
                ServerMessage nMessage = new ServerMessage(Outgoing.WindowManagerAlert);
                nMessage.AppendInt32(0);
                nMessage.AppendString(LanguageLocale.GetValue("user.diamondsreceived") + " " + DiamondsAmount + LanguageLocale.GetValue("diamonds.receive"));
                TargetRoom.SendMessage(nMessage);

                // Guardamos la acción en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Diamonds", "Roomdiamonds in room [" + TargetRoom.RoomId + "] with diamonds [" + Params[1] + "]");
            }
        }
        
        public void giveRoomPiruleta()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Variable de diamantes.
                int DiamondsAmount = 0;

                // Obtenemos el valor establecido por el Staff.
                if (!int.TryParse(Params[1], out DiamondsAmount))
                {
                    // Notificamos al usuario.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.intonly"));
                    return;
                }

                // Encolamos los diamantes a los usuarios de sala.
                TargetRoom.QueueRoomPiruletas(DiamondsAmount);

                // Paquete de la alerta que recibirán los usuarios.
                ServerMessage nMessage = new ServerMessage(Outgoing.WindowManagerAlert);
                nMessage.AppendInt32(0);
                nMessage.AppendString(LanguageLocale.GetValue("user.diamondsreceived") + " " + DiamondsAmount + " piruletas");
                TargetRoom.SendMessage(nMessage);

                // Guardamos la acción en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Moedas", "Enviou moedas no quarto [" + TargetRoom.RoomId + "] quantidade [" + Params[1] + "]");
            }
        }
        public void roomMoedas()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Variable de diamantes.
                int DiamondsAmount = 0;

                // Obtenemos el valor establecido por el Staff.
                if (!int.TryParse(Params[1], out DiamondsAmount))
                {
                    // Notificamos al usuario.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.intonly"));
                    return;
                }

                // Encolamos los diamantes a los usuarios de sala.
                TargetRoom.QueueRoomCredits(DiamondsAmount);

                // Paquete de la alerta que recibirán los usuarios.
                ServerMessage nMessage = new ServerMessage(Outgoing.WindowManagerAlert);
                nMessage.AppendInt32(0);
                nMessage.AppendString(LanguageLocale.GetValue("user.diamondsreceived") + " " + DiamondsAmount + " moedas");
                TargetRoom.SendMessage(nMessage);

                // Guardamos la acción en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Moedas", "Enviou moedas no quarto [" + TargetRoom.RoomId + "] quantidade [" + Params[1] + "]");
            }
        }

        public void globalDiamonds()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Variable de diamantes.
                int DiamondsAmount = 0;

                // Obtenemos el valor establecido por el Staff.
                if (!int.TryParse(Params[1], out DiamondsAmount))
                {
                    // Notificamos al usuario.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.intonly"));
                    return;
                }

                // Encolamos los diamantes a los usuarios online.
                OtanixEnvironment.GetGame().GetClientManager().QueueDiamondsUpdate(DiamondsAmount);

                // Paquete de la alerta que recibirán los usuarios.
                ServerMessage nMessage = new ServerMessage(Outgoing.SendNotif);
                nMessage.AppendString(LanguageLocale.GetValue("user.diamondsreceived") + ": " + DiamondsAmount + " diamantes.\r\n" + "- " + Session.GetHabbo().Username);
                nMessage.AppendString("");
                OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(nMessage);

                // Guardamos la acción en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Global Diamonds", "Send [" + DiamondsAmount + "] diamonds to every online");
            }
        }

        public void globalMoedas()
        {
            // Verifica se tem 2 parametros
            if (Params.Length == 2)
            {
                // Variavel das moedas
                int qtdMoedas = 0;

                // Obtemos a quantidade de moedas enviada pelo staff
                if (!int.TryParse(Params[1], out qtdMoedas))
                {
                    // Notifica o usuário
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.intonly"));
                    return;
                }

                // Colocamos na queue para enviar as moedas pras pessoas
                OtanixEnvironment.GetGame().GetClientManager().QueueMoedasUpdate(qtdMoedas);

                // Avisa os usuários
                ServerMessage nMessage = new ServerMessage(Outgoing.SendNotif);
                nMessage.AppendString(LanguageLocale.GetValue("user.diamondsreceived") + ": " + qtdMoedas + " moedas.\r\n" + "- " + Session.GetHabbo().Username);
                nMessage.AppendString("");
                OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(nMessage);

                // Salvamos o log disso
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Moedas Global", "Enviou [" + qtdMoedas + "] moedas para todos online");
            }
        }

        public void roomaction()
        {
            // Verificamos que haya 2 o más parámetros en la formulación del comando.
            if (Params.Length >= 2)
            {
                // Comando.
                string CommandOrder = Params[1];

                switch (CommandOrder)
                {
                    case "wave":
                        {
                            // Obtiene la lista con los usuarios actuales en la sala.
                            List<RoomUser> usersinroom = TargetRoom.GetRoomUserManager().GetRoomUsers();

                            foreach (RoomUser User in usersinroom)
                            {
                                // Packet con la acción.
                                ServerMessage Message = new ServerMessage(Outgoing.Action);
                                Message.AppendInt32(User.VirtualId);
                                Message.AppendInt32(1);
                                TargetRoom.SendMessage(Message);
                            }

                            break;
                        }
                    case "bots":
                        {
                            int quantidadeBots = 1;

                            if (Params.Length != 3)
                                return;

                            int.TryParse(Params[2], out quantidadeBots);

                            if (quantidadeBots < 1 || quantidadeBots > 400)
                                return;

                            for (int i = 1; i <= quantidadeBots; i++)
                            {
                                TargetRoom.GetRoomUserManager().DeployBot(new RoomBot(0, 999999, Session.GetHabbo().CurrentRoomId, AIType.Generic, true, Session.GetHabbo().Username,
                                Session.GetHabbo().Motto, Session.GetHabbo().Gender, Session.GetHabbo().Look, TargetRoomUser.X, TargetRoomUser.Y, Convert.ToInt32(TargetRoomUser.Z), TargetRoomUser.RotHead, true, "", 0, false), null);
                            }
                                 //                           RoomUser BotUser = Room.GetRoomUserManager().DeployBot(new RoomBot(0, 999999, Session.GetHabbo().CurrentRoomId, AIType.Frank, true, "Frank",
                                 //"Ajudante do Hotel", "M", "hr-3194-38-36.hd-180-1.ch-220-1408.lg-285-73.sh-906-90.ha-3129-73.fa-1206-73.cc-3039-73", 0, 0, 0, 0, true, "", 0, false), null);
                            break;
                        }
                    
                    case "dance":
                        {
                            // Tipo de baile.
                            int DanceId = 1;

                            if (Params.Length == 3)
                                int.TryParse(Params[2], out DanceId);

                            // Tipo de baile inválido.
                            if (DanceId < 1 || DanceId > 4)
                                return;

                            // Obtiene la lista con los usuarios actuales en la sala.
                            List<RoomUser> usersinroom = TargetRoom.GetRoomUserManager().GetRoomUsers();

                            foreach (RoomUser User in usersinroom)
                            {
                                User.DanceId = DanceId;

                                // Packet con la acción.
                                ServerMessage DanceMessage = new ServerMessage(Outgoing.Dance);
                                DanceMessage.AppendInt32(User.VirtualId);
                                DanceMessage.AppendInt32(DanceId);
                                TargetRoom.SendMessage(DanceMessage);
                            }

                            break;
                        }

                    case "sign":
                        {
                            // Tipo de signo.
                            int SignId = -1;

                            // Signo que ha dicho el Administrador.
                            if (Params.Length == 3)
                                int.TryParse(Params[2], out SignId);

                            // Si no es un signo válido.
                            if (SignId < 0 || SignId > 17)
                                return;

                            // Variable de usuarios en sala.
                            List<RoomUser> usersinroom = TargetRoom.GetRoomUserManager().GetRoomUsers();
                            foreach (RoomUser User in usersinroom)
                            {
                                // Ponemos el signo.
                                User.AddStatus("sign", Convert.ToString(SignId));
                                User.UpdateNeeded = true;
                            }

                            break;
                        }

                    case "say":
                        {
                            // Mensaje que dirán los usuarios.
                            string Message = MergeParams(Params, 2);

                            // Si hay mensaje.
                            if (!string.IsNullOrEmpty(Message))
                            {
                                // Variable de usuarios en sala.
                                List<RoomUser> usersinroom = TargetRoom.GetRoomUserManager().GetRoomUsers();
                                foreach (var User in usersinroom)
                                {
                                    // El usuario habla.
                                    User.Chat(User.GetClient(), Message, 0, false);
                                }
                            }

                            break;
                        }

                    case "shout":
                        {
                            // Mensaje que dirán los usuarios.
                            string Message = MergeParams(Params, 2);

                            // Si hay mensaje.
                            if (!string.IsNullOrEmpty(Message))
                            {
                                // Variable de usuarios en sala.
                                List<RoomUser> usersinroom = TargetRoom.GetRoomUserManager().GetRoomUsers();
                                foreach (var User in usersinroom)
                                {
                                    // El usuario habla.
                                    User.Chat(User.GetClient(), Message, 0, true);
                                }
                            }

                            break;
                        }

                    case "enable":
                        {
                            // Tipo de Enable.
                            int EnableId = 0;

                            // Signo que ha dicho el Administrador.
                            if (Params.Length == 3)
                                int.TryParse(Params[2], out EnableId);

                            // Enable nulo.
                            if (EnableId == 0)
                                return;

                            // Variable de usuarios en sala.
                            List<RoomUser> usersinroom = TargetRoom.GetRoomUserManager().GetRoomUsers();
                            foreach (RoomUser User in usersinroom)
                            {
                                // Ponemos el enable.
                                User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(EnableId);
                            }

                            break;
                        }

                    case "freeze":
                        {
                            // Usuarios no quarto
                            List<RoomUser> usuariosNoQuarto = TargetRoom.GetRoomUserManager().GetRoomUsers();

                            foreach(RoomUser User in usuariosNoQuarto)
                            {
                                if(!User.CanWalk && User.comandoFreeze)
                                {
                                    User.CanWalk = true;
                                    User.comandoFreeze = false;
                                    User.ApplyEffect(User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().BackupEffect);
                                }else
                                {
                                    User.CanWalk = false;
                                    User.comandoFreeze = true;
                                    User.ApplyEffect(12);
                                }
                            }
                            break;
                        }

                    case "action":
                        {
                            // Tipo de Enable.
                            int ActionId = 0;

                            // Signo que ha dicho el Administrador.
                            if (Params.Length == 3)
                                int.TryParse(Params[2], out ActionId);

                            // Enable nulo.
                            if (ActionId < 1 || ActionId > 5)
                                return;

                            // Variable de usuarios en sala.
                            List<RoomUser> usersinroom = TargetRoom.GetRoomUserManager().GetRoomUsers();
                            foreach (RoomUser User in usersinroom)
                            {
                                // Ponemos la acción.
                                ServerMessage Message = new ServerMessage(Outgoing.Action);
                                Message.AppendInt32(User.VirtualId);
                                Message.AppendInt32(ActionId);
                                TargetRoom.SendMessage(Message);

                                if (ActionId == 5) // idle
                                {
                                    User.IsAsleep = true;

                                    ServerMessage FallAsleep = new ServerMessage(Outgoing.IdleStatus);
                                    FallAsleep.AppendInt32(User.VirtualId);
                                    FallAsleep.AppendBoolean(User.IsAsleep);
                                    TargetRoom.SendMessage(FallAsleep);
                                }
                            }

                            break;
                        }
                    case "fastwalk":
                        {
                            // Obtiene la lista con los usuarios actuales en la sala.
                            List<RoomUser> usersinroom = TargetRoom.GetRoomUserManager().GetRoomUsers();

                            foreach (RoomUser User in usersinroom)
                            {
                                User.fastWalk = !User.fastWalk;
                            }
                        }
                        break;

                    default:
                        Session.SendWindowManagerAlert(LanguageLocale.GetValue("comandos.quartos.errado") + ":roomaction\":\n\r- wave\n -dance\n - sign\n - say\n - shout\n - enable\n - freeze\n - action\n - fastwalk");
                        break;
                }

                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Roomaction", "Usou o roomaction, tipo: " + CommandOrder);

            }
        }

        public void massbadge()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Código de placa.
                string BadgeCode = Params[1];

                // Damos la placa a la sala.
                OtanixEnvironment.GetGame().GetClientManager().QueueBadgeUpdate(BadgeCode);

                // Guardamos la acción en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Badge", "Mass badge with badge [" + BadgeCode + "]");
            }
        }

        public void unban()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Nombre de Usuario.
                string Username = Params[1];

                // Desbaneamos al usuario.
                OtanixEnvironment.GetGame().GetBanManager().UnbanUser(Username);

                // Guardamos la acción en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, Username, "Unban", "Unban user.");

                // Notificamos al usuario.
                TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("user.theuser") + Username + LanguageLocale.GetValue("user.banned"));
            }
        }

        public void staffinfo()
        {
            // Creamos una lista para los Staffs.
            Dictionary<Habbo, uint> clients = new Dictionary<Habbo, uint>();

            // String del mensaje.
            StringBuilder content = new StringBuilder();

            // Añadimos esto al mensaje.
            content.Append(LanguageLocale.GetValue("staff.on") + EmuSettings.HOTEL_LINK + ":\r\n");

            // Cogemos los usuarios que sean staffs.
            foreach (GameClient client in OtanixEnvironment.GetGame().GetClientManager().clients.Values)
            {
                // Si está online y es más de rango 3.
                if (client != null && client.GetHabbo() != null && client.GetHabbo().Rank > 3)
                    clients.Add(client.GetHabbo(), client.GetHabbo().Rank);
            }

            // Foreach de los Staffs.
            foreach (KeyValuePair<Habbo, uint> client in clients.OrderBy(key => key.Value))
            {
                // Añadimos la información.
                content.Append(client.Key.Username + "(Rank: " + client.Key.Rank + ") - Quarto: " + ((client.Key.CurrentRoom == null) ? "Nenhum." : client.Key.CurrentRoom.RoomData.Name) + " - Tickets lidos/pulados: " + client.Key.readTickets + "/" + client.Key.skippedTickets + "\r\n");
            }

            // Enviamos la alerta del mensaje.
            Session.SendScrollNotif(content.ToString());

            // Guardamos la acción en Logs.
            OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "StaffInfo", "Required staffs status.");

            // Limpiamos los datos.
            clients.Clear();
            clients = null;
        }

        public void QuickPoll()
        {
            // Verificamos que haya igual o más de 3 parámetros en la formulación del comando.
            if (Params.Length >= 3)
            {
                // Variable tiempo encuesta.
                uint Time = 0;

                // Obtenemos el tiempo de la encuesta.
                if (!uint.TryParse(Params[1], out Time))
                {
                    // Notificamos al administrador.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.intonly"));
                    return;
                }

                // Pregunta de la encuesta.
                string Question = MergeParams(Params, 2);

                // Si se ha obtenido una pregunta.
                if (!string.IsNullOrEmpty(Question))
                {
                    // Si ya hay un tipo de encuesta.
                    if (TargetRoom.GotRoomPoll())
                    {
                        // Si la encuesta que hay es un Cuestionario.
                        if (TargetRoom.GetRoomPoll().GetPollType() == PollType.ROOM_QUESTIONARY)
                        {
                            // Notificamos la administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("room.questionary.error"));
                            return;
                        }
                    }

                    // Creamos la QuickPoll.
                    TargetRoom.CreateNewQuestionary(PollType.VOTE_QUESTIONARY);

                    // Enviamos el paquet de la encuesta.
                    VoteQuestionary handler = (VoteQuestionary)TargetRoom.GetRoomPoll();
                    handler.LoadQuestionary(TargetRoom.Id);
                    handler.LoadInformation(Question, Time);
                    TargetRoom.SendMessage(handler.SerializePoll());
                }
            }
            else
            {
                if (TargetRoom.GotRoomPoll() && TargetRoom.GetRoomPoll().GetPollType() == PollType.VOTE_QUESTIONARY)
                {
                    TargetRoom.SendMessage(TargetRoom.GetRoomPoll().ClearInformation());
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("quickpoll.clear"));
                }
            }
        }

        public void viewinventary()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Palabra que añadiremos al filtro.
                string Username = Params[1];

                GameClient Client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
                if (Client != null)
                {
                    // Notificamos al usuario.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("user.online"));
                    return;
                }

                // Id del usuario.
                uint UserId = UsersCache.getIdByUsername(Username);
                if(UserId == 0)
                {
                    // Notificamos al usuario.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                    return;
                }

                // Cargamos el inventario.
                Session.GetHabbo().GetInventoryComponent().LoadUserInventory(UserId);
                Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializeItemInventory());

                // Notificamos al usuario.
                TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("inventory.change") + Username);

                // Guardamos en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, Username, "ViewInventory", "ViewHabboInventory of " + Username);
            }
            else
            {
                // Cargamos el inventario.
                Session.GetHabbo().GetInventoryComponent().LoadUserInventory(0);
                Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializeItemInventory());

                // Notificamos al usuario.
                TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("inventory.change.again"));
            }
        }

        public void addFilter()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Palabra que añadiremos al filtro.
                string word = Params[1].ToLower();

                // La insertamos en la DB.
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("REPLACE INTO server_blackwords VALUES (@word, 'hotel')");
                    dbClient.addParameter("word", word);
                    dbClient.runQuery();
                }

                // La insertamos en caché del emulador.
                BlackWordsManager.AddPrivateBlackWord("hotel", word);

                // Guardamos la acción en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, "", "AddFilter", "Añadida la palabra " + word + " al filtro global.");

                // Notificamos al Administrador.
                TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("word.add") + word + LanguageLocale.GetValue("word.add2"));
            }
        }

        public void say()
        {
            // Verificamos que haya igual o más de 3 parámetros en la formulación del comando.
            if (Params.Length >= 3)
            {
                // Nombre de usuario.
                string Username = Params[1];

                // Mensaje.
                string Message = MergeParams(Params, 2);

                // GameClient del Usuario.
                GameClient TargetClient = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);

                // Si está desconectado.
                if (TargetClient == null)
                {
                    // Notificamos al Administrador.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                    return;
                }

                // Si no está en sala.
                if (TargetClient.GetHabbo().InRoom == false)
                    return;

                // Sala del Usuario.
                Room Room = TargetClient.GetHabbo().CurrentRoom;
                if (Room == null)
                    return;

                // RoomUser del Usuario.
                RoomUser RoomUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Username);
                if (RoomUser == null)
                    return;

                // El usuario habla.
                RoomUser.Chat(TargetClient, Message, 0, false);

                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, TargetClient.GetHabbo().Username, "Makesay", "Fez o usuário falar a frase: " + Message);

            }
        }

        public void massfurni()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Variable ItemId.
                uint ItemId = 0;

                // Obtenemos el ItemID que ha dicho el Administrador.
                if (!uint.TryParse(Params[1], out ItemId))
                {
                    // Notificamos al Administrador.
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.intonly"));
                    return;
                }

                // Lista de todos los usuarios online.
                foreach (GameClient Client in OtanixEnvironment.GetGame().GetClientManager().clients.Values)
                {
                    // Si tienen el inventario disponible.
                    if (Client != null && Client.GetHabbo() != null && Client.GetHabbo().GetInventoryComponent() != null)
                    {
                        // Añadimos el Item.
                        Client.GetHabbo().GetInventoryComponent().AddNewItem(0, ItemId, "", true, false, false, "", Client.GetHabbo().Id, 0);
                        Client.GetHabbo().GetInventoryComponent().UpdateItems(false);

                        // Notificamos al usuario.
                        Client.SendWindowManagerAlert(LanguageLocale.GetValue("mobi.receive"));
                    }
                }

                // Guardamos la acción en Logs.
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, "", "MassFurni", "Ha dado el furni " + ItemId + " a todo el hotel.");
            }
        }

        public void invisible()
        {
            // Actualizamos la variable.
            Session.GetHabbo().SpectatorMode = !Session.GetHabbo().SpectatorMode;

            // Notificamos al usuario.
            TargetRoomUser.WhisperComposer(Session.GetHabbo().SpectatorMode ? LanguageLocale.GetValue("invisible.enabled") : LanguageLocale.GetValue("invisible.disabled"));
        }

        public void getStaffs()
        {
            // Obtenemos los usuarios online.
            foreach (GameClient client in OtanixEnvironment.GetGame().GetClientManager().clients.Values)
            {
                // Si son Mayores o iguales que rango 4 y no están en mi sala.
                if (client.GetHabbo() != null && client.GetHabbo().Rank >= 3 && client.GetHabbo().CurrentRoomId != TargetRoom.Id)
                {
                    // Van a la sala en cuestión.
                    client.GetMessageHandler().enterOnRoom3(TargetRoom);
                }
            }
            OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "GetStaffs", "Puxou todos os staffs");

        }

        public void usersOnRooms()
        {
            // Variable de usuarios en sala.
            int usersOnRoom = 0;

            // Miramos todas las salas cargadas.
            foreach (Room room in OtanixEnvironment.GetGame().GetRoomManager().loadedRooms.Values)
            {
                // Sumamos sus usuarios.
                usersOnRoom += room.UserCount;
            }

            // Notificamos al administrador.
            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("users.rooms.count") + usersOnRoom + LanguageLocale.GetValue("users.rooms.count2"));
        }

        public void developerFurnis()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Variable Palabra.
                string Message = Params[1];

                // Switch palabras.
                switch (Message)
                {
                    // Copiamos la baldosa.
                    case "copy":
                        {
                            TargetRoomUser.DeveloperState = 1;
                            break;
                        }

                    // Pegamos la baldosa.
                    case "paste":
                        {
                            if (TargetRoomUser.DeveloperState == 2)
                                TargetRoomUser.DeveloperState = 3;
                            else
                                TargetRoomUser.DeveloperState = 0;

                            break;
                        }
                    
                    // No tiene efecto.
                    default:
                        {
                            TargetRoomUser.DeveloperState = 0;
                            Session.SendNotif(LanguageLocale.GetValue("developer.usar") + "'copy','paste'");
                            break;
                        }
                }
            }
        }

        public void bundle()
        {
            if(OtanixEnvironment.GetGame().GetCatalog().GetPredesignedRooms().criarBundle(TargetRoom))
                Session.SendNotif("O quarto foi colocado a lista de pack do hotel!");
            else
                Session.SendNotif("Erro ao criar o pack!");
        }

        public void refresh()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2 || Params.Length == 3)
            {
                // Comando.
                string CommandOrder = Params[1];

                switch (CommandOrder)
                {
                    case "minhasCoord":
                        {
                            Session.SendNotif("X: " + TargetRoomUser.X + " - Y:" + TargetRoomUser.Y);
                            break;
                        }
                    case "giftsManager":
                        {
                            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                                   OtanixEnvironment.GetGame().GetGiftManager().init(dbClient);
                            break;
                        }
                    case "songs":
                        {
                            SongManager.Initialize();
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("refresh.songs"));
                            break;
                        }
                    case "bonus":
                        {
                            using (var dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                BonusManager.Initialize(dbClient);
                            }
                            TargetRoomUser.WhisperComposer("Bonus atualizados.");
                            break;
                        }
                    case "youtubetv":
                        {
                            // Actualizamos Datos.
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                OtanixEnvironment.GetGame().GetYoutubeManager().Initialize(dbClient);
                            }

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("refresh.yt"));

                            break;
                        }

                    case "navi":
                        {
                            // Actualizamos Datos.
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                OtanixEnvironment.GetGame().GetNavigator().Initialize(dbClient);
                                OtanixEnvironment.GetGame().GetNewNavigatorManager().Initialize(dbClient);
                            }

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("refresh.navi"));

                            break;
                        }
                    case "presos":
                        {
                            // Actualizamos Datos.
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                OtanixEnvironment.GetGame().GetPrisaoManager().Init(dbClient);
                            }
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("refresh.presos"));
                            break;
                        }
                    case "cata":
                        {
                            // Actualizamos Datos.
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                OtanixEnvironment.GetGame().GetCatalog().Initialize(dbClient);
                                OtanixEnvironment.GetGame().GetCatalogPremium().Initialize(dbClient);
                            }

                            OtanixEnvironment.GetGame().GetCatalog().InitCache();

                            // Paquete de actualización.
                            ServerMessage Message = new ServerMessage(Outgoing.UpdateShop);
                            Message.AppendBoolean(false); // timer?
                            OtanixEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(Message);

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("refresh.cata"));

                            break;
                        }

                    case "items":
                        {
                            // Actualizamos Datos.
                            OtanixEnvironment.GetGame().GetItemManager().reloaditems();

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("refresh.items"));

                            break;
                        }

                    case "filter":
                        {
                            // Actualizamos Datos.
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                Filter.BlackWordsManager.Load(dbClient);
                            }

                            Filter.Filter.Initialize();

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("refresh.filter"));

                            break;
                        }

                    case "piñata":
                        {
                            // Actualizamos Datos.
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                OtanixEnvironment.GetGame().GetPiñataManager().Initialize(dbClient);
                            }

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("refresh.filter"));

                            break;
                        }

                    case "bans":
                        {
                            // Actualizamos Datos.
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                OtanixEnvironment.GetGame().GetBanManager().LoadBans(dbClient);
                            }

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("refresh.bans"));

                            break;
                        }

                    case "offers":
                        {
                            // Actualizamos Datos.
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                OtanixEnvironment.GetGame().GetTargetedOfferManager().Initialize(dbClient);
                            }

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("refresh.offers"));

                            break;
                        }

                    case "pets":
                        {
                            // Actualizamos Datos.
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                PetOrders.Init(dbClient);
                                PetRace.Init(dbClient);
                            }

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("refresh.pets"));

                            break;
                        }

                    case "craftables":
                        {
                            // Actualizamos Datos.
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                OtanixEnvironment.GetGame().GetCraftableProductsManager().Initialize(dbClient);
                            }

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("refresh.craft"));

                            break;
                        }

                    case "fuses":
                        {
                            // Actualizamos Datos.
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                OtanixEnvironment.GetGame().GetRoleManager().LoadRanks(dbClient);
                            }

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("refresh.fuses"));

                            break;
                        }

                    case "moderation":
                        {
                            // Actualizamos Datos.
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                OtanixEnvironment.GetGame().GetModerationTool().LoadMessagePresets(dbClient);
                                OtanixEnvironment.GetGame().GetModerationTool().LoadModActions(dbClient);
                            }

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("refresh.moderation"));

                            break;
                        }

                    case "prombadges":
                        {
                            // Actualizamos Datos.
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                OtanixEnvironment.GetGame().GetPromotionalBadges().loadPromotionalBadges(dbClient);
                            }

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("refresh.prombadge"));

                            break;
                        }

                    case "news":
                        {
                            // Actualizamos Datos.
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                OtanixEnvironment.GetGame().GetNewsManager().Initialize(dbClient);
                            }

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("refresh.news"));

                            break;
                        }

                    case "clothing":
                        {
                            // Actualizamos Datos.
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                OtanixEnvironment.GetGame().GetClothingManager().Initialize(dbClient);
                                // OtanixEnvironment.GetGame().GetUserLookManager().Load();
                            }

                            // Notificamos al Administrador.
                            TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("refresh.clothing"));

                            break;
                        }

                    default:
                        {
                            Session.SendWindowManagerAlert(LanguageLocale.GetValue("comandos.quartos.errado") + " \":refresh\":\n\r -bundles\n - giftsManager\n - youtubetv\n - navi\n - cata\n - items\n - bonus\n - filter\n - piñata\n - bans\n - offers\n - pets\n - fuses\n - craftables\n - moderation\n - prombadges\n - news\n - clothing");
                            break;
                        }
                }
                OtanixEnvironment.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().Username, string.Empty, "Refresh", "Atualizou algo: " + CommandOrder);

            }
        }

        public void spyuser()
        {
            // Verificamos que haya 2 parámetros en la formulación del comando.
            if (Params.Length == 2)
            {
                // Nombre de usuario.
                string Username = Params[1];
                uint UserId = UsersCache.getIdByUsername(Username);

                if (UserId == 0)
                {
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("input.usernotfound"));
                    return;
                }

                if (SpyChatMessage.ContainsUser(UserId))
                {
                    SpyChatMessage.RemoveUserToSpy(UserId);
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("not.monitoring.user") + Username);
                }
                else
                {
                    SpyChatMessage.AddUserToSpy(UserId);
                    TargetRoomUser.WhisperComposer(LanguageLocale.GetValue("monitoring.user") + Username);
                }
            }
        }
   
        public void tamanhoChao()
        {
            if (Params.Length == 2)
            {
                double novaAltura;
                if (!Double.TryParse(Params[1], out novaAltura))
                {
                    Session.SendNotif("Altura incorreta, formas de usar: 0/0,1/1.1/10.2/25,55 etc");
                    return;
                }

                if (novaAltura > 150)
                {
                    Session.SendNotif("Altura muito alta, máximo de 150");
                    return;
                }
                Session.GetHabbo().tamanhoChao = novaAltura;
                TargetRoomUser.WhisperComposer("Comando utilizado com sucesso");
            }
        }
        #endregion
    }
}
