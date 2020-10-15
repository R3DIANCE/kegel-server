using AltV.Net;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace kolbenV2
{
    class Chat : IScript
    {
        [ClientEvent("chatMessageToAll")]
        public void chatMessageToAll(Player player, string message)
        {
            foreach (Player client in Alt.GetAllPlayers())
            {
                if (client.LoggedIn == false)
                {
                    return;
                }
                if (player.PlayerName == "Pavjel")
                {
                    client.Emit("sendMessage", $"(Owner) {DateTime.Now.Hour}:{DateTime.Now.Minute} - {player.PlayerName}: {message}");
                }
                else if (player.IsAdmin)
                {
                    client.Emit("sendMessage", $"(Admin) {DateTime.Now.Hour}:{DateTime.Now.Minute} - {player.PlayerName}: {message}");
                }
                else
                {
                    client.Emit("sendMessage", $"{DateTime.Now.Hour}:{DateTime.Now.Minute} - {player.PlayerName}: {message}");
                }
            }
        }

        public static void GlobalMessage(string message)
        {
            foreach (Player player in Alt.GetAllPlayers())
            {
                if (player.LoggedIn == false)
                {
                    return;
                }
                player.SendNotification(message);
            }
        }

        public static void GloabAdminMessage(string mess)
        {
            foreach (Player players in Alt.GetAllPlayers())
            {
                if (players.LoggedIn)
                {
                    players.Emit("admin:message", mess);
                }
            }
        }
    }
}
