using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;
using Butterfly.HabboHotel.Users;
using Butterfly.HabboHotel.Users.UserDataManagement;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ButterStorm
{
    class UsersCache
    {
        /// <summary>
        /// Cachea la clase Habbo mediante el Id de usuario.
        /// </summary>
        private static Dictionary<uint, Habbo> usersHabboCache;

        /// <summary>
        /// Cachea la clase Habbo mediante el nombre de usuario.
        /// </summary>
        private static Dictionary<string, Habbo> usersHabboNameCache;

        /// <summary>
        /// Cachea el Id de usuario mediante el nombre de usuario.
        /// </summary>
        private static Dictionary<string, uint> idByUsername;

        /// <summary>
        /// Cachea el nombre de usuario mediante el id de usuario.
        /// </summary>
        private static Dictionary<uint, string> usernameById;

        /// <summary>
        /// Cachea las salas provisionales del MUS.
        /// </summary>
        private static Dictionary<uint, uint> usersProvisionalHomes;

        /// <summary>
        /// Inicializamos las variables Dictionary de la caché.
        /// </summary>
        internal static void Initialize()
        {
            usersHabboCache = new Dictionary<uint, Habbo>();
            usersHabboNameCache = new Dictionary<string, Habbo>();
            idByUsername = new Dictionary<string, uint>();
            usernameById = new Dictionary<uint, string>();
            usersProvisionalHomes = new Dictionary<uint, uint>();
        }

        /// <summary>
        /// Limpiamos las variables Dictionary de la caché. Este método devuelve el número de usuarios cacheados.
        /// </summary>
        /// <returns></returns>
        internal static Int32 ClearCache()
        {
            int cacheCount = usersHabboCache.Count + usersHabboNameCache.Count + idByUsername.Count + usernameById.Count;

            usersHabboCache.Clear();
            usersHabboNameCache.Clear();
            idByUsername.Clear();
            usernameById.Clear();

            return cacheCount;
        }

        /// <summary>
        /// Obtiene la clase Habbo mediante el Id de usuario.
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <returns></returns>
        internal static Habbo getHabboCache(uint userId)
        {
            GameClient client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(userId);
            if (client != null && client.GetHabbo() != null)
            {
                if (usersHabboCache.ContainsKey(userId))
                    usersHabboCache.Remove(userId);

                return client.GetHabbo();
            }

            if (usersHabboCache.ContainsKey(userId))
                return usersHabboCache[userId];

            Habbo habbo = UserDataFactory.GetUserDataCache(userId);

            if (habbo != null)
            {
                if (!usersHabboCache.ContainsKey(userId))
                    usersHabboCache.Add(userId, habbo);
            }

            return habbo;
        }

        /// <summary>
        /// Obtiene la clase Habbo mediante el nombre de usuario.
        /// </summary>
        /// <param name="username">Nombre de usuario.</param>
        /// <returns></returns>
        internal static Habbo getHabboCache(string username)
        {
            GameClient client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
            if (client != null && client.GetHabbo() != null)
            {
                if (usersHabboNameCache.ContainsKey(username))
                    usersHabboNameCache.Remove(username);

                return client.GetHabbo();
            }

            if (usersHabboNameCache.ContainsKey(username))
                return usersHabboNameCache[username];

            Habbo habbo = UserDataFactory.GetUserDataCache(username);

            if (habbo != null)
            {
                if (!usersHabboNameCache.ContainsKey(username))
                    usersHabboNameCache.Add(username, habbo);
            }

            return habbo;
        }

        /// <summary>
        /// Obtiene el Id de usuario mediante el nombre de usuario.
        /// </summary>
        /// <param name="username">Nombre de usuario.</param>
        /// <returns></returns>
        internal static uint getIdByUsername(string username)
        {
            if (idByUsername.ContainsKey(username))
                return idByUsername[username];

            uint userId = 0;
            GameClient client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
            if (client != null && client.GetHabbo() != null)
            {
                userId = client.GetHabbo().Id;
            }
            else
            {
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("SELECT id FROM users WHERE username = @usrname");
                    dbClient.addParameter("usrname", username);
                    userId = Convert.ToUInt32(dbClient.getInteger());
                }
            }

            if (userId > 0)
            {
                if (!idByUsername.ContainsKey(username))
                    idByUsername.Add(username, userId);
            }

            return userId;
        }

        /// <summary>
        /// Obtiene el nombre de usuario mediante el Id de usuario.
        /// </summary>
        /// <param name="userId">Id del usuario.</param>
        /// <returns></returns>
        internal static string getUsernameById(uint userId)
        {
            if (usernameById.ContainsKey(userId))
                return usernameById[userId];

            string username = "";
            GameClient client = OtanixEnvironment.GetGame().GetClientManager().GetClientByUserID(userId);
            if (client != null && client.GetHabbo() != null)
            {
                username = client.GetHabbo().Username;
            }
            else
            {
                using (IQueryAdapter dbClient = OtanixEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("SELECT username FROM users WHERE id = " + userId);
                    username = dbClient.getString();
                }
            }

            if (username != "")
            {
                if (!usernameById.ContainsKey(userId))
                    usernameById.Add(userId, username);
            }

            return username;
        }

        internal static void AddUserProvisionalRoom(uint UserId, uint RoomId)
        {
            if (usersProvisionalHomes.ContainsKey(UserId))
                usersProvisionalHomes[UserId] = RoomId;
            else
                usersProvisionalHomes.Add(UserId, RoomId);
        }

        internal static void enterProvisionalRoom(GameClient Session)
        {
            if (usersProvisionalHomes.ContainsKey(Session.GetHabbo().Id))
            {
                Room room = OtanixEnvironment.GetGame().GetRoomManager().LoadRoom(usersProvisionalHomes[Session.GetHabbo().Id]);
                if (room == null)
                    return;
 
                Session.GetMessageHandler().enterOnRoom3(room);

                usersProvisionalHomes.Remove(Session.GetHabbo().Id);
            }
        }
    }
}
