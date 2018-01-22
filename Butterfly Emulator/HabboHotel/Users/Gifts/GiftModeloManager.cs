using Database_Manager.Database.Session_Details.Interfaces;
using ButterStorm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Butterfly.Messages;
using HabboEvents;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Premiums;

namespace Butterfly.HabboHotel.Users.Gifts
{
    class GiftModeloManager
    {
        internal Dictionary<uint, GiftModelo> presentesDisponiveis;

        internal void init(IQueryAdapter dbClient)
        {
            presentesDisponiveis = new Dictionary<uint, GiftModelo>();

            dbClient.setQuery("SELECT * FROM nux_gift ORDER BY id ASC LIMIT 3");
            DataTable row = dbClient.getTable();

            if (row == null)
                return;

            foreach(DataRow item in row.Rows)
            {
                presentesDisponiveis.Add((uint)item["id"], new GiftModelo((string) item["imagem"], (uint) item["id"], (string) item["nomeItem"]));
            }
            
        }

        internal Dictionary<uint, GiftModelo> todosItens()
        {
            return presentesDisponiveis;
        }

        internal bool existeItem(uint id)
        {
            return presentesDisponiveis.ContainsKey(id);
        }

        internal void deliverItem(uint idSelecionado, GameClient user)
        {
            if (!existeItem(idSelecionado))
            {
                user.SendNotif("Não foi possível escolher este item, tente novamente.");
                return;
            }

            if (user == null || user.GetHabbo() == null)
                return;

            // Pega o usuário
            uint usuario = user.GetHabbo().Id;

            switch (idSelecionado)
            {
                case 0:
                    {

                        user.GetHabbo().GetBadgeComponent().GiveBadge("BR075");

                        user.SendNotif("Parabens, escolha feita com sucesso!");
                        if (user.GetHabbo().Rank >= 0)
                        {
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                dbClient.setQuery("UPDATE users SET new_identity = '0' WHERE username = @username");
                                dbClient.addParameter("username", user.GetHabbo().Username);
                                dbClient.runQuery();
                            }
                        }

                        user.GetHabbo().NewIdentity = 0;
                        break;
                    }

                case 1:
                    {
                        user.GetHabbo().GetBadgeComponent().GiveBadge("BR074");
                        user.SendNotif("Parabens, escolha feita com sucesso!");
                        if (user.GetHabbo().Rank >= 0)
                        {
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                dbClient.setQuery("UPDATE users SET new_identity = '0' WHERE username = @username");
                                dbClient.addParameter("username", user.GetHabbo().Username);
                                dbClient.runQuery();
                            }
                        }

                        user.GetHabbo().NewIdentity = 0;
                        break;
                    }
                default:
                    {
                        user.GetHabbo().GetBadgeComponent().GiveBadge("BR073");
                        user.SendNotif("Parabens, escolha feita com sucesso!");
                        if (user.GetHabbo().Rank >= 0)
                        {
                            using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                            {
                                dbClient.setQuery("UPDATE users SET new_identity = '0' WHERE username = @username");
                                dbClient.addParameter("username", user.GetHabbo().Username);
                                dbClient.runQuery();
                            }
                        }

                        user.GetHabbo().NewIdentity = 0;
                        break;
                    }

            }
            if (user.GetHabbo().Rank >= 0)
            {
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("UPDATE users SET new_identity = '0' WHERE username = @username");
                    dbClient.addParameter("username", user.GetHabbo().Username);
                    dbClient.runQuery();
                }
            }

            user.GetHabbo().NewIdentity = 0;
        }
        internal ServerMessage Serialize()
        {
            ServerMessage Message = new ServerMessage(Outgoing.NuxItemListComposer);
            Message.AppendUInt(1); // ??
            Message.AppendUInt(4); // ??
            Message.AppendUInt(4); // ??
            Message.AppendInt32(presentesDisponiveis.Count); // Número total de packs:
            foreach(var item in presentesDisponiveis.Values)
            {
                Message.AppendString("gifts/" + item.imagem); // imagem
                Message.AppendInt32(item.quantidadeItensDoPacote); // quantidade de itens desse pacote:
                foreach(var itemPacote in item.nomeItem.Split('/'))
                {
                    Message.AppendString(""); // nome do item (aqui ele busca o texto dentro da swf)
                    Message.AppendString(itemPacote); // nome do item (aqui ele exibe o texto direto, sem ser da swf)
                }
            }

            return Message;
        }
    }
}
