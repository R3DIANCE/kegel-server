using AltV.Net;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace kolbenV2
{
    class Login : IScript
    {
        Database db = new Database();

        [ClientEvent("registerAttemptServer")]
        public void registerAttemptServer(IPlayer player, string username)
        {
            if (username.Length > 12)
            {
                player.Emit("login:notify", "Mehr als 12 Zeichen");
                return;
            }
            if (username == "")
            {
                player.Emit("login:notify", "Du musst einen Namen angeben!");
                return;
            }
            db.Insert($"INSERT INTO accounts (name,socialid) VALUES ('{username}', '{player.SocialClubId}')");
            Alt.Log(player.SocialClubId.ToString());           
            player.Emit("login:notify", "Account erstellt!");
            player.Emit("open:login", username);
        }


        [ClientEvent("loginAttemptServer")]
        public void loginAttemptServer(Player player, string username)
        {
            if (player.LoggedIn)
            {
                return;
            }
            player.LoggedIn = true;
            player.LoadPlayer();
            player.Emit("close:login");
            player.SetTeamSelect();
            if (db.SelectInt($"SELECT * FROM accounts WHERE socialid={player.SocialClubId}", "admin") == 1)
            {
                player.IsAdmin = true;
            }
            foreach(Player client in Alt.GetAllPlayers())
            {
                if (client.LoggedIn == false)
                {
                    return;
                }
                if (client != player)
                {
                    player.Emit("playerListAdd", client.PlayerName);
                }
                client.Emit("playerListAdd", player.PlayerName);
            }
            Chat.GlobalMessage($"{player.PlayerName} ist dem Server beigetreten!");
            Alt.Log("Connect - Total: " + Alt.GetAllPlayers().Count);
            player.SetSyncedMetaData("NAME", player.PlayerName);
        }
    }
}
